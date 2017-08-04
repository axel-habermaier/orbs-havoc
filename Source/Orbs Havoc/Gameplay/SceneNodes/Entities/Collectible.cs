namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	using System;
	using System.Numerics;
	using Behaviors;
	using Client;
	using Utilities;

	/// <summary>
	///   Represents a collectible within a level.
	/// </summary>
	internal class Collectible : Entity
	{
		/// <summary>
		///   Invoked when the entity is removed from a game session.
		/// </summary>
		/// <remarks>This method is not called when the game session is disposed.</remarks>
		public override void OnRemoved()
		{
			if (GameSession.ServerMode)
				return;

			AddEffect();
		}

		/// <summary>
		///   Adds the effect that visually signals the collectible being spawned or collected.
		/// </summary>
		private void AddEffect()
		{
			var effect = GameSession.Effects.Collectible.Allocate();
			effect.Emitters[0].ColorRange = Type.GetColor();
			effect.Emitters[0].ScaleRange = Type.GetTexture().Height / 32;

			ParticleEffectNode.Create(GameSession.Allocator, effect, WorldPosition).AttachTo(GameSession.SceneGraph);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the collectible belongs to.</param>
		/// <param name="position">The position of the collectible.</param>
		/// <param name="collectibleType">The type of the collectible.</param>
		public static Collectible Create(GameSession gameSession, Vector2 position, EntityType collectibleType)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentSatisfies(collectibleType.IsCollectible(), nameof(collectibleType), "Expected a collectible type.");

			var collectible = gameSession.Allocate<Collectible>();
			collectible.GameSession = gameSession;
			collectible.Position = position;
			collectible.Type = collectibleType;
			collectible.Player = gameSession.Players.ServerPlayer;

			gameSession.SceneGraph.Add(collectible);

			if (gameSession.ServerMode)
			{
				Assert.That(Math.Abs(collectibleType.GetTexture().Width - collectibleType.GetTexture().Height) <= 0, "Expected a square texture.");
				collectible.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, collectibleType.GetTexture().Width / 2));
			}
			else
			{
				var node = gameSession.Allocate<SceneNode>();
				var sprite = SpriteBehavior.Create(gameSession, collectibleType.GetTexture(), collectibleType.GetColor(), 100);
				node.AddBehavior(sprite);
				node.AddBehavior(CircleMovementBehavior.Create(gameSession.Allocator, 2f, 4));
				collectible.AddEffect();
				collectible.AttachChild(node);
			}

			return collectible;
		}

		public static class Health
		{
			public const float RespawnDelay = 10;
			public const float HealthIncrease = 20;
		}
	}
}