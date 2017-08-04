namespace OrbsHavoc.Network
{
	using System;
	using Messages;
	using Utilities;

	/// <summary>
	///   When applied to a network message class, indicates that the message should be transmitted reliably.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal sealed class ReliableTransmissionAttribute : Attribute
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="messageType">The network message type of the reliable message.</param>
		public ReliableTransmissionAttribute(MessageType messageType)
		{
			Assert.ArgumentInRange(messageType, nameof(messageType));
			MessageType = messageType;
		}

		/// <summary>
		///   Gets the network message type of the reliable message.
		/// </summary>
		public MessageType MessageType { get; }
	}
}