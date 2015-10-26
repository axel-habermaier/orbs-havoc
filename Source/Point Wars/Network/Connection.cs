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

namespace PointWars.Network
{
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a UDP-based connection to a remote peer.
	/// </summary>
	public sealed class Connection : DisposableObject
	{
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
		///   A cached queue of received messages.
		/// </summary>
		private readonly Queue<SequencedMessage> _receivedMessages = new Queue<SequencedMessage>();

		private readonly Socket _socket;

		/// <summary>
		///   The allocator that is used to allocate message objects.
		/// </summary>
		private PoolAllocator _allocator;

		private IPEndPoint _remoteEndPoint;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Connection()
		{
			_socket = new Socket(SocketType.Dgram, ProtocolType.IPv6) { DualMode = true, Blocking = false };
		}

		/// <summary>
		///   Gets or sets the remote end point of the connection.
		/// </summary>
		private IPEndPoint RemoteEndPoint
		{
			get
			{
				Assert.NotDisposed(this);
				return _remoteEndPoint;
			}
			set
			{
				Assert.NotDisposed(this);
				Assert.ArgumentNotNull(value, nameof(value));

				_remoteEndPoint = value;
				_socket.Connect(RemoteEndPoint);
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_socket.SafeDispose();
		}
	}
}