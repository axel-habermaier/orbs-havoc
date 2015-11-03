// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Gameplay
{
	using System;
	using Entities;
	using Network;
	using Network.Messages;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Implements the client logic for handling incoming and outgoing client messages.
	/// </summary>
	internal partial class ClientLogic : IMessageHandler
	{
		/// <summary>
		///   The allocator that is used to allocate pooled objects.
		/// </summary>
		private readonly PoolAllocator _allocator;

		/// <summary>
		///   The game session that is being played.
		/// </summary>
		private readonly GameSession _gameSession;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="gameSession">The game session that is being played.</param>
		public ClientLogic(PoolAllocator allocator, GameSession gameSession)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			_allocator = allocator;
			_gameSession = gameSession;
		}

		/// <summary>
		///   Gets or sets a value indicating whether the server and client game state have been synced.
		/// </summary>
		public bool IsSynced { get; set; }

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityAdded(EntityAddMessage message)
		{
			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Expected a valid player.");

			var avatar = Avatar.Create(_gameSession, player);
			_gameSession.SceneGraph.Add(avatar, _gameSession.SceneGraph.Root);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerChatMessage(PlayerChatMessage message)
		{
			throw new NotImplementedException();
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
		void IMessageHandler.OnDisconnect(DisconnectMessage message)
		{
			throw new ServerQuitException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerJoin(PlayerJoinMessage message)
		{
			_gameSession.Players.Add(Player.Create(_allocator, message.PlayerName, message.Player));
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerKill(PlayerKillMessage message)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerLeave(PlayerLeaveMessage message)
		{
			var player = _gameSession.Players[message.Player];
			Assert.NotNull(player, "Expected a valid player.");

			_gameSession.Players.Remove(player);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerName(PlayerNameMessage message)
		{
			throw new NotImplementedException();
		}

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
		void IMessageHandler.OnEntityRemove(EntityRemoveMessage message)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnPlayerStats(PlayerStatsMessage message, uint sequenceNumber)
		{
			throw new NotImplementedException();
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
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateCircle(UpdateCircleMessage message, uint sequenceNumber)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateTransform(UpdateTransformMessage message, uint sequenceNumber)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdatePosition(UpdatePositionMessage message, uint sequenceNumber)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateRay(UpdateRayMessage message, uint sequenceNumber)
		{
			throw new NotImplementedException();
		}
	}
}