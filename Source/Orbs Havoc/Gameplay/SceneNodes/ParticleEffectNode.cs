namespace OrbsHavoc.Gameplay.SceneNodes
{
	using System.Numerics;
	using Platform.Memory;
	using Rendering;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Represents a node that emits and draws particles.
	/// </summary>
	internal class ParticleEffectNode : SceneNode
	{
		private ParticleEffect _effect;

		/// <summary>
		///   Invoked when the scene node is detached from its scene graph.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			_effect.SafeDispose();
		}

		/// <summary>
		///   Updates the particle effect.
		/// </summary>
		/// <param name="elapsedSeconds">The amount of seconds that has passed since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			_effect.SetSpawnPosition(WorldPosition);
			_effect.Update(elapsedSeconds);

			if (_effect.IsCompleted)
				Remove();
		}

		/// <summary>
		///   Draws the particle effect using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			_effect.Draw(spriteBatch);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the sprite.</param>
		/// <param name="effectTemplate">The template of the particle effect that should be added to the scene graph.</param>
		/// <param name="position">The node's initial position.</param>
		public static ParticleEffectNode Create(PoolAllocator allocator, ParticleEffectTemplate effectTemplate, Vector2 position)
		{
			return Create(allocator, effectTemplate.Allocate(), position);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the sprite.</param>
		/// <param name="effect">The particle effect that should be added to the scene graph.</param>
		/// <param name="position">The node's initial position.</param>
		public static ParticleEffectNode Create(PoolAllocator allocator, ParticleEffect effect, Vector2 position)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var sprite = allocator.Allocate<ParticleEffectNode>();
			sprite.Position = position;
			sprite._effect = effect;

			return sprite;
		}
	}
}