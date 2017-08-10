namespace OrbsHavoc.Rendering.Particles
{
	using Utilities;

	/// <summary>
	///   Sets the orientation of particles in accordance with their velocities.
	/// </summary>
	internal class VelocityOrientationModifier : Modifier
	{
		/// <summary>
		///   Executes the modifier, updating the given number of particles contained in the particles collection.
		/// </summary>
		/// <param name="particles">The particles that should be updated.</param>
		/// <param name="count">The number of particles that should be updated.</param>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override unsafe void Execute(ParticleCollection particles, int count, float elapsedSeconds)
		{
			var orientations = particles.Orientations;
			var velocities = particles.Velocities;

			while (count-- > 0)
			{
				*orientations = -MathUtils.ToAngle(*velocities);

				orientations += 1;
				velocities += 1;
			}
		}
	}
}