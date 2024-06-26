﻿namespace OrbsHavoc.Gameplay.Behaviors
{
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Allows a player to control an orb.
	/// </summary>
	internal class PlayerInputBehavior : Behavior<Orb>
	{
		private const float MaxSpeed = 4000;
		private const float MaxAcceleration = 4000;
		private const float Drag = .85f;
		private readonly WeaponBehavior[] _weapons = new WeaponBehavior[Orb.WeaponsCount];
		private Vector2 _acceleration;

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			foreach (var weapon in _weapons)
				SceneNode.AddBehavior(weapon);
		}

		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="target">The target the ship should be facing, relative to the ship's position.</param>
		/// <param name="moveUp">Indicates whether the player should move up.</param>
		/// <param name="moveDown">Indicates whether the player should move down.</param>
		/// <param name="moveLeft">Indicates whether the player should move to the left.</param>
		/// <param name="moveRight">Indicates whether the player should move to the right.</param>
		/// <param name="firePrimary">Indicates whether the primary weapon should be fired.</param>
		/// <param name="fireSecondary">Indicates whether the secondary weapon should be fired.</param>
		public void HandleInput(Vector2 target, bool moveUp, bool moveDown, bool moveLeft, bool moveRight, bool firePrimary, bool fireSecondary)
		{
			// Update the orb's orientation
			if (target.LengthSquared() > 10)
				SceneNode.Orientation = MathUtils.ToAngle(target);

			// Update the orb's acceleration
			_acceleration = Vector2.Zero;

			if (moveLeft)
				_acceleration += new Vector2(-1, 0);
			if (moveRight)
				_acceleration += new Vector2(1, 0);
			if (moveUp)
				_acceleration += new Vector2(0, -1);
			if (moveDown)
				_acceleration += new Vector2(0, 1);

			if (_acceleration != Vector2.Zero)
				_acceleration = Vector2.Normalize(_acceleration);

			// Fire the orb's weapons
			_weapons[SceneNode.PrimaryWeapon.GetWeaponSlot()].HandlePlayerInput(firePrimary);
			if (SceneNode.SecondaryWeapon != EntityType.Unknown)
				_weapons[SceneNode.SecondaryWeapon.GetWeaponSlot()].HandlePlayerInput(fireSecondary);
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			// Compute the new velocity and make sure the orb eventually stops when the player doesn't move
			SceneNode.Velocity += _acceleration * MaxAcceleration * elapsedSeconds;
			SceneNode.Velocity *= Drag;

			// Cap the velocity
			if (SceneNode.Velocity.LengthSquared() > MaxSpeed * MaxSpeed)
				SceneNode.Velocity = MaxSpeed * Vector2.Normalize(SceneNode.Velocity);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static PlayerInputBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var input = allocator.Allocate<PlayerInputBehavior>();
			input._acceleration = Vector2.Zero;
			input._weapons[EntityType.MiniGun.GetWeaponSlot()] = MiniGunBehavior.Create(allocator);
			input._weapons[EntityType.PlasmaGun.GetWeaponSlot()] = MiniGunBehavior.Create(allocator); // TODO
			input._weapons[EntityType.LightingGun.GetWeaponSlot()] = LightingGunBehavior.Create(allocator);
			input._weapons[EntityType.RocketLauncher.GetWeaponSlot()] = RocketLauncherBehavior.Create(allocator);
			input._weapons[EntityType.Bfg.GetWeaponSlot()] = MiniGunBehavior.Create(allocator); // TODO
			input._weapons[EntityType.Bomb.GetWeaponSlot()] = MiniGunBehavior.Create(allocator); // TODO
			input._weapons[EntityType.Mine.GetWeaponSlot()] = MiniGunBehavior.Create(allocator); // TODO
			input._weapons[EntityType.ShockWave.GetWeaponSlot()] = MiniGunBehavior.Create(allocator); // TODO

			return input;
		}
	}
}