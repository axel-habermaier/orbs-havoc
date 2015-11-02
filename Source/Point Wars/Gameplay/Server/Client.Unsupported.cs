﻿// The MIT License (MIT)
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

namespace PointWars.Gameplay.Server
{
	using Network;
	using Network.Messages;
	using Utilities;

	internal partial class Client
	{
		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerKill(PlayerKillMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityAdded(EntityAddMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerJoin(PlayerJoinMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerLeave(PlayerLeaveMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnReject(ClientRejectedMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityRemove(EntityRemoveMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnPlayerStats(PlayerStatsMessage message, uint sequenceNumber)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnSynced(ClientSyncedMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateCircle(UpdateCircleMessage message, uint sequenceNumber)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateTransform(UpdateTransformMessage message, uint sequenceNumber)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdatePosition(UpdatePositionMessage message, uint sequenceNumber)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		void IMessageHandler.OnUpdateRay(UpdateRayMessage message, uint sequenceNumber)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnEntityCollision(EntityCollisionMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles an unsupported message.
		/// </summary>
		/// <param name="message">The unsupported message that should be handled.</param>
		private void HandleUnsupportedMessage(Message message)
		{
			Assert.NotReached("Received an unexpected message of type '{0}' from client at '{1}'.", message.MessageType, _connection.RemoteEndPoint);
		}
	}
}