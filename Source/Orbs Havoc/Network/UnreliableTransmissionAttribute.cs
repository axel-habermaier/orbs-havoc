namespace OrbsHavoc.Network
{
	using System;
	using Messages;
	using Utilities;

	/// <summary>
	///   When applied to a network message class, indicates that the message should be transmitted unreliably. Optionally, as
	///   many messages of the type as possible can be batched together for optimized transmission.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal sealed class UnreliableTransmissionAttribute : Attribute
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="messageType">The network message type of the unreliable message.</param>
		public UnreliableTransmissionAttribute(MessageType messageType)
		{
			Assert.ArgumentInRange(messageType, nameof(messageType));
			MessageType = messageType;
		}

		/// <summary>
		///   Gets the network message type of the unreliable message.
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		///   Gets or sets a value indicating whether as many messages of the type as possible should be batched together for
		///   optimized transmission.
		/// </summary>
		public bool EnableBatching { get; set; }
	}
}