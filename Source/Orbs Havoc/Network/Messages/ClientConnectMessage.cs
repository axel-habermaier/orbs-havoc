// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Network.Messages
{
	using System.Text;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a server about a connection attempt from a potential client.
	/// </summary>
	[ReliableTransmission(MessageType.ClientConnect)]
	internal sealed class ClientConnectMessage : Message
	{
		/// <summary>
		///   Gets the name of the player that is connecting.
		/// </summary>
		public string PlayerName { get; private set; }

		/// <summary>
		///   Gets the revision number of the network protocol that the connecting client implements.
		/// </summary>
		public byte NetworkRevision { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			writer.WriteByte(NetworkRevision);
			writer.WriteString(PlayerName, NetworkProtocol.MessagePlayerNameLength);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			NetworkRevision = reader.ReadByte();
			PlayerName = reader.ReadString(NetworkProtocol.MessagePlayerNameLength);
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnConnect(this);
		}

		/// <summary>
		///   Creates a connect message.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="playerName">The name of the player that is connecting.</param>
		public static ClientConnectMessage Create(PoolAllocator poolAllocator, string playerName)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNullOrWhitespace(playerName, nameof(playerName));
			Assert.That(Encoding.UTF8.GetByteCount(playerName) <= NetworkProtocol.MessagePlayerNameLength, "Player name is too long.");

			var message = poolAllocator.Allocate<ClientConnectMessage>();
			message.PlayerName = playerName;
			message.NetworkRevision = NetworkProtocol.Revision;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, NetworkRevision={NetworkRevision}, PlayerName='{PlayerName}'";
		}
	}
}