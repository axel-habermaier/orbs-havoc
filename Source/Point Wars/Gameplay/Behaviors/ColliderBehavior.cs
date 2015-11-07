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
	using System.Numerics;
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

			int x, y;
			level.GetBlock(Circle.Position, out x, out y);

			switch (level[x, y])
			{
				case BlockType.HorizontalWall:
					var newY = HandleCollisionWithNonCurvedWall(SceneNode.WorldPosition.Y, Radius, level.GetBlockArea(x, y).Center.Y);
					if (SceneNode != null)
						SceneNode.Position = new Vector2(Circle.Position.X, newY);
					break;
				case BlockType.VerticalWall:
					var newX = HandleCollisionWithNonCurvedWall(SceneNode.WorldPosition.X, Radius, level.GetBlockArea(x, y).Center.X);
					if (SceneNode != null)
						SceneNode.Position = new Vector2(newX, SceneNode.WorldPosition.Y);
					break;
				case BlockType.LeftTopWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).BottomRight);
					break;
				case BlockType.RightTopWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).BottomLeft);
					break;
				case BlockType.LeftBottomWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).TopRight);
					break;
				case BlockType.RightBottomWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).TopLeft);
					break;
			}
		}

		/// <summary>
		///   Handles an entity collision with a horizontal or vertical wall.
		/// </summary>
		private float HandleCollisionWithNonCurvedWall(float circlePosition, float radius, float areaCenter)
		{
			if (MathUtils.Abs(circlePosition - areaCenter) > radius + Level.WallThickness / 2)
				return circlePosition;

			if (circlePosition < areaCenter)
				circlePosition -= circlePosition + radius - areaCenter + Level.WallThickness / 2;
			else
				circlePosition += areaCenter + Level.WallThickness / 2 - circlePosition + radius;

			SceneNode.HandleWallCollision();
			return circlePosition;
		}

		/// <summary>
		///   Handles an entity collision with a curved wall.
		/// </summary>
		private void HandleCollisionWithCurvedWall(Circle entityCircle, Vector2 wallPosition)
		{
			var wallCircle = new Circle(wallPosition, Level.BlockSize / 2 + Level.WallThickness / 2);
			if (!entityCircle.Intersects(wallCircle))
				return;

			var distance = Vector2.Distance(entityCircle.Position, wallCircle.Position);
			if (distance < wallCircle.Radius - Level.WallThickness / 2)
			{
				var overlap = distance + entityCircle.Radius - wallCircle.Radius + Level.WallThickness;
				if (!(overlap > 0))
					return;

				SceneNode.Position -= Vector2.Normalize(entityCircle.Position - wallCircle.Position) * overlap;
				SceneNode.HandleWallCollision();
			}
			else
			{
				var radius = entityCircle.Radius + wallCircle.Radius;
				var overlap = MathUtils.Abs(distance - radius);

				SceneNode.Position -= Vector2.Normalize(wallCircle.Position - entityCircle.Position) * overlap;
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