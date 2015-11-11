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
	using System.Numerics;
	using Behaviors;
	using Utilities;

	/// <summary>
	///   Represents a bullet fired by a mini gun.
	/// </summary>
	internal class Bullet : Entity
	{
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
			if (entity.Type != EntityType.Avatar || Player == entity.Player)
				return;

			var avatar = (Avatar)entity;
			avatar.ApplyDamage(Player, Game.MiniGunTemplate.Damage);

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

			SceneGraph.Add(ParticleEffectNode.Create(GameSession.Allocator, explosion, WorldPosition));
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

			gameSession.SceneGraph.Add(bullet);

			if (gameSession.ServerMode)
				bullet.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 8));
			else
			{
				var effect = gameSession.Effects.Bullet.Allocate();

				effect.Emitters[0].ColorRange = player.ColorRange;
				effect.Emitters[0].OrientationRange = -orientation;
				effect.Emitters[0].Direction = MathUtils.ToAngle(velocity);

				ParticleEffectNode.Create(gameSession.Allocator, effect, Vector2.Zero).AttachTo(bullet);
			}

			return bullet;
		}
	}
}