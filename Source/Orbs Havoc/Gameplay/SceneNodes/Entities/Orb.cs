namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using System;
	using System.Numerics;
	using Assets;
	using Behaviors;
	using Network.Messages;
	using Rendering;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Represents a player orb.
	/// </summary>
	internal class Orb : Entity
	{
		public const float CriticalHealthThreshold = 35;
		public const float MaxHealth = 100;
		public const float MaxRegenerationHealth = 200;
		public const float MaxHealthLimitExceededDecrease = 5;
		public const float RespawnDelay = 2;
		public const int WeaponsCount = 8;

		private ParticleEffect _coreEffect;
		private uint _lastUpdateSequenceNumber;
		private float _nextHealthUpdate;
		private ParticleEffectNode _regeneration;
		private SpriteBehavior _sprite;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Orb()
		{
			Type = EntityType.Orb;
			WeaponEnergyLevels[EntityType.MiniGun.GetWeaponSlot()] = 1;
		}

		/// <summary>
		///   Gets the energy levels of the orb's weapons.
		/// </summary>
		public int[] WeaponEnergyLevels { get; } = new int[WeaponsCount];

		/// <summary>
		///   Gets or sets the orb's primary weapon.
		/// </summary>
		public EntityType PrimaryWeapon { get; set; }

		/// <summary>
		///   Gets or sets the orb's secondary weapon.
		/// </summary>
		public EntityType SecondaryWeapon { get; set; }

		/// <summary>
		///   Gets the orb's player input behavior in server mode.
		/// </summary>
		public PlayerInputBehavior PlayerInput { get; private set; }

		/// <summary>
		///   Gets or sets the power up that currently influences the orb.
		/// </summary>
		public EntityType PowerUp { get; set; }

		/// <summary>
		///   Gets or sets the remaining time until the power up is removed.
		/// </summary>
		public float RemainingPowerUpTime { get; set; }

		/// <summary>
		///   Gets or sets the orb's remaining health.
		/// </summary>
		public float Health { get; set; }

		/// <summary>
		///   Gets a value indicating whether the orb's health has reached a critical level.
		/// </summary>
		public bool HasCriticalHealth => Health <= CriticalHealthThreshold;

		/// <summary>
		///   Updates the state of the server-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void ServerUpdate(float elapsedSeconds)
		{
			if (PowerUp != EntityType.None)
			{
				RemainingPowerUpTime -= elapsedSeconds;
				if (RemainingPowerUpTime < 0)
					PowerUp = EntityType.None;
			}

			_nextHealthUpdate -= elapsedSeconds;
			if (PowerUp == EntityType.Regeneration && _nextHealthUpdate <= 0)
			{
				Health = Math.Min(MaxRegenerationHealth, Health + PowerUps.Regeneration.HealthIncrease);
				_nextHealthUpdate = 1;
			}

			if (PowerUp != EntityType.Regeneration && Health > MaxHealth && _nextHealthUpdate <= 0)
			{
				Health = Math.Max(MaxHealth, Health - MaxHealthLimitExceededDecrease);
				_nextHealthUpdate = 1;
			}
		}

		/// <summary>
		///   Updates the state of the client-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void ClientUpdate(float elapsedSeconds)
		{
			switch (PowerUp)
			{
				case EntityType.Invisibility:
					var hsv = Player.Color.ToHsv();
					hsv.Z /= Player.IsLocalPlayer ? 4 : 10;
					var color = Color.FromHsv(hsv);

					_sprite.Color = color;
					_coreEffect.Emitters[0].ColorRange = color;
					break;
				case EntityType.Regeneration:
					if (_regeneration == null)
					{
						_regeneration = ParticleEffectNode.Create(GameSession.Allocator, GameSession.Effects.Regeneration, Vector2.Zero);
						_regeneration.AttachTo(this);
					}
					break;
				default:
					_sprite.Color = Player.Color;
					_coreEffect.Emitters[0].ColorRange = Player.ColorRange;

					if (_regeneration != null)
					{
						_regeneration.Remove();
						_regeneration = null;
					}

					break;
			}

			_coreEffect.Emitters[0].ScaleRange = MathUtils.Clamp(Health / MaxHealth, 0.2f, 1.5f);
		}

		/// <summary>
		///   Handles the collision with the given entity.
		/// </summary>
		/// <param name="entity">The entity this entity collided with.</param>
		public override void HandleCollision(Entity entity)
		{
			switch (entity.Type)
			{
				case EntityType.Health:
					if (Health < 100)
					{
						Health = Math.Min(100, Health + Collectible.Health.HealthIncrease);
						entity.Remove();
					}
					break;
				case EntityType.Regeneration:
					CollectPowerUp(entity, PowerUps.Regeneration.Time);
					break;
				case EntityType.QuadDamage:
					CollectPowerUp(entity, PowerUps.QuadDamage.Time);
					break;
				case EntityType.Invisibility:
					CollectPowerUp(entity, PowerUps.Invisibility.Time);
					break;
			}
		}

		/// <summary>
		///   Collects the power up, if possible.
		/// </summary>
		private void CollectPowerUp(Entity powerUp, float powerUpTime)
		{
			if (PowerUp != EntityType.None)
				return;

			PowerUp = powerUp.Type;
			RemainingPowerUpTime = powerUpTime;
			powerUp.Remove();
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			Player.RemainingRespawnDelay = RespawnDelay;
			Player.Orb = null;

			if (GameSession.ServerMode)
				return;

			var explosion = GameSession.Effects.AvatarExplosion.Allocate();
			explosion.Emitters[0].ColorRange = Player.ColorRange;

			SceneGraph.Add(ParticleEffectNode.Create(GameSession.Allocator, explosion, WorldPosition));
		}

		/// <summary>
		///   Applies the given damage to the orb.
		/// </summary>
		/// <param name="attacker">The attacking player that causes the damage.</param>
		/// <param name="damage">The damage that should be applied.</param>
		public void ApplyDamage(Player attacker, float damage)
		{
			Assert.ArgumentNotNull(attacker, nameof(attacker));

			if (Health <= 0)
				return;

			Health -= PowerUp == EntityType.Armor ? damage * PowerUps.Armor.DamageFactor : damage;
			if (Health > 0)
				return;

			// Only increase the kill count if the player didn't commit suicide... otherwise, 
			// constant self-killing might win a game!
			if (attacker != Player)
				attacker.Kills += 1;

			Player.Deaths += 1;

			GameSession.Broadcast(PlayerKillMessage.Create(GameSession.Allocator, attacker.Identity, Player.Identity));
			Remove();
		}

		/// <summary>
		///   Broadcasts update messages for the entity.
		/// </summary>
		public override void BroadcastUpdates()
		{
			base.BroadcastUpdates();
			GameSession.Broadcast(UpdateOrbMessage.Create(GameSession.Allocator, this));
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		public void OnUpdate(UpdateOrbMessage message, uint sequenceNumber)
		{
			if (!AcceptUpdate(ref _lastUpdateSequenceNumber, sequenceNumber))
				return;

			PowerUp = message.PowerUp;
			RemainingPowerUpTime = message.RemainingPowerUpTime;
			Health = message.Health;
			PrimaryWeapon = message.PrimaryWeapon;
			SecondaryWeapon = message.SecondaryWeapon;

			for (var i = 0; i < WeaponsCount; ++i)
				WeaponEnergyLevels[i] = message.WeaponEnergyLevels[i];
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the orb belongs to.</param>
		/// <param name="position">The position of the orb.</param>
		/// <param name="orientation">The orientation of the orb.</param>
		public static Orb Create(GameSession gameSession, Player player, Vector2 position, float orientation)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			var orb = gameSession.Allocate<Orb>();
			orb.GameSession = gameSession;
			orb.Player = player;
			orb.PowerUp = EntityType.None;
			orb.PrimaryWeapon = EntityType.MiniGun;
			orb.SecondaryWeapon = EntityType.None;
			orb.RemainingPowerUpTime = 0;
			orb.Health = MaxHealth;
			orb.Position = position;
			orb.Orientation = orientation;
			orb._nextHealthUpdate = 0;
			orb._coreEffect = null;
			orb._regeneration = null;
			orb._sprite = null;
			orb._lastUpdateSequenceNumber = 0;

			// Reset the weapon energy levels, skipping the mini gun which can always be used
			for (var i = 1; i < WeaponsCount; ++i)
				orb.WeaponEnergyLevels[i] = Weapons.WeaponTemplates[i].MaxEnergy;

			player.Orb = orb;
			gameSession.SceneGraph.Add(orb);

			if (gameSession.ServerMode)
			{
				orb.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 32));

				if (player.Kind == PlayerKind.Bot)
					orb.AddBehavior(AiBehavior.Create(gameSession.Allocator));
				else
					orb.AddBehavior(orb.PlayerInput = PlayerInputBehavior.Create(gameSession.Allocator));
			}
			else
			{
				orb._coreEffect = gameSession.Effects.AvatarCore.Allocate();
				orb._coreEffect.Emitters[0].ColorRange = player.ColorRange;
				orb._coreEffect.Emitters[0].EmissionRate = (int)MaxHealth;
				orb._coreEffect.Emitters[0].ScaleRange = new Range<float>(0.6f, 1.1f);

				ParticleEffectNode.Create(gameSession.Allocator, orb._coreEffect, Vector2.Zero).AttachTo(orb);
				orb._sprite = SpriteBehavior.Create(gameSession, AssetBundle.Orb, player.Color, 200);
				orb.AddBehavior(orb._sprite);
			}

			return orb;
		}
	}
}