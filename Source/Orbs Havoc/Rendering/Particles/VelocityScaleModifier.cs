namespace OrbsHavoc.Rendering.Particles
{
	using Utilities;

	/// <summary>
	///   Changes the scale of the particles based on the particles' velocities.
	/// </summary>
	public sealed class VelocityScaleModifier : Modifier
	{
		private readonly float _aboveScaleDelta;
		private readonly float _belowScaleDelta;
		private readonly float _maxScale;
		private readonly float _minScale;
		private readonly float _speedThreshold;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="minScale">The velocity-independent minimum scale of the particles.</param>
		/// <param name="maxScale">The velocity-independent maximum scale of the particles.</param>
		/// <param name="speedThreshold">The speed threshold that selects between the two scale factors.</param>
		/// <param name="belowScaleDelta">The scale delta that should be applied below the speed threshold.</param>
		/// <param name="aboveScaleDelta">The scale delta that should be applied above the speed threshold.</param>
		public VelocityScaleModifier(float minScale, float maxScale, float speedThreshold, float belowScaleDelta, float aboveScaleDelta)
		{
			_minScale = minScale;
			_maxScale = maxScale;
			_speedThreshold = speedThreshold;
			_belowScaleDelta = belowScaleDelta;
			_aboveScaleDelta = aboveScaleDelta;
		}

		/// <summary>
		///   Executes the modifier, updating the given number of particles contained in the particles collection.
		/// </summary>
		/// <param name="particles">The particles that should be updated.</param>
		/// <param name="count">The number of particles that should be updated.</param>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override unsafe void Execute(ParticleCollection particles, int count, float elapsedSeconds)
		{
			var scales = particles.Scales;
			var velocities = particles.Velocities;

			while (count-- > 0)
			{
				var speed = velocities->Length();
				var delta = speed > _speedThreshold ? _aboveScaleDelta : _belowScaleDelta;

				var scale = *scales + delta * elapsedSeconds;
				*scales = MathUtils.Clamp(scale, _minScale, _maxScale);

				scales += 1;
				velocities += 1;
			}
		}
	}
}