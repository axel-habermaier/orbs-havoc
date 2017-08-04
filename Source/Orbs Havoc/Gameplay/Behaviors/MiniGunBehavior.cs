namespace OrbsHavoc.Gameplay.Behaviors
{
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Fires a bullet when the weapon is triggered.
	/// </summary>
	internal class MiniGunBehavior : WeaponBehavior
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public MiniGunBehavior()
		{
			Template = Weapons.MiniGun;
		}

		/// <summary>
		///   Fires a single shot of a non-continuous weapon.
		/// </summary>
		protected override void Fire()
		{
			SpawnBullet(SceneNode.Orientation - RandomNumbers.NextSingle(Weapons.MiniGun.MinSpread, Weapons.MiniGun.MaxSpread));
			SpawnBullet(SceneNode.Orientation + RandomNumbers.NextSingle(Weapons.MiniGun.MinSpread, Weapons.MiniGun.MaxSpread));
		}

		/// <summary>
		///   Spawns a bullet with the given direction.
		/// </summary>
		private void SpawnBullet(float direction)
		{
			var directionVector = MathUtils.FromAngle(direction);
			var velocity = Vector2.Normalize(directionVector) * Template.Speed;

			Bullet.Create(SceneNode.GameSession, SceneNode.Player, SceneNode.WorldPosition, velocity, direction);
		}

		/// <summary>
		///   Allocates an instance using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the behavior.</param>
		public static MiniGunBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			return allocator.Allocate<MiniGunBehavior>();
		}
	}
}