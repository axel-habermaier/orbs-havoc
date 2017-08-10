namespace OrbsHavoc.Gameplay.Client
{
	using System.Numerics;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Reflects particles when the hit a level wall.
	/// </summary>
	internal class ParticleReflectionModifier : Modifier
	{
		private readonly Level _level;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="level">The level with the walls that should reflect the particles.</param>
		public ParticleReflectionModifier(Level level)
		{
			Assert.ArgumentNotNull(level, nameof(level));
			_level = level;
		}

		/// <summary>
		///   Executes the modifier, updating the given number of particles contained in the particles collection.
		/// </summary>
		/// <param name="particles">The particles that should be updated.</param>
		/// <param name="count">The number of particles that should be updated.</param>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override unsafe void Execute(ParticleCollection particles, int count, float elapsedSeconds)
		{
			var positions = particles.Positions;
			var velocities = particles.Velocities;
			var scales = particles.Scales;
			var size = particles.Emitter.Texture.Size;
			var radius = (size.Width < size.Height ? size.Height : size.Width) / 2f;

			while (count-- > 0)
			{
				var circle = new Circle(*positions, *scales * radius);
				var collisionInfo = _level.CheckWallCollision(circle);

				// Reflect particles that move into the direction of the wall
				if (collisionInfo != null && Vector2.Dot(collisionInfo.Value.Normal, *velocities) < 0)
				{
					var reflectedVelocity = Vector2.Reflect(*velocities, collisionInfo.Value.Normal);
					*velocities = reflectedVelocity;
				}

				positions += 1;
				velocities += 1;
				scales += 1;
			}
		}
	}
}