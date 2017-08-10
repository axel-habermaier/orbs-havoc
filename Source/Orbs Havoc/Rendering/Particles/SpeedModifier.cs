namespace OrbsHavoc.Rendering.Particles
{
	using System;
	using System.Numerics;

	/// <summary>
	///   Changes the speed of the particles.
	/// </summary>
	internal sealed class SpeedModifier : Modifier
	{
		/// <summary>
		///   The minimum speed in percent of the initial speed.
		/// </summary>
		public float MinimumSpeedPercentage = 5;

		/// <summary>
		///   Executes the modifier, updating the given number of particles contained in the particles collection.
		/// </summary>
		/// <param name="particles">The particles that should be updated.</param>
		/// <param name="count">The number of particles that should be updated.</param>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override unsafe void Execute(ParticleCollection particles, int count, float elapsedSeconds)
		{
			var initialSpeeds = particles.InitialSpeeds;
			var velocities = particles.Velocities;
			var ages = particles.Ages;

			while (count-- > 0)
			{
				var age = *ages;
				var speed = Math.Max(*initialSpeeds * MinimumSpeedPercentage / 100, *initialSpeeds * (age * age * age));
				*velocities = Vector2.Normalize(*velocities) * speed;

				velocities += 1;
				initialSpeeds += 1;
				ages += 1;
			}
		}
	}
}