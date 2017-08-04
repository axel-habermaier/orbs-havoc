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
				foreach (var entity in GameSession.Physics.GetEntitiesInArea(new Circle(WorldPosition, radius)))
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

				foreach (var emitter in effect.Emitters)
				{
					emitter.OrientationRange = -orientation;
					emitter.Direction = MathUtils.ToAngle(velocity);
				}

				ParticleEffectNode.Create(gameSession.Allocator, effect, rocket._visualOffset).AttachTo(rocket);
			}

			return rocket;
		}
	}
}