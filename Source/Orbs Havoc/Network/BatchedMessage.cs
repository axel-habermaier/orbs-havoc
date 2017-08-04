namespace OrbsHavoc.Network
{
	using System.Collections.Generic;
	using Messages;
	using Utilities;

	/// <summary>
	///   Batches a list of messages for optimized network transmission.
	/// </summary>
	internal class BatchedMessage
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="messageType">The type of the batched messages.</param>
		public BatchedMessage(MessageType messageType)
		{
			Assert.ArgumentInRange(messageType, nameof(messageType));

			MessageType = messageType;
			Messages = new Queue<Message>();
		}

		/// <summary>
		///   Gets the type of the batched messages.
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		///   Gets the messages that the batched message contains.
		/// </summary>
		public Queue<Message> Messages { get; }
	}
}