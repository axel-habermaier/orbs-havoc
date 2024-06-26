﻿namespace OrbsHavoc.Gameplay.Behaviors
{
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
		private float GetCooldown()
		{
			switch (_collectibleType)
			{
				case EntityType.Armor:
					return PowerUps.Armor.RespawnDelay;
				case EntityType.Regeneration:
					return PowerUps.Regeneration.RespawnDelay;
				case EntityType.QuadDamage:
					return PowerUps.QuadDamage.RespawnDelay;
				case EntityType.Speed:
					return PowerUps.Speed.RespawnDelay;
				case EntityType.Invisibility:
					return PowerUps.Invisibility.RespawnDelay;
				case EntityType.Health:
					return Collectible.Health.RespawnDelay;
				default:
					Assert.NotReached("Unsupported collectible type.");
					return 0;
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