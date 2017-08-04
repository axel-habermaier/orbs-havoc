namespace OrbsHavoc.Gameplay.Behaviors
{
	using System;
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Represents an AI that controls an orb.
	/// </summary>
	internal class AiBehavior : Behavior<Orb>
	{
		private PlayerInputBehavior _inputBehavior;

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			SceneNode.AddBehavior(_inputBehavior);
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			var up = false;
			var down = false;
			var left = false;
			var right = false;

			var target = Vector2.Zero;
			var enemy = GetNearestAvatar();

			if (enemy != null)
			{
				if (Vector2.DistanceSquared(SceneNode.WorldPosition, enemy.WorldPosition) > 40000)
				{
					up = SceneNode.WorldPosition.Y > enemy.WorldPosition.Y;
					left = SceneNode.WorldPosition.X > enemy.WorldPosition.X;
					down = !up;
					right = !left;
				}

				target = enemy.WorldPosition - SceneNode.WorldPosition;
			}

			_inputBehavior.HandleInput(target, up, down, left, right, target != Vector2.Zero, false);
		}

		/// <summary>
		///   Gets the orb closest to the AI.
		/// </summary>
		private Orb GetNearestAvatar()
		{
			Orb nearest = null;
			var nearestDistance = Single.MaxValue;

			foreach (var orb in SceneNode.SceneGraph.EnumeratePostOrder<Orb>())
			{
				if (orb == SceneNode || orb.PowerUp == EntityType.Invisibility)
					continue;

				var distance = Vector2.DistanceSquared(orb.WorldPosition, SceneNode.WorldPosition);
				if (!(distance < nearestDistance))
					continue;

				nearestDistance = distance;
				nearest = orb;
			}

			return nearest;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static AiBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var ai = allocator.Allocate<AiBehavior>();
			ai._inputBehavior = PlayerInputBehavior.Create(allocator);
			return ai;
		}
	}
}