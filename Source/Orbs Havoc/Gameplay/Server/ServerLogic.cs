// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Gameplay.Server
{
	using System;
	using Behaviors;
	using Network;
	using Network.Messages;
	using Platform.Logging;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Implements the server logic for handling incoming client messages and the synchronization of client game states.
	/// </summary>
	internal class ServerLogic
	{
		/// <summary>
		///   If tracing is enabled, all server-specific gameplay events are shown in the debug output.
		/// </summary>
		private const bool EnableTracing = false;

		/// <summary>
		///   The allocator that is used to allocate pooled objects.
		/// </summary>
		private readonly PoolAllocator _allocator;

		/// <summary>
		///   The game session that is being played.
		/// </summary>
		private readonly GameSession _gameSession;

		/// <summary>
		///   The allocator for networked identities.
		/// </summary>
		private NetworkIdentityAllocator _networkIdentities = new NetworkIdentityAllocator(UInt16.MaxValue);

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="gameSession">The game session that is being played.</param>
		public ServerLogic(PoolAllocator allocator, GameSession gameSession)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			_allocator = allocator;
			_gameSession = gameSession;

			_gameSession.EntityAdded += OnEntityAdded;
			_gameSession.EntityRemoved += OnEntityRemoved;
		}

		/// <summary>
		///   Gets the number of players currently connected to the game session.
		/// </summary>
		public int PlayerCount => _gameSession.Players.Count;

		/// <summary>
		///   Synchronizes the added entity with all clients.
		/// </summary>
		/// <param name="entity">The entity that has been added.</param>
		private void OnEntityAdded(Entity entity)
		{
			Assert.InRange(entity.Type);

			entity.NetworkIdentity = _networkIdentities.Allocate();
			var message = EntityAddMessage.Create(_allocator, entity);
			_gameSession.Broadcast(message);

			Log.DebugIf(EnableTracing, "(Server) +{1} {0}", message.Entity, message.EntityType);
			entity.OnAdded();
		}

		/// <summary>
		///   Synchronizes the removed entity with all clients.
		/// </summary>
		/// <param name="entity">The entity that has been removed.</param>
		private void OnEntityRemoved(Entity entity)
		{
			Log.DebugIf(EnableTracing, "(Server) -{1} {0}", entity.NetworkIdentity, entity.Type);
			entity.OnRemoved();

			_gameSession.Broadcast(EntityRemoveMessage.Create(_allocator, entity.NetworkIdentity));
			_networkIdentities.Free(entity.NetworkIdentity);
		}

		/// <summary>
		///   Sends a snapshot of the current game state to the given connection.
		/// </summary>
		/// <param name="connection">The connection the state snapshot should be sent to.</param>
		/// <param name="clientPlayer">The player that represents the client.</param>
		public void SendStateSnapshot(Connection connection, Player clientPlayer)
		{
			Assert.ArgumentNotNull(connection, nameof(connection));
			Assert.ArgumentNotNull(clientPlayer, nameof(clientPlayer));

			Log.DebugIf(EnableTracing, "(Server) Sending game state snapshot to {0}, player '{1}' ({2}).",
				connection.RemoteEndPoint, clientPlayer.Name, clientPlayer.Identity);

			// Synchronize all players
			foreach (var player in _gameSession.Players)
			{
				var message = PlayerJoinMessage.Create(_allocator, player);
				connection.EnqueueMessage(message);

				Log.DebugIf(EnableTracing, "(Server)    {0}", message);
			}

			// Synchronize all entities
			foreach (var entity in _gameSession.SceneGraph.EnumeratePreOrder<Entity>())
			{
				var message = EntityAddMessage.Create(_allocator, entity);
				connection.EnqueueMessage(message);

				Log.DebugIf(EnableTracing, "(Server)    {0}", message);
			}

			// Mark the end of the synchronization
			var syncedMessage = ClientSyncedMessage.Create(_allocator, clientPlayer.Identity);
			connection.EnqueueMessage(syncedMessage);

			Log.DebugIf(EnableTracing, "(Server)    Sync completed.");
		}

		/// <summary>
		///   Creates a new player with the given name.
		/// </summary>
		/// <param name="playerName">The name of the new player.</param>
		/// <param name="playerKind">The kind of the new player.</param>
		public Player CreatePlayer(string playerName, PlayerKind playerKind)
		{
			Assert.ArgumentNotNullOrWhitespace(playerName, nameof(playerName));

			var player = Player.Create(_allocator, _gameSession.Players.MakeUniquePlayerName(null, playerName), playerKind);
			_gameSession.Players.Add(player);

			// Broadcast the news about the new player to all clients (this message is not sent to the new client yet)
			_gameSession.Broadcast(PlayerJoinMessage.Create(_allocator, player));

			Log.DebugIf(EnableTracing, "(Server) Created player '{0}' ({1})", playerName, player.Identity);
			return player;
		}

		/// <summary>
		///   Removes the given player from the game session.
		/// </summary>
		/// <param name="player">The player that should be removed.</param>
		public void RemovePlayer(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.That(!player.IsServerPlayer, "Cannot remove the server player.");

			foreach (var entity in _gameSession.SceneGraph.EnumeratePostOrder<Entity>())
			{
				if (entity.Player == player)
					entity.Remove();
			}

			_gameSession.Broadcast(PlayerLeaveMessage.Create(_allocator, player.Identity, player.LeaveReason));
			_gameSession.Players.Remove(player);

			Log.DebugIf(EnableTracing, "(Server) Removed player '{0}' ({1}).", player.Name, player.Identity);
		}

		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="player">The player that generated the input.</param>
		/// <param name="inputMessage">The input that should be handled.</param>
		/// <param name="inputMask">
		///   The input mask that should be used to determine which of the eight state values per input must be
		///   considered.
		/// </param>
		public void HandlePlayerInput(Player player, PlayerInputMessage inputMessage, byte inputMask)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNull(inputMessage, nameof(inputMessage));

			if (player.Orb == null)
				return;

			var primaryWeapon = player.Orb.WeaponEnergyLevels[inputMessage.PrimaryWeapon.GetWeaponSlot()] == 0
				? player.Orb.PrimaryWeapon
				: inputMessage.PrimaryWeapon;

			player.Orb.PlayerInput.HandleInput(
				inputMessage.Target,
				(inputMask & inputMessage.MoveUp) != 0,
				(inputMask & inputMessage.MoveDown) != 0,
				(inputMask & inputMessage.MoveLeft) != 0,
				(inputMask & inputMessage.MoveRight) != 0,
				(inputMask & inputMessage.FirePrimary) != 0 && player.Orb.PrimaryWeapon == primaryWeapon,
				(inputMask & inputMessage.FireSecondary) != 0);

			player.Orb.PrimaryWeapon = primaryWeapon;
		}

		/// <summary>
		///   Respawns the given player.
		/// </summary>
		/// <param name="player">The player that should be respawned.</param>
		public void RespawnPlayer(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentSatisfies(player.Orb == null, nameof(player), "The player cannot be respawned.");

			if (player.RemainingRespawnDelay > 0)
				return;

			// Try to find a player start location that is currently not occupied by any other player; if there
			// is none, don't spawn the player and try again in a later frame (that shouldn't happen at all
			// or at least not often anyway); we'll give up after a couple of retries for this reason...
			const int retries = 16;
			for (var i = 0; i < retries; ++i)
			{
				var startIndex = RandomNumbers.NextIndex(_gameSession.Level.PlayerStarts);
				var startBlockIndex = _gameSession.Level.PlayerStarts[startIndex];
				var startArea = _gameSession.Level.GetBlockArea(startBlockIndex);
				var isOccupied = false;

				foreach (var orb in _gameSession.SceneGraph.EnumeratePostOrder<Orb>())
				{
					var collider = orb.GetBehavior<ColliderBehavior>();
					if (!collider.Circle.Intersects(startArea))
						continue;

					isOccupied = true;
					break;
				}

				if (isOccupied)
					continue;

				Log.DebugIf(EnableTracing, "(Server) Respawning player '{0}' ({1}).", player.Name, player.Identity);
				player.Orb = Orb.Create(_gameSession, player, startArea.Center, 0);
				break;
			}
		}

		/// <summary>
		///   Changes the name of the given player.
		/// </summary>
		/// <param name="player">The player whose name should be changed.</param>
		/// <param name="playerName">The new name of the player.</param>
		public void RenamePlayer(Player player, string playerName)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNullOrWhitespace(playerName, nameof(playerName));

			playerName = _gameSession.Players.MakeUniquePlayerName(player, playerName);
			if (TextString.DisplayEqual(player.Name, playerName))
				return;

			Log.DebugIf(EnableTracing, "(Server) Player '{0}' ({1}) is renamed to '{2}'.", player.Name, player.Identity, playerName);
			player.Name = playerName;
			_gameSession.Broadcast(PlayerNameMessage.Create(_allocator, player.Identity, player.Name));
		}

		/// <summary>
		///   Handles a chat message sent by the given player.
		/// </summary>
		/// <param name="player">The player that sent the chat message.</param>
		/// <param name="message">The chat message that has been sent.</param>
		public void Chat(Player player, string message)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			_gameSession.Broadcast(PlayerChatMessage.Create(_allocator, player.Identity, message));
			Log.DebugIf(EnableTracing, "(Server) Player '{0}' ({1}): {2}", player.Name, player.Identity, message);
		}

		/// <summary>
		///   Broadcasts all entity updates to all connected clients.
		/// </summary>
		public void BroadcastEntityUpdates()
		{
			foreach (var entity in _gameSession.SceneGraph.EnumeratePreOrder<Entity>())
			{
				Assert.InRange(entity.Type);
				entity.BroadcastUpdates();
			}
		}

		/// <summary>
		///   Broadcasts the player stats to all connected clients.
		/// </summary>
		public void BroadcastPlayerStats()
		{
			foreach (var player in _gameSession.Players)
			{
				if (!player.IsServerPlayer)
					_gameSession.Broadcast(PlayerStatsMessage.Create(_gameSession.Allocator, player));
			}
		}
	}
}