namespace OrbsHavoc.Network.Messages
{
	using System.Text;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a server and its clients about a player name change.
	/// </summary>
	[ReliableTransmission(MessageType.PlayerName)]
	internal sealed class PlayerNameMessage : Message
	{
		/// <summary>
		///   Gets the new name of the player.
		/// </summary>
		public string PlayerName { get; private set; }

		/// <summary>
		///   Gets the player whose name is changed.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteString(PlayerName, NetworkProtocol.MessagePlayerNameLength);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Player = ReadIdentifier(ref reader);
			PlayerName = reader.ReadString(NetworkProtocol.MessagePlayerNameLength);
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnPlayerName(this);
		}

		/// <summary>
		///   Creates a message that instructs the server to change the name of the given player.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="player">The player whose name should be changed.</param>
		/// <param name="playerName">The new player name.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity player, string playerName)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNullOrWhitespace(playerName, nameof(playerName));
			Assert.That(Encoding.UTF8.GetByteCount(playerName) <= NetworkProtocol.MessagePlayerNameLength, "Player name is too long.");

			var message = poolAllocator.Allocate<PlayerNameMessage>();
			message.Player = player;
			message.PlayerName = playerName;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Player={Player}, PlayerName='{PlayerName}'";
		}
	}
}