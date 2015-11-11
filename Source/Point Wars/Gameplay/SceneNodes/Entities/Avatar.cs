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

namespace PointWars.Gameplay.SceneNodes.Entities
{
	using System;
	using System.Numerics;
	using Assets;
	using Behaviors;
	using Network.Messages;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a player avatar.
	/// </summary>
	internal class Avatar : Entity
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Avatar()
		{
			Type = EntityType.Avatar;
			WeaponEnergyLevels[EntityType.MiniGun.GetWeaponSlot()] = 1;
		}

		/// <summary>
		///   Gets the energy levels of the avatar's weapons.
		/// </summary>
		public int[] WeaponEnergyLevels { get; } = new int[Game.WeaponCount];

		/// <summary>
		///   Gets or sets the avatar's primary weapon.
		/// </summary>
		public EntityType PrimaryWeapon { get; set; }

		/// <summary>
		///   Gets or sets the avatar's secondary weapon.
		/// </summary>
		public EntityType SecondaryWeapon { get; set; }

		/// <summary>
		///   Gets the avatar's player input behavior in server mode.
		/// </summary>
		public PlayerInputBehavior PlayerInput { get; private set; }

		/// <summary>
		///   Gets or sets the power up that currently influences the avatar.
		/// </summary>
		public EntityType PowerUp { get; set; }

		/// <summary>
		///   Gets or sets the remaining time until the power up is removed.
		/// </summary>
		public float RemainingPowerUpTime { get; set; }

		/// <summary>
		///   Gets or sets the avatar's remaining health.
		/// </summary>
		public float Health { get; set; }

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
						Health = Math.Min(100, Health + Game.HealthCollectibleHealthIncrease);
						entity.Remove();
					}
					break;
			}
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			Player.RemainingRespawnDelay = Game.RespawnDelay;
			Player.Avatar = null;

			if (!GameSession.ServerMode)
				SceneGraph.Add(ParticleEffectNode.Create(GameSession.Allocator, GameSession.Effects.AvatarExplosion, WorldPosition));
		}

		/// <summary>
		///   Applies the given damage to the avatar.
		/// </summary>
		/// <param name="player">The player that causes the damage.</param>
		/// <param name="damage">The damage that should be applied.</param>
		public void ApplyDamage(Player player, float damage)
		{
			Assert.ArgumentNotNull(player, nameof(player));

			Health -= PowerUp == EntityType.Armor ? damage * Game.ArmorDamageFactor : damage;
			if (Health > 0)
				return;

			++player.Kills;
			++Player.Deaths;

			GameSession.Broadcast(PlayerKillMessage.Create(GameSession.Allocator, player.Identity, Player.Identity));
			Remove();
		}

		/// <summary>
		///   Broadcasts update messages for the entity.
		/// </summary>
		public override void BroadcastUpdates()
		{
			base.BroadcastUpdates();

			// TODO: PowerUpMessage
			// TODO: WeaponMessage
			return;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the avatar belongs to.</param>
		/// <param name="position">The position of the avatar.</param>
		/// <param name="orientation">The orientation of the avatar.</param>
		public static Avatar Create(GameSession gameSession, Player player, Vector2 position, float orientation)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			var avatar = gameSession.Allocate<Avatar>();
			avatar.GameSession = gameSession;
			avatar.Player = player;
			avatar.PowerUp = EntityType.None;
			avatar.PrimaryWeapon = EntityType.MiniGun;
			avatar.SecondaryWeapon = EntityType.None;
			avatar.RemainingPowerUpTime = 0;
			avatar.Health = Game.MaxAvatarHealth;
			avatar.Position = position;
			avatar.Orientation = orientation;

			// Reset the weapon energy levels, skipping the mini gun which can always be used
			for (var i = 1; i < Game.WeaponCount; ++i)
				avatar.WeaponEnergyLevels[i] = 0;

			player.Avatar = avatar;
			gameSession.SceneGraph.Add(avatar);

			if (gameSession.ServerMode)
			{
				avatar.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 32));

				if (player.Kind == PlayerKind.Bot)
					avatar.AddBehavior(AiBehavior.Create(gameSession.Allocator));
				else
					avatar.AddBehavior(avatar.PlayerInput = PlayerInputBehavior.Create(gameSession.Allocator));
			}
			else
			{
				SpriteNode.Create(gameSession.Allocator, avatar, AssetBundle.Avatar, new Color(0xFFFFD800), 200);
				ParticleEffectNode.Create(gameSession.Allocator, gameSession.Effects.AvatarTrail, Vector2.Zero).AttachTo(avatar);
			}

			return avatar;
		}
	}
}