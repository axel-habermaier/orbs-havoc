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
		///   The number of weapons that can be used by an avatar.
		/// </summary>
		public static readonly int WeaponCount = Enum.GetValues(typeof(WeaponType)).Length - 1;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Avatar()
		{
			Type = EntityType.Avatar;
			WeaponEnergyLevels[(int)WeaponType.MiniGun] = 1;
		}

		/// <summary>
		///   Gets the energy levels of the avatar's weapons.
		/// </summary>
		public byte[] WeaponEnergyLevels { get; } = new byte[WeaponCount];

		/// <summary>
		///   Gets or sets the avatar's primary weapon.
		/// </summary>
		public WeaponType PrimaryWeapon { get; set; }

		/// <summary>
		///   Gets or sets the avatar's secondary weapon.
		/// </summary>
		public WeaponType SecondaryWeapon { get; set; }

		/// <summary>
		///   Gets the avatar's player input behavior in server mode.
		/// </summary>
		public PlayerInputBehavior PlayerInput { get; private set; }

		/// <summary>
		///   Gets or sets the power up that currently influences the avatar.
		/// </summary>
		public PowerUpType PowerUp { get; set; }

		/// <summary>
		///   Gets or sets the remaining time until the power up is removed.
		/// </summary>
		public float RemainingPowerUpTime { get; set; }

		/// <summary>
		///   Gets or sets the avatar's remaining health.
		/// </summary>
		public int Health { get; set; }

		/// <summary>
		///   Updates the state of the server-side entity.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void ServerUpdate(float elapsedSeconds)
		{
			if (PowerUp != PowerUpType.None)
			{
				RemainingPowerUpTime -= elapsedSeconds;
				if (RemainingPowerUpTime < 0)
					PowerUp = PowerUpType.None;
			}

			if (Health <= 0)
				Remove();
		}

		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			Player.Avatar = null;
		}

		/// <summary>
		/// Applies the given damage to the avatar.
		/// </summary>
		/// <param name="damage">The damage that should be applied.</param>
		public void ApplyDamage(int damage)
		{
			Health -= PowerUp == PowerUpType.Armor ? damage / 2 : damage;
		}

		/// <summary>
		///   Broadcasts update messages for the entity.
		/// </summary>
		/// <param name="broadcast">The callback that should be used to broadcast the message.</param>
		public override void BroadcastUpdates(Action<Message> broadcast)
		{
			base.BroadcastUpdates(broadcast);

			// TODO: PowerUpMessage
			// TODO: WeaponMessage
			return;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the ship belongs to.</param>
		public static Avatar Create(GameSession gameSession, Player player)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			var avatar = gameSession.Allocate<Avatar>();
			avatar.GameSession = gameSession;
			avatar.Player = player;
			avatar.PowerUp = PowerUpType.None;
			avatar.PrimaryWeapon = WeaponType.MiniGun;
			avatar.SecondaryWeapon = WeaponType.Unknown;
			avatar.RemainingPowerUpTime = 0;
			avatar.Health = 100;

			// Reset the weapon energy levels, skipping the mini gun which can always be used
			for (var i = 1; i < WeaponCount; ++i)
				avatar.WeaponEnergyLevels[i] = 0;

			player.Avatar = avatar;
			gameSession.SceneGraph.Add(avatar);

			if (gameSession.ServerMode)
			{
				avatar.AddBehavior(avatar.PlayerInput = PlayerInputBehavior.Create(gameSession.Allocator));
				avatar.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 32));
			}
			else
				SpriteNode.Create(gameSession.Allocator, avatar, AssetBundle.Avatar, Colors.YellowGreen);

			return avatar;
		}
	}
}