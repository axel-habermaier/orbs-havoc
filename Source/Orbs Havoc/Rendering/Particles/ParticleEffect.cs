namespace OrbsHavoc.Rendering.Particles
{
	using System.Collections.Generic;
	using System.Numerics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a particle effect consisting of one or more particle emitters.
	/// </summary>
	public sealed class ParticleEffect : PooledObject
	{
		/// <summary>
		///   Gets or sets the emitters of the particle effect.
		/// </summary>
		public List<Emitter> Emitters { get; } = new List<Emitter>();

		/// <summary>
		///   Gets a value indicating whether the particle effect is completed, i.e., there are no living particles and no new
		///   particles will be emitted.
		/// </summary>
		public bool IsCompleted
		{
			get
			{
				Assert.NotNull(Emitters);
				Assert.NotPooled(this);

				foreach (var emitter in Emitters)
				{
					if (!emitter.IsCompleted)
						return false;
				}

				return true;
			}
		}

		/// <summary>
		///   Sets the spawn position of all of the particle effect's emitters.
		/// </summary>
		public void SetSpawnPosition(Vector2 position)
		{
			foreach (var emitter in Emitters)
				emitter.SetSpawnPosition(position);
		}

		/// <summary>
		///   Resets the particle effect, restarting it.
		/// </summary>
		public void Reset()
		{
			Assert.NotPooled(this);

			if (Emitters == null)
				return;

			foreach (var emitter in Emitters)
				emitter.Reset();
		}

		/// <summary>
		///   Updates the particle effect.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			Assert.NotPooled(this);

			if (Emitters == null)
				return;

			foreach (var emitter in Emitters)
				emitter.Update(elapsedSeconds);
		}

		/// <summary>
		///   Draws the particle effect to the given render output.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch the particle effect should be drawn with.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			if (Emitters == null)
				return;

			foreach (var emitter in Emitters)
				emitter.Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Emitters.SafeDisposeAll();
		}
	}
}