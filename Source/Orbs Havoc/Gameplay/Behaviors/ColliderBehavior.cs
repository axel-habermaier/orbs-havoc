namespace OrbsHavoc.Gameplay.Behaviors
{
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Represents a collider, i.e., an area that determines the occupied space of an entity.
	/// </summary>
	internal class ColliderBehavior : Behavior<Entity>
	{
		/// <summary>
		///   Gets the entity's radius.
		/// </summary>
		public float Radius { get; private set; }

		/// <summary>
		///   Gets the circle representing the collision area.
		/// </summary>
		public Circle Circle => new Circle(SceneNode.WorldPosition, Radius);

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			SceneNode.GameSession.Physics.AddCollider(this);
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			SceneNode.GameSession.Physics.RemoveCollider(this);
		}

		/// <summary>
		///   Handles collisions with level walls.
		/// </summary>
		/// <param name="level">The level containing the walls that should be checked.</param>
		public void HandleWallCollisions(Level level)
		{
			Assert.ArgumentNotNull(level, nameof(level));

			var collisionInfo = level.CheckWallCollision(Circle);
			if (collisionInfo == null)
				return;

			if (collisionInfo.Value.IsSubmerged)
				SceneNode.Remove();
			else
			{
				SceneNode.Position += collisionInfo.Value.Offset;
				SceneNode.HandleWallCollision();
			}
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="radius">The entity's radius.</param>
		public static ColliderBehavior Create(PoolAllocator allocator, float radius)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var behavior = allocator.Allocate<ColliderBehavior>();
			behavior.Radius = radius;
			return behavior;
		}
	}
}