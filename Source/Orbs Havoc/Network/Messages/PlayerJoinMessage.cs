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

namespace OrbsHavoc.Network.Messages
{
	using System.Text;
	using Gameplay;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Informs a client that a new player has joined the game.
	/// </summary>
	[ReliableTransmission(MessageType.PlayerJoin)]
	internal sealed class PlayerJoinMessage : Message
	{
		/// <summary>
		///   Gets the name of the player that joined.
		/// </summary>
		public string PlayerName { get; private set; }

		/// <summary>
		///   Gets or sets the kind of the player.
		/// </summary>
		public PlayerKind PlayerKind { get; set; }

		/// <summary>
		///   Gets or sets the player's color.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		///   Gets the identifier of the player that joined the game session.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteByte((byte)PlayerKind);
			writer.WriteString(PlayerName, NetworkProtocol.MessagePlayerNameLength);
			writer.WriteUInt32(Color.ToArgb());
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Player = ReadIdentifier(ref reader);
			PlayerKind = (PlayerKind)reader.ReadByte();
			PlayerName = reader.ReadString(NetworkProtocol.MessagePlayerNameLength);
			Color = new Color(reader.ReadUInt32());
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnPlayerJoin(this);
		}

		/// <summary>
		///   Creates a join message for the given player.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="player">The player that has joined the game session.</param>
		public static Message Create(PoolAllocator poolAllocator, Player player)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.NotNullOrWhitespace(player.Name, "Invalid player name.");
			Assert.InRange(player.Kind);
			Assert.That(Encoding.UTF8.GetByteCount(player.Name) <= NetworkProtocol.MessagePlayerNameLength, "Player name is too long.");

			var message = poolAllocator.Allocate<PlayerJoinMessage>();
			message.Player = player.Identity;
			message.PlayerKind = player.Kind;
			message.PlayerName = player.Name;
			message.Color = player.Color;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Player={Player}, PlayerName='{PlayerName}', PlayerKind='{PlayerKind}'";
		}
	}
}