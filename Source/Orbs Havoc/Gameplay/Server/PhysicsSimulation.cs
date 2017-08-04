namespace OrbsHavoc.Gameplay.Server
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Behaviors;
	using SceneNodes;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Simulates the game's physics, in particular checking for entity collisions.
	/// </summary>
	internal class PhysicsSimulation
	{
		private readonly List<Entity> _cachedList = new List<Entity>();
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
		///   Removes the given collider, causing collisions to no longer be registered for its associated entity.
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

		/// <summary>
		///   Gets all entities contained in the given area.
		/// </summary>
		/// <param name="area">The area that should be checked.</param>
		public List<Entity> GetEntitiesInArea(Circle area)
		{
			_cachedList.Clear();

			foreach (var collider in _colliders)
			{
				if (collider.Circle.Intersects(area))
					_cachedList.Add(collider.SceneNode);
			}

			return _cachedList;
		}

		/// <summary>
		///   Casts a ray into the world and returns the result of the nearest entity collision.
		/// </summary>
		/// <param name="start">The start position of the ray.</param>
		/// <param name="normalizedDirection">The normalized direction of the ray.</param>
		/// <param name="length">The length of the ray.</param>
		/// <param name="predicate">The predicate that determines whether a scene node is considered for collision.</param>
		public RayCastResult RayCast(Vector2 start, Vector2 normalizedDirection, float length, Func<SceneNode, bool> predicate)
		{
			Assert.ArgumentSatisfies(length > 0, nameof(length), "Invalid length.");
			Assert.ArgumentNotNull(predicate, nameof(predicate));

			var direction = normalizedDirection * length;
			var offset = 1.0f;
			Entity entity = null;

			// Check for collisions with entities
			foreach (var collider in _colliders)
			{
				// Check if we're supposed to ignore the collider's scene node
				if (!predicate(collider.SceneNode))
					continue;

				// There can be no collisions if the collider is completely out of range
				var d = length + collider.Radius;
				var center = collider.SceneNode.WorldPosition;
				if (Vector2.DistanceSquared(center, start) > d * d)
					continue;

				// Otherwise, check for an intersection, taking the closer one (there might be two, obviously)
				// see http://math.stackexchange.com/questions/311921/get-location-of-vector-circle-intersection
				var startCenter = start - center;
				var a = Vector2.Dot(direction, direction);
				var b = 2 * (direction.X * startCenter.X + direction.Y * startCenter.Y);
				var c = Vector2.Dot(startCenter, startCenter) - collider.Radius * collider.Radius;

				// There is no intersection if the discriminant is negative
				var discriminant = b * b - 4 * a * c;
				if (discriminant < 0)
					continue;

				discriminant = MathUtils.Sqrt(discriminant);
				var t = 2 * c / (-b + discriminant);

				if (t < 0 || t > 1 || t >= offset)
					continue;

				offset = t;
				entity = collider.SceneNode;
			}

			return new RayCastResult(entity, offset * length);
		}

		/// <summary>
		///   Contains the result of a ray cast.
		/// </summary>
		public struct RayCastResult
		{
			/// <summary>
			///   Gets the length of the ray from the start position to the nearest collision or the maximum ray length
			///   if there is no collision.
			/// </summary>
			public float Length { get; }

			/// <summary>
			///   Gets the entity the ray collided with or null if there is no entity collision.
			/// </summary>
			public Entity Entity { get; }

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			public RayCastResult(Entity entity, float length)
			{
				Length = length;
				Entity = entity;
			}
		}
	}
}