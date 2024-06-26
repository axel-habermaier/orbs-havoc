namespace OrbsHavoc.Network.Messages
{
	using System.Numerics;
	using Gameplay.SceneNodes.Entities;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Notifies a client about a newly added entity.
	/// </summary>
	[ReliableTransmission(MessageType.EntityAdd)]
	internal sealed class EntityAddMessage : Message
	{
		/// <summary>
		///   Gets the entity that is added.
		/// </summary>
		public NetworkIdentity Entity { get; private set; }

		/// <summary>
		///   Gets the parent entity of the entity that is added. Can be the reserved entity to indicate that the
		///   added entity has no parent.
		/// </summary>
		public NetworkIdentity ParentEntity { get; private set; }

		/// <summary>
		///   Gets the player the entity belongs to.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Gets the entity's initial position.
		/// </summary>
		public Vector2 Position { get; private set; }

		/// <summary>
		///   Gets the entity's initial velocity.
		/// </summary>
		public Vector2 Velocity { get; private set; }

		/// <summary>
		///   Gets the entity's initial orientation.
		/// </summary>
		public float Orientation { get; private set; }

		/// <summary>
		///   Gets the type of the entity that is added.
		/// </summary>
		public EntityType EntityType { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity);
			WriteIdentifier(ref writer, Player);
			WriteIdentifier(ref writer, ParentEntity);
			WriteVector2(ref writer, Position);
			WriteVector2(ref writer, Velocity);
			WriteOrientation(ref writer, Orientation);
			writer.WriteByte((byte)EntityType);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity = ReadIdentifier(ref reader);
			Player = ReadIdentifier(ref reader);
			ParentEntity = ReadIdentifier(ref reader);
			Position = ReadVector2(ref reader);
			Velocity = ReadVector2(ref reader);
			Orientation = ReadOrientation(ref reader);
			EntityType = (EntityType)reader.ReadByte();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnEntityAdded(this);
		}

		/// <summary>
		///   Creates an add message that the server broadcasts to all clients.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="entity">The entity that has been added.</param>
		public static EntityAddMessage Create(PoolAllocator poolAllocator, Entity entity)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNull(entity, nameof(entity));
			Assert.NotNull(entity.Player, "Entity has no player.");

			var parentIdentity = NetworkProtocol.ReservedEntityIdentity;
			if (entity.Parent is Entity parentEntity)
				parentIdentity = parentEntity.NetworkIdentity;

			var message = poolAllocator.Allocate<EntityAddMessage>();
			message.Entity = entity.NetworkIdentity;
			message.ParentEntity = parentIdentity;
			message.Player = entity.Player.Identity;
			message.EntityType = entity.Type;
			message.Position = entity.Position;
			message.Velocity = entity.Velocity;
			message.Orientation = entity.Orientation;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Entity={Entity}, Player={Player}, ParentEntity={ParentEntity}, EntityType={EntityType}, " +
				   $"Position={Position}, Orientation={Orientation}";
		}
	}
}