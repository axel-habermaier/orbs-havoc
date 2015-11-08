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

namespace PointWars.Network.Messages
{
	using System;
	using Gameplay;
	using Network;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client about the player stats of itself and other clients.
	/// </summary>
	[UnreliableTransmission(MessageType.PlayerStats, EnableBatching = true)]
	internal sealed class PlayerStatsMessage : Message
	{
		/// <summary>
		///   Gets the number of deaths of the player.
		/// </summary>
		public ushort Deaths { get; private set; }

		/// <summary>
		///   Gets the number of kills scored by the player.
		/// </summary>
		public ushort Kills { get; private set; }

		/// <summary>
		///   Gets the player's network latency.
		/// </summary>
		public ushort Ping { get; private set; }

		/// <summary>
		///   Gets the player's rank.
		/// </summary>
		public byte Rank { get; private set; }

		/// <summary>
		///   Gets the player whose stats are updated.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteUInt16(Kills);
			writer.WriteUInt16(Deaths);
			writer.WriteUInt16(Ping);
			writer.WriteByte(Rank);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Player = ReadIdentifier(ref reader);
			Kills = reader.ReadUInt16();
			Deaths = reader.ReadUInt16();
			Ping = reader.ReadUInt16();
			Rank = reader.ReadByte();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnPlayerStats(this, sequenceNumber);
		}

		/// <summary>
		///   Creates a stats message that the server broadcasts to all clients.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="player">The player whose stats are updated.</param>
		public static Message Create(PoolAllocator poolAllocator, Player player)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.InRange(player.Kills, 0, UInt16.MaxValue);
			Assert.InRange(player.Deaths, 0, UInt16.MaxValue);
			Assert.InRange(player.Ping, 0, UInt16.MaxValue);
			Assert.InRange(player.Rank, 0, NetworkProtocol.MaxPlayers);

			var message = poolAllocator.Allocate<PlayerStatsMessage>();
			message.Player = player.Identity;
			message.Kills = (ushort)player.Kills;
			message.Deaths = (ushort)player.Deaths;
			message.Ping = (ushort)player.Ping;
			message.Rank = (byte)player.Rank;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Player={Player},Kills={Kills},Deaths={Deaths},Ping={Ping}";
		}
	}
}