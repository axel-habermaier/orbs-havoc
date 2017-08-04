namespace OrbsHavoc.Gameplay.Behaviors
{
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Fires a rocket when the weapon is triggered.
	/// </summary>
	internal class RocketLauncherBehavior : WeaponBehavior
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public RocketLauncherBehavior()
		{
			Template = Weapons.RocketLauncher;
		}

		/// <summary>
		///   Fires a single shot of a non-continuous weapon.
		/// </summary>
		protected override void Fire()
		{
			var spread = RandomNumbers.NextSingle(Weapons.RocketLauncher.MinSpread, Weapons.RocketLauncher.MaxSpread);
			spread *= RandomNumbers.NextInteger() % 2 == 0 ? 1 : -1;

			var directionVector = MathUtils.FromAngle(SceneNode.Orientation + spread);
			var velocity = Vector2.Normalize(directionVector) * Template.Speed;

			Rocket.Create(SceneNode.GameSession, SceneNode.Player, SceneNode.WorldPosition, velocity, SceneNode.Orientation);
		}

		/// <summary>
		///   Allocates an instance using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the behavior.</param>
		public static RocketLauncherBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			return allocator.Allocate<RocketLauncherBehavior>();
		}
	}
}