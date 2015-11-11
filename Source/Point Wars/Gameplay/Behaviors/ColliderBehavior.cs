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

namespace PointWars.Gameplay.Behaviors
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
			SceneNode.GameSession.PhysicsSimulation.AddCollider(this);
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			SceneNode.GameSession.PhysicsSimulation.RemoveCollider(this);
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

			SceneNode.Position += collisionInfo.Value.Offset;
			SceneNode.HandleWallCollision();
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