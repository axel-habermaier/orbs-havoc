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

			var blockType = level[x, y];
			var isInverse = false;
			Vector2 position;
			Size size;

			switch (blockType)
			{
				case EntityType.TopWall:
					position = level.GetBlockArea(x, y).TopLeft;
					size = new Size(Level.BlockSize, Level.BlockSize / 2 + Level.WallThickness / 2);
					HandleWallCollision(Circle, new Rectangle(position, size), blockType);
					break;
				case EntityType.LeftWall:
					position = level.GetBlockArea(x, y).TopLeft;
					size = new Size(Level.BlockSize / 2 + Level.WallThickness / 2, Level.BlockSize);
					HandleWallCollision(Circle, new Rectangle(position, size), blockType);
					break;
				case EntityType.BottomWall:
					position = level.GetBlockArea(x, y).TopLeft + new Vector2(0, Level.BlockSize / 2 - Level.WallThickness / 2);
					size = new Size(Level.BlockSize, Level.BlockSize / 2 + Level.WallThickness);
					HandleWallCollision(Circle, new Rectangle(position, size), blockType);
					break;
				case EntityType.RightWall:
					position = level.GetBlockArea(x, y).TopLeft + new Vector2(Level.BlockSize / 2 - Level.WallThickness / 2, 0);
					size = new Size(Level.BlockSize / 2 + Level.WallThickness, Level.BlockSize);
					HandleWallCollision(Circle, new Rectangle(position, size), blockType);
					break;
				case EntityType.LeftTopWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).BottomRight, isInverse);
					break;
				case EntityType.RightTopWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).BottomLeft, isInverse);
					break;
				case EntityType.LeftBottomWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).TopRight, isInverse);
					break;
				case EntityType.RightBottomWall:
					HandleCollisionWithCurvedWall(Circle, level.GetBlockArea(x, y).TopLeft, isInverse);
					break;
				case EntityType.InverseLeftTopWall:
					isInverse = true;
					goto case EntityType.LeftTopWall;
				case EntityType.InverseRightTopWall:
					isInverse = true;
					goto case EntityType.RightTopWall;
				case EntityType.InverseLeftBottomWall:
					isInverse = true;
					goto case EntityType.LeftBottomWall;
				case EntityType.InverseRightBottomWall:
					isInverse = true;
					goto case EntityType.RightBottomWall;
				case EntityType.Wall:
					SceneNode.HandleWallCollision();
					break;
			}
		}

		/// <summary>
		///   Handles an entity collision with a horizontal or vertical wall.
		/// </summary>
		private void HandleWallCollision(Circle circle, Rectangle wall, EntityType wallType)
		{
			if (!wall.Intersects(circle))
				return;

			switch (wallType)
			{
				case EntityType.LeftWall:
					SceneNode.Position += new Vector2(wall.Right - circle.Position.X + circle.Radius, 0);
					break;
				case EntityType.RightWall:
					SceneNode.Position += new Vector2(wall.Left - circle.Position.X - circle.Radius, 0);
					break;
				case EntityType.TopWall:
					SceneNode.Position += new Vector2(0, wall.Bottom - circle.Position.Y + circle.Radius);
					break;
				case EntityType.BottomWall:
					SceneNode.Position += new Vector2(0, wall.Top - circle.Position.Y - circle.Radius);
					break;
			}

			SceneNode.HandleWallCollision();
		}

		/// <summary>
		///   Handles an entity collision with a curved wall.
		/// </summary>
		private void HandleCollisionWithCurvedWall(Circle entityCircle, Vector2 wallPosition, bool isInverse)
		{
			var wallCircle = new Circle(wallPosition, Level.BlockSize / 2 + Level.WallThickness / 2);
			if (!entityCircle.Intersects(wallCircle))
				return;

			var distance = Vector2.Distance(entityCircle.Position, wallCircle.Position);
			if (isInverse)
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