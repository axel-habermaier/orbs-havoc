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
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using Network;
	using Network.Messages;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a collection of clients.
	/// </summary>
	internal class ClientCollection : DisposableObject
	{
		/// <summary>
		///   The allocator that is used to allocate pooled objects.
		/// </summary>
		private readonly PoolAllocator _allocator;

		/// <summary>
		///   A cached buffer that is used to receive incomming connection messages.
		/// </summary>
		private readonly byte[] _buffer = new byte[NetworkProtocol.MaxPacketSize];

		/// <summary>
		///   The clients of the game session.
		/// </summary>
		private readonly List<Client> _clients = new List<Client>(NetworkProtocol.MaxPlayers);

		/// <summary>
		///   Listens for incoming UDP connections.
		/// </summary>
		private readonly Socket _listener;

		/// <summary>
		///   The server logic that handles the communication between the server and the clients.
		/// </summary>
		private readonly ServerLogic _serverLogic;

		/// <summary>
		///   The cached end point of the connecting client.
		/// </summary>
		private EndPoint _connectingEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="serverLogic">The server logic that handles the communication between the server and the clients.</param>
		/// <param name="listener">The socket that should be used to listen for connecting clients.</param>
		public ClientCollection(PoolAllocator allocator, ServerLogic serverLogic, Socket listener)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(serverLogic, nameof(serverLogic));
			Assert.ArgumentNotNull(listener, nameof(listener));

			_allocator = allocator;
			_serverLogic = serverLogic;
			_listener = listener;
		}

		/// <summary>
		///   Updates the clients.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			try
			{
				while (_listener.Available > 0)
					AddClient(_listener.ReceiveFrom(_buffer, ref _connectingEndPoint));
			}
			catch (SocketException e)
			{
				throw new NetworkException($"The server can no longer accept incoming connection requests: {e.GetMessage()}");
			}

			RemoveDisconnectedClients();

			foreach (var client in _clients)
				client.Update(elapsedSeconds);
		}

		/// <summary>
		///   Adds a new client, if the connecting end point isn't known already.
		/// </summary>
		/// <param name="packetSize">The size of the connection packet sent by the client.</param>
		private void AddClient(int packetSize)
		{
			// Check if we already know the client
			foreach (var knownClient in _clients)
			{
				if (knownClient.RemoteEndPoint.Equals((IPEndPoint)_connectingEndPoint))
					return;
			}

			// Otherwise, create a new one
			var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
			socket.Connect(_connectingEndPoint);

			var connection = Connection.Create(_allocator, socket, _buffer, packetSize);
			var client = Client.Create(_allocator, connection, _serverLogic);
			_clients.Add(client);
		}

		/// <summary>
		///   Dispatches all messages received from the clients.
		/// </summary>
		public void DispatchClientMessages()
		{
			foreach (var client in _clients)
				client.DispatchReceivedMessages();
		}

		/// <summary>
		///   Sends all queued messages to the clients.
		/// </summary>
		public void SendQueuedMessages()
		{
			foreach (var client in _clients)
			{
				try
				{
					client.SendQueuedMessages();
				}
				catch (Exception e) when (e is ConnectionDroppedException || e is NetworkException)
				{
					// Ignore the exception here, we'll deal with the dropped client during the next update
				}
			}
		}

		/// <summary>
		///   Sends the given message to all connected clients.
		/// </summary>
		/// <param name="message">The message that should be sent to all clients.</param>
		public void Broadcast(Message message)
		{
			Assert.ArgumentNotNull(message, nameof(message));

			using (message)
			{
				foreach (var client in _clients)
				{
					if (client.IsDisconnected || !client.IsSynced)
						continue;

					message.AcquireOwnership();
					client.Send(message);
				}
			}
		}

		/// <summary>
		///   Removes all disconnected clients.
		/// </summary>
		private void RemoveDisconnectedClients()
		{
			for (var i = 0; i < _clients.Count; ++i)
			{
				if (!_clients[i].IsDisconnected)
					continue;

				_clients[i].SafeDispose();
				_clients[i] = _clients[_clients.Count - 1];
				_clients.RemoveAt(_clients.Count - 1);
				--i;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			foreach (var client in _clients)
				client.Disconnect();

			while (_clients.Count > 0)
			{
				_clients[_clients.Count - 1].SafeDispose();
				_clients.RemoveAt(_clients.Count - 1);
			}
		}
	}
}