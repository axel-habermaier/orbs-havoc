namespace OrbsHavoc.Network.Messages
{
	using System.Numerics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client about an update to a circle.
	/// </summary>
	[UnreliableTransmission(MessageType.UpdateCircle, EnableBatching = true)]
	internal sealed class UpdateCircleMessage : Message
	{
		/// <summary>
		///   Gets the entity that is updated.
		/// </summary>
		public NetworkIdentity Entity { get; private set; }

		/// <summary>
		///   Gets the new circle center.
		/// </summary>
		public Vector2 Center { get; private set; }

		/// <summary>
		///   Gets the new circle radius.
		/// </summary>
		public float Radius { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity);
			WriteVector2(ref writer, Center);
			writer.WriteUInt16((ushort)Radius);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity = ReadIdentifier(ref reader);
			Center = ReadVector2(ref reader);
			Radius = reader.ReadUInt16();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnUpdateCircle(this, sequenceNumber);
		}

		/// <summary>
		///   Creates an update message that the server broadcasts to all players.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="entity">The entity that is updated.</param>
		/// <param name="center">The updated center of the circle entity.</param>
		/// <param name="radius">The updated radius of the circle entity.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity entity, Vector2 center, float radius)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));

			var message = poolAllocator.Allocate<UpdateCircleMessage>();
			message.Entity = entity;
			message.Center = center;
			message.Radius = radius;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Entity={Entity}, Center={{{Center}}}, Radius={Radius}";
		}
	}
}