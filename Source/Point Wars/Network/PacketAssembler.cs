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
	using System;
	using System.Collections.Generic;
	using System.Net;
	using System.Net.Sockets;
	using Messages;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Assembles messages into packets and sends the packets over a UDP channel. If the given messages don't fit into a single
	///   packet, multiple packets are sent.
	/// </summary>
	internal struct PacketAssembler
	{
		/// <summary>
		///   If tracing is enabled, the contents of all sent packets are shown in the debug output.
		/// </summary>
		private const bool EnableTracing = false;

		/// <summary>
		///   The acknowledgement that is set in the headers of the assembled packets
		/// </summary>
		private readonly uint _acknowledgement;

		/// <summary>
		///   The socket that should be used to send the messages.
		/// </summary>
		private readonly Socket _socket;

		/// <summary>
		///   The buffer that the current packet is written into.
		/// </summary>
		private readonly byte[] _buffer;
		/// <summary>
		/// Indicates whether the socket is connected.
		/// </summary>
		private readonly bool _isConnected;

		/// <summary>
		///   The writer that is currently being used to assemble the packet.
		/// </summary>
		private BufferWriter _writer;

		/// <summary>
		///   The remote end point the packets are sent to.
		/// </summary>
		private readonly IPEndPoint _remoteEndPoint;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="socket">The UDP channel that should be used to send the assembled packets.</param>
		/// <param name="remoteEndPoint">The remote end point the packets should be sent to.</param>
		/// <param name="buffer">The buffer the packet should be assembled to.</param>
		/// <param name="isConnected">Indicates whether the socket is connected.</param>
		/// <param name="acknowledgement">The acknowledgement that should be set in the headers of the assembled packets.</param>
		public PacketAssembler(Socket socket, IPEndPoint remoteEndPoint, byte[] buffer, bool isConnected, uint acknowledgement)
			: this()
		{
			Assert.ArgumentNotNull(socket, nameof(socket));
			Assert.ArgumentNotNull(remoteEndPoint, nameof(remoteEndPoint));
			Assert.ArgumentNotNull(buffer, nameof(buffer));

			_socket = socket;
			_remoteEndPoint = remoteEndPoint;
			_buffer = buffer;
			_isConnected = isConnected;
			_acknowledgement = acknowledgement;

			AllocatePacket();
		}

		/// <summary>
		///   The number of packets sent to the remote peer.
		/// </summary>
		public int PacketCount { get; private set; }

		/// <summary>
		///   Sends the given reliable messages in the order they are contained in the queue.
		/// </summary>
		/// <param name="sequencedMessages">The sequenced reliable messages that should be sent.</param>
		public void SendReliableMessages(Queue<SequencedMessage> sequencedMessages)
		{
			Assert.ArgumentNotNull(sequencedMessages, nameof(sequencedMessages));

			foreach (var sequencedMessage in sequencedMessages)
			{
				Assert.That(!sequencedMessage.Message.UseBatchedTransmission, "Reliable messages with batched serialization are not supported.");
				SendMessage(sequencedMessage);
			}
		}

		/// <summary>
		///   Sends the given unreliable messages.
		/// </summary>
		/// <param name="sequencedMessages">The sequenced unreliable messages that should be sent.</param>
		public void SendUnreliableMessages(List<SequencedMessage> sequencedMessages)
		{
			Assert.ArgumentNotNull(sequencedMessages, nameof(sequencedMessages));

			foreach (var sequencedMessage in sequencedMessages)
			{
				Assert.That(!sequencedMessage.Message.UseBatchedTransmission, "Reliable messages with batched serialization are not supported.");
				SendMessage(sequencedMessage);
			}
		}

		/// <summary>
		///   Sends the given batched unreliable messages.
		/// </summary>
		/// <param name="batchedMessages">The batched unreliable messages that should be sent.</param>
		/// <param name="deliveryManager">The delivery manager that should be used to assign sequence numbers to the batched messages.</param>
		public void SendBatchedMessages(List<BatchedMessage> batchedMessages, DeliveryManager deliveryManager)
		{
			Assert.ArgumentNotNull(batchedMessages, nameof(batchedMessages));
			Assert.ArgumentNotNull(deliveryManager, nameof(deliveryManager));

			foreach (var batchedMessage in batchedMessages)
				SendBatchedMessage(batchedMessage, deliveryManager);
		}

		/// <summary>
		///   Sends the given batched unreliable message.
		/// </summary>
		/// <param name="batchedMessage">The batched unreliable message that should be sent.</param>
		/// <param name="deliveryManager">The delivery manager that should be used to assign sequence numbers to the batched message.</param>
		private void SendBatchedMessage(BatchedMessage batchedMessage, DeliveryManager deliveryManager)
		{
			Assert.ArgumentNotNull(batchedMessage, nameof(batchedMessage));

			// Each iteration of the loop writes a fragment of the batched message to the current packet packet.
			while (batchedMessage.Messages.Count != 0)
			{
				const int countSize = sizeof(byte);
				const int typeSize = sizeof(byte);
				const int sequenceNumberSize = sizeof(uint);
				const int headerSize = countSize + typeSize + sequenceNumberSize;

				// If we can't write the message header, there's no point in even trying to write any messages.
				// Just allocate a new packet right now.
				if (!_writer.CanWrite(headerSize))
				{
					SendPacket();
					AllocatePacket();
				}

				// Write the message type and sequence number of the batched message.
				var sequenceNumber = deliveryManager.AssignUnreliableSequenceNumber();
				_writer.WriteByte((byte)batchedMessage.MessageType);
				_writer.WriteUInt32(sequenceNumber);

				// We don't know yet how many messages we're going to write, so remember the current write position
				// and skip writing the message count for the moment. Note that we might send empty batched messages
				// in the case that the header fits but no messages fit into the packet. We can live with that, as it
				// causes only few bytes of overhead and shouldn't happen often anyway.
				var countPosition = _writer.WritePosition;
				_writer.SkipBytes(sizeof(byte));

				// Write all messages into the current packet that fit, keeping count of how many messages we've written.
				var count = 0;
				do
				{
					var message = batchedMessage.Messages.Peek();

					// If the message doesn't fit, we have to write the remaining messages to the next packet.
					if (!_writer.TryWrite(message, BatchedMessageSerializer))
						break;

					// If the message does fit, we can dispose of the message and start writing the next one.
					batchedMessage.Messages.Dequeue().SafeDispose();
					++count;
				} while (batchedMessage.Messages.Count > 0);

				// Check whether we've written too many messages. This can't happen at the moment because
				// each message is greater than 2 bytes and we have at most 512 bytes per packet available,
				// so the message count will always be less than Byte.MaxValue. This will become a problem
				// if we ever increase the packet size.
				Assert.That(NetworkProtocol.MaxPacketSize == 512, "Batched message might have to be split even further.");
				Assert.That(count <= Byte.MaxValue, "Too many messages have been written.");

				// We now have to write the message count, so we go back to the reserved place for the count in the packet,
				// write the count and reset the writing position.
				var position = _writer.WritePosition;
				_writer.WritePosition = countPosition;
				_writer.WriteByte((byte)count);
				_writer.WritePosition = position;

				Log.DebugIf(EnableTracing, "   (b) {0}: {1} #{2}", sequenceNumber, batchedMessage.MessageType, count);

				// If there are any messages left, we've run out of space and have to allocate a new packet for the
				// next fragment of the batched message.
				if (batchedMessage.Messages.Count > 0)
				{
					SendPacket();
					AllocatePacket();
				}
			}
		}

		/// <summary>
		///   Sends the given message.
		/// </summary>
		/// <param name="sequencedMessage">The message that should be sent.</param>
		private void SendMessage(SequencedMessage sequencedMessage)
		{
			// Write the message into the packet. If all goes well, the message fits into the packet and we can continue
			// writing the next one.
			if (_writer.TryWrite(sequencedMessage, SequencedMessageSerializer))
				return;

			// Otherwise, the message did not fit into the packet; so, send the packet and allocate a new one.
			SendPacket();
			AllocatePacket();

			// Write the message again. This really should not fail; if it does, something really bad has happened.
			if (!_writer.TryWrite(sequencedMessage, SequencedMessageSerializer))
				throw new NetworkException("Failed to write message into newly allocated packet.");
		}

		/// <summary>
		///   Serializes the given sequenced message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		/// <param name="sequencedMessage">The message that should be serialized.</param>
		private static void SerializeSequencedMessage(ref BufferWriter writer, SequencedMessage sequencedMessage)
		{
			writer.WriteByte((byte)sequencedMessage.Message.MessageType);
			writer.WriteUInt32(sequencedMessage.SequenceNumber);

			sequencedMessage.Message.Serialize(ref writer);
			Log.DebugIf(EnableTracing, "   ({2}) {0}: {1}", sequencedMessage.SequenceNumber, sequencedMessage.Message,
				sequencedMessage.Message.IsReliable ? "r" : "u");
		}

		/// <summary>
		///   Serializes the given batched message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		/// <param name="message">The message that should be serialized.</param>
		private static void SerializeBatchedMessage(ref BufferWriter writer, Message message)
		{
			message.Serialize(ref writer);
		}

		/// <summary>
		///   Allocates a new packet.
		/// </summary>
		private void AllocatePacket()
		{
			_writer = new BufferWriter(_buffer, Endianess.Big);

			Log.DebugIf(EnableTracing, "Packet #{2} to {0}, ack: {1}", _remoteEndPoint, _acknowledgement, PacketCount + 1);
			PacketHeader.Write(ref _writer, _acknowledgement);
		}

		/// <summary>
		///   Sends the assembled packet over the UDP channel.
		/// </summary>
		public void SendPacket()
		{
			Assert.NotNull(_buffer, "No packet has been allocated.");

			if (!_isConnected)
				_socket.SendTo(_buffer, _writer.Count, SocketFlags.None, _remoteEndPoint);
			else
				_socket.Send(_buffer, _writer.Count, SocketFlags.None);

			Log.DebugIf(EnableTracing, "Packet length: {0} bytes", _writer.Count);
			++PacketCount;
		}

		/// <summary>
		///   A cached delegate of the sequenced message serialization function.
		/// </summary>
		private static readonly BufferWriter.Serializer<SequencedMessage> SequencedMessageSerializer = SerializeSequencedMessage;

		/// <summary>
		///   A cached delegate of the batched message serialization function.
		/// </summary>
		private static readonly BufferWriter.Serializer<Message> BatchedMessageSerializer = SerializeBatchedMessage;
	}
}