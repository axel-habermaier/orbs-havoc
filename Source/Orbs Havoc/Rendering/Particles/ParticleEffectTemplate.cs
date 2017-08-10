namespace OrbsHavoc.Rendering.Particles
{
	using System;
	using Platform.Memory;

	/// <summary>
	///   Represents a template from which a certain particle effect can be created.
	/// </summary>
	internal sealed class ParticleEffectTemplate : DisposableObject
	{
		private readonly ObjectPool<ParticleEffect> _pooledEffects;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="initializationCallback">
		///   The function that should be used by the template to initialize a newly allocated particle effect.
		/// </param>
		public ParticleEffectTemplate(Action<ParticleEffect> initializationCallback)
		{
			_pooledEffects = new ObjectPool<ParticleEffect>(initializationCallback);
		}

		/// <summary>
		///   Gets a pooled effect or allocates a new instance if none are currently pooled.
		/// </summary>
		public ParticleEffect Allocate()
		{
			var effect = _pooledEffects.Allocate();
			effect.Reset();

			return effect;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_pooledEffects.SafeDispose();
		}
	}
}