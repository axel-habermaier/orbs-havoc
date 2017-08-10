namespace OrbsHavoc.Rendering.Particles
{
	/// <summary>
	///   Changes the scales of the particles.
	/// </summary>
	internal sealed class ScaleModifier : Modifier
	{
		/// <summary>
		///   The scale delta per second.
		/// </summary>
		public float Delta;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="delta">The scale delta per second.</param>
		public ScaleModifier(float delta = 0)
		{
			Delta = delta;
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

			if (Delta < 0)
			{
				while (count-- > 0)
				{
					var scale = *scales + Delta * elapsedSeconds;
					*scales = scale < 0 ? 0 : scale;
					scales += 1;
				}
			}
			else
			{
				while (count-- > 0)
				{
					*scales += Delta * elapsedSeconds;
					scales += 1;
				}
			}
		}
	}
}