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

namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using System;
	using System.Numerics;
	using Behaviors;
	using Utilities;

	/// <summary>
	///   Represents a rocket fired by a rocket launcher.
	/// </summary>
	internal class Rocket : Entity
	{
		private Vector2 _visualOffset;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Rocket()
		{
			Type = EntityType.Rocket;
		}

		/// <summary>
		///   Handles the collision with the given entity.
		/// </summary>
		/// <param name="entity">The entity this entity collided with.</param>
		public override void HandleCollision(Entity entity)
		{
			if (Player == entity.Player || entity.Type != EntityType.Orb)
				return;

			Remove();
		}

		/// <summary>
		///   Handles the collision with a level wall.
		/// </summary>
		public override void HandleWallCollision()
		{
			Remove();
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			if (GameSession.ServerMode)
			{
				var radius = Weapons.RocketLauncher.Range;
				foreach (var entity in GameSession.PhysicsSimulation.GetEntitiesInArea(new Circle(WorldPosition, radius)))
				{
					var orb = entity as Orb;
					if (orb == null)
						continue;

					var distanceMultiplier = Math.Max(0, (radius - Vector2.Distance(WorldPosition, orb.WorldPosition)) / radius);
					var damageMultiplier = Player.Orb != null && Player.Orb.PowerUp == EntityType.QuadDamage
						? PowerUps.QuadDamage.DamageMultiplier
						: 1;

					orb.ApplyDamage(Player, Weapons.RocketLauncher.Damage * damageMultiplier * distanceMultiplier);
				}
			}
			else
			{
				var explosion = GameSession.Effects.RocketExplosion.Allocate();
				explosion.Emitters[0].ColorRange = Player.ColorRange;

				SceneGraph.Add(ParticleEffectNode.Create(GameSession.Allocator, explosion, WorldPosition + _visualOffset));
			}
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the bullet belongs to.</param>
		/// <param name="position">The initial position of the bullet.</param>
		/// <param name="velocity">The initial velocity of the bullet.</param>
		/// <param name="orientation">The bullet's orientation.</param>
		public static Rocket Create(GameSession gameSession, Player player, Vector2 position, Vector2 velocity, float orientation)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(player, nameof(player));

			var rocket = gameSession.Allocate<Rocket>();
			rocket.GameSession = gameSession;
			rocket.Player = player;
			rocket.Position = position;
			rocket.Velocity = velocity;
			rocket.Orientation = orientation;
			rocket._visualOffset = Vector2.Normalize(velocity) * 30;

			gameSession.SceneGraph.Add(rocket);

			if (gameSession.ServerMode)
				rocket.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 32));
			else
			{
				var effect = gameSession.Effects.Rocket.Allocate();

				effect.Emitters[0].ColorRange = player.ColorRange;
				effect.Emitters[0].OrientationRange = -orientation;
				effect.Emitters[1].OrientationRange = -orientation;
				effect.Emitters[0].Direction = MathUtils.ToAngle(velocity);
				effect.Emitters[1].Direction = MathUtils.ToAngle(velocity);

				ParticleEffectNode.Create(gameSession.Allocator, effect, rocket._visualOffset).AttachTo(rocket);
			}

			return rocket;
		}
	}
}