namespace OrbsHavoc.Gameplay.Client
{
	using Network;
	using Network.Messages;
	using Utilities;

	/// <summary>
	///   Implements the client logic for handling incoming and outgoing client messages.
	/// </summary>
	internal partial class ClientLogic
	{
		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnConnect(ClientConnectMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerInput(PlayerInputMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles an unsupported message.
		/// </summary>
		/// <param name="message">The unsupported message that should be handled.</param>
		private static void HandleUnsupportedMessage(Message message)
		{
			Assert.NotReached($"The client cannot handle a message of type '{message.MessageType}'.");
		}
	}
}