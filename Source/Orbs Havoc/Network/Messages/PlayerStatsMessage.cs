namespace OrbsHavoc.Network.Messages
{
	using System;
	using Gameplay;
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
		///   Gets the player's remaining respawn delay.
		/// </summary>
		public byte RespawnDelay { get; private set; }

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
			writer.WriteByte(RespawnDelay);
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
			RespawnDelay = reader.ReadByte();
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
			message.RespawnDelay = (byte)MathUtils.RoundIntegral(player.RemainingRespawnDelay);
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