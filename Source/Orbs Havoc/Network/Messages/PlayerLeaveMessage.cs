namespace OrbsHavoc.Network.Messages
{
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client that a player has left the game.
	/// </summary>
	[ReliableTransmission(MessageType.PlayerLeave)]
	internal sealed class PlayerLeaveMessage : Message
	{
		/// <summary>
		///   Gets the player that has left the game session.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Gets the reason explaining why the player has left the game session.
		/// </summary>
		public LeaveReason Reason { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteByte((byte)Reason);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Player = ReadIdentifier(ref reader);
			Reason = (LeaveReason)reader.ReadByte();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnPlayerLeave(this);
		}

		/// <summary>
		///   Creates a leave message for the given player.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="player">The player that has left the game session.</param>
		/// <param name="reason">The reason why the player left the game session.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity player, LeaveReason reason)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentInRange(reason, nameof(reason));
			Assert.ArgumentSatisfies(reason != LeaveReason.Unknown, nameof(reason), "The leave reason cannot be unknown.");

			var message = poolAllocator.Allocate<PlayerLeaveMessage>();
			message.Player = player;
			message.Reason = reason;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Player={Player}, LeaveReason={Reason}";
		}
	}
}