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

namespace OrbsHavoc.Network
{
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using Messages;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a connection to a remote peer, using the Lwar network protocol specification.
	/// </summary>
	internal class Connection : PooledObject
	{
		/// <summary>
		///   A cached buffer that is used to receive or send data.
		/// </summary>
		private readonly byte[] _buffer = new byte[NetworkProtocol.MaxPacketSize];

		/// <summary>
		///   The delivery manager responsible for the delivery guarantees of all incoming and outgoing messages.
		/// </summary>
		private readonly DeliveryManager _deliveryManager;

		/// <summary>
		///   The message queue responsible for packing all queued outgoing messages into a packet. Reliable messages will be
		///   resent until their reception has been acknowledged by the remote peer.
		/// </summary>
		private readonly MessageQueue _outgoingMessages;

		/// <summary>
		///   The packet assembler that the connection uses to assemble packets.
		/// </summary>
		private readonly PacketAssembler _packetAssembler = new PacketAssembler();

		/// <summary>
		///   A cached queue of received messages.
		/// </summary>
		private readonly Queue<SequencedMessage> _receivedMessages = new Queue<SequencedMessage>();

		/// <summary>
		///   The allocator that is used to allocate message objects.
		/// </summary>
		private PoolAllocator _allocator;

		/// <summary>
		///   Provides the time that is used to check whether a connection is lagging or dropped.
		/// </summary>
		private Clock _clock = new Clock();

		/// <summary>
		///   The deserializer that is used to deserialize incoming messages.
		/// </summary>
		private MessageDeserializer _deserializer;

		/// <summary>
		///   Indicates whether the socket is connected.
		/// </summary>
		private bool _isConnected;

		/// <summary>
		///   The socket that is used for the communication with the remote peer.
		/// </summary>
		private Socket _socket;

		/// <summary>
		///   The time in milliseconds since the last packet has been received.
		/// </summary>
		private double _timeSinceLastPacket;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Connection()
		{
			_deliveryManager = new DeliveryManager();
			_outgoingMessages = new MessageQueue(_deliveryManager);
			_packetAssembler.PacketAssembled += SendPacket;
		}

		/// <summary>
		///   Gets a value indicating whether the connection to the remote peer has been dropped.
		/// </summary>
		public bool IsDropped { get; private set; }

		/// <summary>
		///   Gets the endpoint of the remote peer.
		/// </summary>
		public IPEndPoint RemoteEndPoint { get; private set; }

		/// <summary>
		///   Gets the remaining time in milliseconds before the connection will be dropped.
		/// </summary>
		public double TimeToDrop => NetworkProtocol.DroppedTimeout - _timeSinceLastPacket;

		/// <summary>
		///   Gets a value indicating whether the connection to the remote peer is lagging.
		/// </summary>
		public bool IsLagging => _timeSinceLastPacket > NetworkProtocol.LaggingTimeout;

		/// <summary>
		///   Gets the connection's ping, i.e., the time it took the remote peer to acknowledge the last reliable message.
		/// </summary>
		public int Ping => _deliveryManager.Ping;

		/// <summary>
		///   Dispatches all received messages using the given message dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the messages.</param>
		public void DispatchReceivedMessages(IMessageHandler handler)
		{
			Assert.ArgumentNotNull(handler, nameof(handler));
			CheckAccess();

			var elapsedTime = _clock.Milliseconds;
			_clock.Reset();

			// Cap the time so that we don't disconnect when the debugger is suspending the process
			_timeSinceLastPacket += Math.Min(elapsedTime, 500);

			HandlePackets();
			HandleMessages(handler);
		}

		/// <summary>
		///   Handles the packets received over the socket.
		/// </summary>
		private void HandlePackets()
		{
			try
			{
				if (_isConnected)
				{
					while (_socket.Available > 0)
					{
						var size = _socket.Receive(_buffer);
						HandlePacket(size);
					}
				}
				else if (_socket.Available > 0)
				{
					EndPoint serverEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
					var size = _socket.ReceiveFrom(_buffer, ref serverEndPoint);

					_socket.Connect(serverEndPoint);
					_isConnected = true;

					HandlePacket(size);
				}
			}
			catch (SocketException e)
			{
				IsDropped = true;
				throw new NetworkException(e.GetMessage());
			}
		}

		/// <summary>
		///   Enqueues the given message for later sending to the remote peer.
		/// </summary>
		/// <param name="message">The message that should be sent.</param>
		public void EnqueueMessage(Message message)
		{
			using (message)
				_outgoingMessages.Enqueue(message);
		}

		/// <summary>
		///   Sends all queued messages.
		/// </summary>
		public void SendQueuedMessages()
		{
			CheckAccess();

			try
			{
				_packetAssembler.PrepareSending(_deliveryManager.LastReceivedReliableSequenceNumber);
				_outgoingMessages.SendMessages(_packetAssembler);
			}
			catch (SocketException e)
			{
				IsDropped = true;
				throw new NetworkException(e.GetMessage());
			}
		}

		/// <summary>
		///   Sends the given packet of the given size.
		/// </summary>
		/// <param name="packet">The packet that should be sent.</param>
		/// <param name="sizeInBytes">The size of the packet that should be sent.</param>
		private void SendPacket(byte[] packet, int sizeInBytes)
		{
			Assert.ArgumentNotNull(packet, nameof(packet));
			Assert.ArgumentInRange(sizeInBytes, 0, NetworkProtocol.MaxPacketSize, nameof(sizeInBytes));

			if (!_isConnected)
				_socket.SendTo(packet, sizeInBytes, SocketFlags.None, RemoteEndPoint);
			else
				_socket.Send(packet, sizeInBytes, SocketFlags.None);
		}

		/// <summary>
		///   Closes the connection to the remote peer.
		/// </summary>
		public void Disconnect()
		{
			if (IsDropped)
				return;

			try
			{
				EnqueueMessage(DisconnectMessage.Create(_allocator));
				SendQueuedMessages();
			}
			catch (SocketException e)
			{
				Log.Debug($"Failed to send disconnect message: {e.GetMessage()}");
			}

			IsDropped = true;
		}

		/// <summary>
		///   Dispatches the messages contained in the given packet.
		/// </summary>
		/// <param name="size">The size of the packet.</param>
		private void HandlePacket(int size)
		{
			var buffer = new BufferReader(_buffer, 0, size, Endianess.Big);

			if (!PacketHeader.TryRead(ref buffer, out var acknowledgement))
				return;

			_deliveryManager.UpdateLastAckedSequenceNumber(acknowledgement);

			var readBytes = -1;
			while (!buffer.EndOfBuffer && readBytes != buffer.Count)
			{
				readBytes = buffer.Count;

				while (_deserializer.TryDeserialize(ref buffer, out var message))
					_receivedMessages.Enqueue(message);
			}

			Assert.That(buffer.EndOfBuffer, "Received an invalid packet from the remote peer.");

			if (!buffer.EndOfBuffer)
				return;

			_timeSinceLastPacket = 0;
		}

		/// <summary>
		///   Handles the received messages.
		/// </summary>
		private void HandleMessages(IMessageHandler handler)
		{
			foreach (var sequencedMessage in _receivedMessages)
				sequencedMessage.Message.Dispatch(handler, sequencedMessage.SequenceNumber);

			ClearReceivedMessages();
		}

		/// <summary>
		///   Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected override void OnReturning()
		{
			ClearReceivedMessages();

			_socket.SafeDispose();
			_socket = null;
			_outgoingMessages.Clear();
			_deserializer.SafeDispose();
			_timeSinceLastPacket = 0;

			IsDropped = false;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_outgoingMessages.SafeDispose();
		}

		/// <summary>
		///   Clears the queue of received messages.
		/// </summary>
		private void ClearReceivedMessages()
		{
			foreach (var sequencedMessage in _receivedMessages)
				sequencedMessage.Message.SafeDispose();

			_receivedMessages.Clear();
		}

		/// <summary>
		///   In debug builds, checks whether the connection is faulted or has already been disposed.
		/// </summary>
		private void CheckAccess()
		{
			Assert.NotPooled(this);
			Assert.That(!IsDropped, "The connection has been dropped and can no longer be used.");

			if (TimeToDrop > 0 || IsDropped)
				return;

			IsDropped = true;
			throw new ConnectionDroppedException();
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate message objects.</param>
		private static Connection Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var connection = allocator.Allocate<Connection>();
			connection._allocator = allocator;
			connection._deserializer = MessageDeserializer.Create(allocator, connection._deliveryManager);
			connection._clock.Reset();
			connection._deliveryManager.Reset();
			return connection;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate message objects.</param>
		/// <param name="serverEndPoint">The end point of the server the connection should be established with.</param>
		public static Connection Create(PoolAllocator allocator, IPEndPoint serverEndPoint)
		{
			Assert.ArgumentNotNull(serverEndPoint, nameof(serverEndPoint));
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var connection = Create(allocator);
			connection._socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
			connection.RemoteEndPoint = serverEndPoint;
			connection._isConnected = false;

			return connection;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate message objects.</param>
		/// <param name="socket">The socket that should be used to receive and send data.</param>
		/// <param name="buffer">The buffer containing the initial packet received by the connection.</param>
		/// <param name="size">The size of the initial packet received by the connection.</param>
		public static Connection Create(PoolAllocator allocator, Socket socket, byte[] buffer, int size)
		{
			Assert.ArgumentNotNull(buffer, nameof(buffer));
			Assert.That(buffer.Length == NetworkProtocol.MaxPacketSize, "Invalid buffer size.");
			Assert.InRange(size, buffer);

			var connection = Create(allocator);
			connection._socket = socket;
			connection._isConnected = true;
			connection.RemoteEndPoint = (IPEndPoint)socket.RemoteEndPoint;

			Array.Copy(buffer, connection._buffer, buffer.Length);
			connection.HandlePacket(size);

			return connection;
		}
	}
}