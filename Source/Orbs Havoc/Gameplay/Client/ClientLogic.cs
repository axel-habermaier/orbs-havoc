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

namespace OrbsHavoc.Gameplay.Client
{
	using System;
	using System.Numerics;
	using Assets;
	using Behaviors;
	using Network;
	using Network.Messages;
	using Platform.Memory;
	using SceneNodes;
	using SceneNodes.Entities;
	using Utilities;
	using Views;

	/// <summary>
	///   Implements the client logic for handling incoming and outgoing client messages.
	/// </summary>
	internal partial class ClientLogic : IMessageHandler
	{
		private readonly PoolAllocator _allocator;
		private readonly GameSession _gameSession;
		private readonly ViewCollection _views;
		private NetworkIdentityMap<Entity> _entityMap = new NetworkIdentityMap<Entity>(NetworkProtocol.MaxEntities);

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="gameSession">The game session that is being played.</param>
		/// <param name="views">The views that show the game.</param>
		public ClientLogic(PoolAllocator allocator, GameSession gameSession, ViewCollection views)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(views, nameof(views));

			_allocator = allocator;
			_gameSession = gameSession;
			_views = views;

			_gameSession.ChangeLevel(AssetBundle.TestLevel);
		}

		/// <summary>
		///   Gets or sets a value indicating whether the server and client game state have been synced.
		/// </summary>
		public bool IsSynced { get; set; }

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnReject(ClientRejectedMessage message)
		{
			switch (message.Reason)
			{
				case RejectReason.Full:
					throw new ServerFullException();
				case RejectReason.VersionMismatch:
					throw new ProtocolMismatchException();
				default:
					throw new InvalidOperationException("Unknown reject reason.");
			}
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnDisconnect(DisconnectMessage message)
		{
			throw new ServerQuitException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnSynced(ClientSyncedMessage message)
		{
			Assert.IsNull(_gameSession.Players.LocalPlayer, "A local player has already been set.");

			var player = _gameSession.Players[message.LocalPlayer];
			Assert.NotNull(player, "Unknown local player.");

			player.IsLocalPlayer = true;
			IsSynced = true;

			AssetBundle.Crosshair.ChangeColor(player.Color);

			var effect = _gameSession.Effects.Cursor.Allocate();
			effect.Emitters[0].ColorRange = player.ColorRange;

			var effectNode = ParticleEffectNode.Create(_allocator, effect, Vector2.Zero);
			effectNode.AddBehavior(TrailMouseBehavior.Create(_allocator, _views.Application.InputDevice.Mouse, _views.Game.Camera));
			effectNode.AttachTo(_gameSession.SceneGraph);

			_gameSession.MouseEffect = effectNode;
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityAdded(EntityAddMessage message)
		{
			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Entity add message references unknown player.");

			Entity entity = null;
			if (message.EntityType.IsCollectible())
				entity = Collectible.Create(_gameSession, message.Position, message.EntityType);
			else
			{
				switch (message.EntityType)
				{
					case EntityType.Orb:
						entity = Orb.Create(_gameSession, player, message.Position, message.Orientation);
						break;
					case EntityType.Bullet:
						entity = Bullet.Create(_gameSession, player, message.Position, message.Velocity, message.Orientation);
						break;
					case EntityType.Rocket:
						entity = Rocket.Create(_gameSession, player, message.Position, message.Velocity, message.Orientation);
						break;
					case EntityType.LightingBolt:
						entity = LightingBolt.Create(_gameSession, player);
						entity.AttachTo(_entityMap[message.ParentEntity]);
						break;
					default:
						Assert.NotReached("Unknown entity type.");
						break;
				}
			}

			_entityMap.Add(message.Entity, entity);

			entity.NetworkIdentity = message.Entity;
			entity.OnAdded();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityRemove(EntityRemoveMessage message)
		{
			var entity = _entityMap[message.Entity];
			Assert.NotNull(entity, "Entity remove message references unknown entity.");

			entity.OnRemoved();
			entity.Remove();

			_entityMap.Remove(entity.NetworkIdentity);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityCollision(EntityCollisionMessage message)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerJoin(PlayerJoinMessage message)
		{
			var player = Player.Create(_allocator, message.PlayerName, message.PlayerKind, message.Player);
			player.Color = message.Color;

			_gameSession.Players.Add(player);
			_views.EventMessages.AddJoinMessage(player);
			_views.Scoreboard.OnPlayersChanged();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerLeave(PlayerLeaveMessage message)
		{
			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Leave message references unknown player.");

			// Add the event message first, otherwise the player will already have been removed
			switch (message.Reason)
			{
				case LeaveReason.ConnectionDropped:
					_views.EventMessages.AddTimeoutMessage(player);
					break;
				case LeaveReason.Misbehaved:
					_views.EventMessages.AddKickedMessage(player, "Network protocol violation.");
					break;
				case LeaveReason.Disconnect:
				case LeaveReason.Unknown:
					_views.EventMessages.AddLeaveMessage(player);
					break;
				default:
					Assert.InRange(message.Reason);
					break;
			}

			// Now we can remove the player
			_gameSession.Players.Remove(player);
			_views.Scoreboard.OnPlayersChanged();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerKill(PlayerKillMessage message)
		{
			var killer = _gameSession.Players[message.Killer];
			var victim = _gameSession.Players[message.Victim];

			Assert.NotNull(killer, "Kill message references unknown killer.");
			Assert.NotNull(victim, "Kill message references unknown victim.");

			_views.EventMessages.AddKillMessage(killer, victim);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerName(PlayerNameMessage message)
		{
			// Ignore the server player
			if (message.Player == NetworkProtocol.ServerPlayerIdentity)
				return;

			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Player name message references unknown player.");

			var previousName = player.Name;
			player.Name = message.PlayerName;

			_views.EventMessages.AddNameChangeMessage(player, previousName);
			_views.Scoreboard.OnPlayersChanged();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerChatMessage(PlayerChatMessage message)
		{
			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Chat message references unknown player.");

			_views.EventMessages.AddChatMessage(player, message.Message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnPlayerStats(PlayerStatsMessage message, uint sequenceNumber)
		{
			// Get the player, if we know it; since the message is unreliable, it might
			// arrive sooner than the reliable join message for the message's player
			var player = _gameSession.Players[message.Player];
			if (player == null)
				return;

			player.Kills = message.Kills;
			player.Deaths = message.Deaths;
			player.Ping = message.Ping;
			player.Rank = message.Rank;
			player.RemainingRespawnDelay = message.RespawnDelay;

			_views.Scoreboard.OnPlayersChanged();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateCircle(UpdateCircleMessage message, uint sequenceNumber)
		{
			// Get the entity, if we know it; since the message is unreliable, it might
			// arrive sooner than the reliable entity add message for the message's entity
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateTransform(UpdateTransformMessage message, uint sequenceNumber)
		{
			// Get the entity, if we know it; since the message is unreliable, it might
			// arrive sooner than the reliable entity add message for the message's entity
			_entityMap[message.Entity]?.TransformationInterpolator.UpdateTransform(message.Position, message.Orientation, sequenceNumber);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateOrb(UpdateOrbMessage message, uint sequenceNumber)
		{
			// Get the entity, if we know it; since the message is unreliable, it might
			// arrive sooner than the reliable entity add message for the message's entity
			var orb = _entityMap[message.Entity] as Orb;
			orb?.OnUpdate(message, sequenceNumber);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateLightingBolt(UpdateLightingBoltMessage message, uint sequenceNumber)
		{
			// Get the entity, if we know it; since the message is unreliable, it might
			// arrive sooner than the reliable entity add message for the message's entity
			var bolt = _entityMap[message.Entity] as LightingBolt;
			bolt?.OnUpdate(message, sequenceNumber);
		}
	}
}