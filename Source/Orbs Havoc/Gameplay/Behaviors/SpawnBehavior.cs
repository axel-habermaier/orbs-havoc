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

namespace OrbsHavoc.Gameplay.Behaviors
{
	using System;
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Periodically spawns a collectible.
	/// </summary>
	internal class SpawnBehavior : Behavior<SceneNode>
	{
		private EntityType _collectibleType;
		private float _cooldown;
		private Entity _entity;
		private GameSession _gameSession;
		private Vector2 _position;

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			if (_entity != null && _entity.IsRemoved)
			{
				_entity.SafeDispose();
				_entity = null;
				_cooldown = GetCooldown();

				return;
			}

			if (_entity != null)
				return;

			_cooldown -= elapsedSeconds;
			if (!(_cooldown <= 0))
				return;

			_entity = Collectible.Create(_gameSession, _position, _collectibleType);
			_entity.AcquireOwnership();
		}

		/// <summary>
		///   Gets the cooldown for the spawner's collectible.
		/// </summary>
		/// <returns></returns>
		private float GetCooldown()
		{
			switch (_collectibleType)
			{
				case EntityType.Armor:
					return Constants.PowerUps.Armor.ArmorRespawnDelay;
				case EntityType.Regeneration:
					return Constants.PowerUps.Regeneration.RegenerationRespawnDelay;
				case EntityType.QuadDamage:
					return Constants.PowerUps.QuadDamage.QuadDamageRespawnDelay;
				case EntityType.Speed:
					return Constants.PowerUps.Speed.SpeedRespawnDelay;
				case EntityType.Invisibility:
					return Constants.PowerUps.Invisibility.RespawnDelay;
				case EntityType.Health:
					return Constants.HealthCollectible.HealthRespawnDelay;
				default:
					throw new InvalidOperationException("Unsupported collectible type.");
			}
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			_entity.SafeDispose();
			_entity = null;
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the spawned entities belongs to.</param>
		/// <param name="position">The location of the spawned collectibles.</param>
		/// <param name="collectibleType">The type of the collectible spawned by the spawner.</param>
		public static SpawnBehavior Create(GameSession gameSession, Vector2 position, EntityType collectibleType)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentSatisfies(collectibleType.IsCollectible(), nameof(collectibleType), "Expected a collectible type.");

			var spawner = gameSession.Allocate<SpawnBehavior>();
			spawner._gameSession = gameSession;
			spawner._collectibleType = collectibleType;
			spawner._cooldown = 0;
			spawner._entity = null;
			spawner._position = position;

			return spawner;
		}
	}
}