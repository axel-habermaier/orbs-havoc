namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using System.Numerics;
	using Behaviors;
	using Utilities;

	/// <summary>
	///   Represents a bullet fired by a mini gun.
	/// </summary>
	internal class Bullet : Entity
	{
		private Vector2 _visualOffset;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Bullet()
		{
			Type = EntityType.Bullet;
		}

		/// <summary>
		///   Handles the collision with the given entity.
		/// </summary>
		/// <param name="entity">The entity this entity collided with.</param>
		public override void HandleCollision(Entity entity)
		{
			if (entity.Type != EntityType.Orb || Player == entity.Player)
				return;

			var orb = (Orb)entity;
			var damageMultiplier = Player.Orb != null && Player.Orb.PowerUp == EntityType.QuadDamage ? PowerUps.QuadDamage.DamageMultiplier : 1;
			orb.ApplyDamage(Player, Weapons.MiniGun.Damage * damageMultiplier);

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
				return;

			var explosion = GameSession.Effects.BulletExplosion.Allocate();
			explosion.Emitters[0].ColorRange = Player.ColorRange;

			SceneGraph.Add(ParticleEffectNode.Create(GameSession.Allocator, explosion, WorldPosition + _visualOffset));
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the bullet belongs to.</param>
		/// <param name="position">The initial position of the bullet.</param>
		/// <param name="velocity">The initial velocity of the bullet.</param>
		/// <param name="orientation">The bullet's orientation.</param>
		public static Bullet Create(GameSession gameSession, Player player, Vector2 position, Vector2 velocity, float orientation)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(player, nameof(player));

			var bullet = gameSession.Allocate<Bullet>();
			bullet.GameSession = gameSession;
			bullet.Player = player;
			bullet.Position = position;
			bullet.Velocity = velocity;
			bullet.Orientation = orientation;
			bullet._visualOffset = Vector2.Normalize(velocity) * 15;

			gameSession.SceneGraph.Add(bullet);

			if (gameSession.ServerMode)
				bullet.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 8));
			else
			{
				var effect = gameSession.Effects.Bullet.Allocate();

				effect.Emitters[0].ColorRange = player.ColorRange;
				effect.Emitters[0].OrientationRange = -orientation;
				effect.Emitters[0].Direction = MathUtils.ToAngle(velocity);

				ParticleEffectNode.Create(gameSession.Allocator, effect, bullet._visualOffset).AttachTo(bullet);
			}

			return bullet;
		}
	}
}