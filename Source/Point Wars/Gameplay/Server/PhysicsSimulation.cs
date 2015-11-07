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

namespace PointWars.Gameplay.Server
{
	using System.Collections.Generic;
	using System.Numerics;
	using Behaviors;
	using Platform.Logging;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Simulates the game's physics, in particular checking for entity collisions.
	/// </summary>
	internal class PhysicsSimulation
	{
		private readonly List<ColliderBehavior> _colliders = new List<ColliderBehavior>();
		private readonly GameSession _gameSession;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the physics simulation is provided for.</param>
		public PhysicsSimulation(GameSession gameSession)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			_gameSession = gameSession;
		}

		/// <summary>
		///   Adds the given collider, causing collisions to be registered for its associated entity.
		/// </summary>
		/// <param name="collider">The collider that should be added.</param>
		public void AddCollider(ColliderBehavior collider)
		{
			Assert.ArgumentNotNull(collider, nameof(collider));
			Assert.ArgumentSatisfies(!_colliders.Contains(collider), nameof(collider), "The collider has already been added.");

			_colliders.Add(collider);
		}

		/// <summary>
		///   Reooves the given collider, causing collisions to no longer be registered for its associated entity.
		/// </summary>
		/// <param name="collider">The collider that should be removed.</param>
		public void RemoveCollider(ColliderBehavior collider)
		{
			Assert.ArgumentNotNull(collider, nameof(collider));
			Assert.ArgumentSatisfies(_colliders.Contains(collider), nameof(collider), "The collider has not been added.");

			_colliders.Remove(collider);
		}

		/// <summary>
		///   Updates the physics simulation.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			// First, move all entities
			foreach (var entity in _gameSession.SceneGraph.EnumeratePostOrder<Entity>())
				entity.Position += entity.Velocity * elapsedSeconds;

			// Secondly, check for collisions
			for (var i = 0; i < _colliders.Count; ++i)
			{
				// Check for collisions with static world geometry
				int x, y;
				_gameSession.Level.GetBlock(_colliders[i].Circle.Position, out x, out y);

				switch (_gameSession.Level[x, y])
				{
					case BlockType.HorizontalWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var circle = _colliders[i].Circle;
						var newY = HandleCollisionWithNonCurvedWall(circle.Position.Y, circle.Radius, area.Center.Y);
						_colliders[i].SceneNode.Position = new Vector2(circle.Position.X, newY);
						break;
					}
					case BlockType.VerticalWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var circle = _colliders[i].Circle;
						var newX = HandleCollisionWithNonCurvedWall(circle.Position.X, circle.Radius, area.Center.X);
						_colliders[i].SceneNode.Position = new Vector2(newX, circle.Position.Y);
						break;
					}
					case BlockType.LeftTopWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var offset = HandleCollisionWithCurvedWall(_colliders[i].Circle, area.BottomRight);
						_colliders[i].SceneNode.Position += offset;
						break;
					}
					case BlockType.RightTopWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var offset = HandleCollisionWithCurvedWall(_colliders[i].Circle, area.BottomLeft);
						_colliders[i].SceneNode.Position += offset;
						break;
					}
					case BlockType.LeftBottomWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var offset = HandleCollisionWithCurvedWall(_colliders[i].Circle, area.TopRight);
						_colliders[i].SceneNode.Position += offset;
						break;
					}
					case BlockType.RightBottomWall:
					{
						var area = _gameSession.Level.GetBlockArea(x, y);
						var offset = HandleCollisionWithCurvedWall(_colliders[i].Circle, area.TopLeft);
						_colliders[i].SceneNode.Position += offset;
						break;
					}
				}

				// Check for collisions with other entities
				for (var j = i + 1; j < _colliders.Count; ++j)
				{
					var collider1 = _colliders[i];
					var collider2 = _colliders[j];

					var entity1 = collider1.SceneNode;
					var entity2 = collider2.SceneNode;

					if (!collider1.Circle.Intersects(collider2.Circle))
						continue;

					entity1.HandleCollision(entity2);
					entity2.HandleCollision(entity1);
				}
			}
		}

		/// <summary>
		///   Handles an entity collision with a horizontal or vertical wall.
		/// </summary>
		private static float HandleCollisionWithNonCurvedWall(float circlePosition, float radius, float areaCenter)
		{
			if (MathUtils.Abs(circlePosition - areaCenter) > radius + Level.WallThickness / 2)
				return circlePosition;

			if (circlePosition < areaCenter)
				circlePosition -= circlePosition + radius - areaCenter + Level.WallThickness / 2;
			else
				circlePosition += areaCenter + Level.WallThickness / 2 - circlePosition + radius;

			return circlePosition;
		}

		/// <summary>
		///   Handles an entity collision with a curved wall.
		/// </summary>
		private static Vector2 HandleCollisionWithCurvedWall(Circle entityCircle, Vector2 wallPosition)
		{
			var wallCircle = new Circle(wallPosition, Level.BlockSize / 2 + Level.WallThickness / 2);
			if (!entityCircle.Intersects(wallCircle))
				return Vector2.Zero;

			var distance = Vector2.Distance(entityCircle.Position, wallCircle.Position);
			if (distance < wallCircle.Radius - Level.WallThickness / 2)
			{
				var overlap = distance + entityCircle.Radius - wallCircle.Radius + Level.WallThickness;
				if (overlap > 0)
					return -Vector2.Normalize(entityCircle.Position - wallCircle.Position) * overlap;
				
				return Vector2.Zero;
			}
			else
			{
				var radius = entityCircle.Radius + wallCircle.Radius;
				var overlap = MathUtils.Abs(distance - radius);
				return -Vector2.Normalize(wallCircle.Position - entityCircle.Position) * overlap;
			}
		}
	}
}