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
				_colliders[i].HandleWallCollisions(_gameSession.Level);

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

		
	}
}