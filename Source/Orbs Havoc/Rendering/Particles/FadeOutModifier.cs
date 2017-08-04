namespace OrbsHavoc.Rendering.Particles
{
	/// <summary>
	///   Fades out the particles based on their remaining life time.
	/// </summary>
	public sealed class FadeOutModifier : Modifier
	{
		/// <summary>
		///   Executes the modifier, updating the given number of particles contained in the particles collection.
		/// </summary>
		/// <param name="particles">The particles that should be updated.</param>
		/// <param name="count">The number of particles that should be updated.</param>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override unsafe void Execute(ParticleCollection particles, int count, float elapsedSeconds)
		{
			var color = particles.Colors;
			var age = particles.Ages;

			while (count-- > 0)
			{
				color->Alpha = (byte)(*age * 255);

				color += 1;
				age += 1;
			}
		}
	}
}