namespace OrbsHavoc.Network.Messages
{
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client about an update to a lighting bolt.
	/// </summary>
	[UnreliableTransmission(MessageType.UpdateRay, EnableBatching = true)]
	internal sealed class UpdateLightingBoltMessage : Message
	{
		/// <summary>
		///   Gets the entity that is updated.
		/// </summary>
		public NetworkIdentity Entity { get; private set; }

		/// <summary>
		///   Gets the new lighting bolt length.
		/// </summary>
		public float Length { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity);
			writer.WriteUInt16((ushort)Length);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity = ReadIdentifier(ref reader);
			Length = reader.ReadUInt16();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnUpdateLightingBolt(this, sequenceNumber);
		}

		/// <summary>
		///   Creates an update message that the server broadcasts to all players.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="entity">The entity that is updated.</param>
		/// <param name="length">The length of the lighting bolt.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity entity, float length)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));

			var message = poolAllocator.Allocate<UpdateLightingBoltMessage>();
			message.Entity = entity;
			message.Length = length;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Entity={Entity}, Length={Length}";
		}
	}
}