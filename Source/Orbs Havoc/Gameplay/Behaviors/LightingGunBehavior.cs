namespace OrbsHavoc.Gameplay.Behaviors
{
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Fires a lighting bolt when the weapon is triggered.
	/// </summary>
	internal class LightingGunBehavior : WeaponBehavior
	{
		private LightingBolt _bolt;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public LightingGunBehavior()
		{
			Template = Weapons.LightingGun;
		}

		/// <summary>
		///   Starts firing a continuous weapon.
		/// </summary>
		protected override void StartFiring()
		{
			Assert.IsNull(_bolt, "Unexpected active lighting bolt entity.");

			_bolt = LightingBolt.Create(SceneNode.GameSession, SceneNode.Player);
			_bolt.AttachTo(SceneNode);
			_bolt.AcquireOwnership();
		}

		/// <summary>
		///   Stops firing a continuous weapon.
		/// </summary>
		protected override void StopFiring()
		{
			Assert.NotNull(_bolt, "Expected an active lighting bolt entity.");

			_bolt.Remove();
			_bolt.SafeDispose();
			_bolt = null;
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			if (_bolt == null)
				return;

			_bolt.Remove();
			_bolt.SafeDispose();
			_bolt = null;
		}

		/// <summary>
		///   Allocates an instance using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the behavior.</param>
		public static LightingGunBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var gun = allocator.Allocate<LightingGunBehavior>();
			gun._bolt = null;
			return gun;
		}
	}
}