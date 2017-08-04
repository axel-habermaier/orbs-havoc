namespace OrbsHavoc.Network.Messages
{
	using Gameplay.SceneNodes.Entities;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client about updated orb data.
	/// </summary>
	[UnreliableTransmission(MessageType.UpdateOrb)]
	internal sealed class UpdateOrbMessage : Message
	{
		/// <summary>
		///   Gets the orb that is updated.
		/// </summary>
		public NetworkIdentity Entity { get; private set; }

		/// <summary>
		///   Gets the energy levels of the orb's weapons.
		/// </summary>
		public int[] WeaponEnergyLevels { get; } = new int[Orb.WeaponsCount];

		/// <summary>
		///   Gets or sets the orb's primary weapon.
		/// </summary>
		public EntityType PrimaryWeapon { get; private set; }

		/// <summary>
		///   Gets or sets the orb's secondary weapon.
		/// </summary>
		public EntityType SecondaryWeapon { get; private set; }

		/// <summary>
		///   Gets or sets the power up that currently influences the orb.
		/// </summary>
		public EntityType PowerUp { get; private set; }

		/// <summary>
		///   Gets or sets the remaining time until the power up is removed.
		/// </summary>
		public float RemainingPowerUpTime { get; private set; }

		/// <summary>
		///   Gets or sets the orb's remaining health.
		/// </summary>
		public float Health { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity);
			writer.WriteByte((byte)PrimaryWeapon);
			writer.WriteByte((byte)SecondaryWeapon);
			writer.WriteByte((byte)PowerUp);
			writer.WriteByte((byte)MathUtils.RoundIntegral(RemainingPowerUpTime));
			writer.WriteByte((byte)MathUtils.RoundIntegral(Health));

			foreach (var energyLevel in WeaponEnergyLevels)
				writer.WriteByte((byte)energyLevel);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity = ReadIdentifier(ref reader);
			PrimaryWeapon = (EntityType)reader.ReadByte();
			SecondaryWeapon = (EntityType)reader.ReadByte();
			PowerUp = (EntityType)reader.ReadByte();
			RemainingPowerUpTime = reader.ReadByte();
			Health = reader.ReadByte();

			for (var i = 0; i < Orb.WeaponsCount; ++i)
				WeaponEnergyLevels[i] = reader.ReadByte();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnUpdateOrb(this, sequenceNumber);
		}

		/// <summary>
		///   Creates an update message that the server broadcasts to all players.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="orb">The orb that is updated.</param>
		public static Message Create(PoolAllocator poolAllocator, Orb orb)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentNotNull(orb, nameof(orb));

			var message = poolAllocator.Allocate<UpdateOrbMessage>();
			message.Entity = orb.NetworkIdentity;
			message.PowerUp = orb.PowerUp;
			message.RemainingPowerUpTime = orb.RemainingPowerUpTime;
			message.Health = orb.Health;
			message.PrimaryWeapon = orb.PrimaryWeapon;
			message.SecondaryWeapon = orb.SecondaryWeapon;

			for (var i = 0; i < Orb.WeaponsCount; ++i)
				message.WeaponEnergyLevels[i] = orb.WeaponEnergyLevels[i];

			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Avatar={Entity}";
		}
	}
}