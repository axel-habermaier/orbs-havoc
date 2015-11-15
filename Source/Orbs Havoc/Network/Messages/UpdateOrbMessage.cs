// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Network.Messages
{
	using Gameplay;
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
		public NetworkIdentity Orb { get; private set; }

		/// <summary>
		///   Gets the energy levels of the orb's weapons.
		/// </summary>
		public int[] WeaponEnergyLevels { get; } = new int[Constants.Orb.WeaponCount];

		/// <summary>
		///   Gets or sets the orb's primary weapon.
		/// </summary>
		public EntityType PrimaryWeapon { get; set; }

		/// <summary>
		///   Gets or sets the orb's secondary weapon.
		/// </summary>
		public EntityType SecondaryWeapon { get; set; }

		/// <summary>
		///   Gets or sets the power up that currently influences the orb.
		/// </summary>
		public EntityType PowerUp { get; set; }

		/// <summary>
		///   Gets or sets the remaining time until the power up is removed.
		/// </summary>
		public float RemainingPowerUpTime { get; set; }

		/// <summary>
		///   Gets or sets the orb's remaining health.
		/// </summary>
		public float Health { get; set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Orb);
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
			Orb = ReadIdentifier(ref reader);
			PrimaryWeapon = (EntityType)reader.ReadByte();
			SecondaryWeapon = (EntityType)reader.ReadByte();
			PowerUp = (EntityType)reader.ReadByte();
			RemainingPowerUpTime = reader.ReadByte();
			Health = reader.ReadByte();

			for (var i = 0; i < Constants.Orb.WeaponCount; ++i)
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
			message.Orb = orb.NetworkIdentity;
			message.PowerUp = orb.PowerUp;
			message.RemainingPowerUpTime = orb.RemainingPowerUpTime;
			message.Health = orb.Health;
			message.PrimaryWeapon = orb.PrimaryWeapon;
			message.SecondaryWeapon = orb.SecondaryWeapon;

			for (var i = 0; i < Constants.Orb.WeaponCount; ++i)
				message.WeaponEnergyLevels[i] = orb.WeaponEnergyLevels[i];

			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Avatar={Orb}";
		}
	}
}