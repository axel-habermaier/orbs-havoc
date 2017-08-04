namespace OrbsHavoc.Network.Messages
{
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client that its connection attempt has been rejected.
	/// </summary>
	[UnreliableTransmission(MessageType.ClientRejected)]
	internal sealed class ClientRejectedMessage : Message
	{
		/// <summary>
		///   Gets the reason for the reject.
		/// </summary>
		public RejectReason Reason { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			writer.WriteByte((byte)Reason);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Reason = (RejectReason)reader.ReadByte();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnReject(this);
		}

		/// <summary>
		///   Creates a reject message that the server sends when it rejects a connection attempt from a client.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="reason">The reason why the connection attempt was rejected.</param>
		public static Message Create(PoolAllocator poolAllocator, RejectReason reason)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentInRange(reason, nameof(reason));

			var message = poolAllocator.Allocate<ClientRejectedMessage>();
			message.Reason = reason;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Reason={Reason}";
		}
	}
}