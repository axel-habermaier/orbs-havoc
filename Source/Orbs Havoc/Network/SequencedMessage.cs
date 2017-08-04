namespace OrbsHavoc.Network
{
	using Messages;
	using Utilities;

	/// <summary>
	///   Associates a message with a sequence number.
	/// </summary>
	internal struct SequencedMessage
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="message">The network message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public SequencedMessage(Message message, uint sequenceNumber)
			: this()
		{
			Assert.ArgumentNotNull(message, nameof(message));

			Message = message;
			SequenceNumber = sequenceNumber;
		}

		/// <summary>
		///   Gets the network message.
		/// </summary>
		public Message Message { get; }

		/// <summary>
		///   Gets the sequence number of the message.
		/// </summary>
		public uint SequenceNumber { get; }
	}
}