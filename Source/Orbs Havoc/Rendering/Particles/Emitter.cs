// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Rendering.Particles
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Emits, updates, and removes particles of a particle effect, with all particles sharing the same properties and modifiers.
	/// </summary>
	public sealed class Emitter : DisposableObject
	{
		/// <summary>
		///   The number of times per second that the emitter searches for dead particles and removes them.
		/// </summary>
		private const int RemovalRate = 30;

		/// <summary>
		///   The maximum number of live particles supported by the emitter.
		/// </summary>
		private int _capacity;

		/// <summary>
		///   The number of active particles.
		/// </summary>
		private int _particleCount;

		/// <summary>
		///   The particles managed by the emitter.
		/// </summary>
		private ParticleCollection _particles;

		/// <summary>
		///   The number of seconds since the last particle was emitted.
		/// </summary>
		private float _secondsSinceLastEmit;

		/// <summary>
		///   The number of seconds since the emitter removed dead particles.
		/// </summary>
		private float _secondsSinceLastRemoval;

		/// <summary>
		///   The emitter's spawn position, i.e., the location where all new particles will be spawned.
		/// </summary>
		private Vector2 _spawnPosition;

		/// <summary>
		///   The total seconds since the emitter emitted was first updated.
		/// </summary>
		private double _totalSeconds;

		/// <summary>
		///   Gets the modifiers affecting the particles of the emitter.
		/// </summary>
		public List<Modifier> Modifiers { get; } = new List<Modifier>();

		/// <summary>
		///   Gets or sets the maximum number of live particles supported by the emitter.
		/// </summary>
		public int Capacity
		{
			get { return _capacity; }
			set
			{
				if (_capacity == value)
					return;

				Assert.InRange(value, 1, Int32.MaxValue);

				_capacity = value;
				_particles.SafeDispose();
				_particles = new ParticleCollection(this, _capacity);
				_particleCount = Math.Min(_particleCount, _capacity);
			}
		}

		/// <summary>
		///   Gets or sets the amount of time in seconds that the emitter emits new particles. A value of Single.PositiveInfinity means
		///   that emitting never stops.
		/// </summary>
		public float Duration { get; set; } = Single.PositiveInfinity;

		/// <summary>
		///   Gets or sets the number of particles that are emitted per second. A value of Int32.MaxValue means that all
		///   unallocated particles are emitted at once.
		/// </summary>
		public int EmissionRate { get; set; }

		/// <summary>
		///   Gets or sets the range of the initial particle colors.
		/// </summary>
		public Range<Color> ColorRange { get; set; }

		/// <summary>
		///   Gets or sets the range of the initial particle orientations.
		/// </summary>
		public Range<float> OrientationRange { get; set; }

		/// <summary>
		///   Gets or sets the range of the initial particle directions. A range of [0; 2 * Pi] means that the particles are
		///   emitted in every direction.
		/// </summary>
		public Range<float> Direction { get; set; } = new Range<float>(0, MathUtils.TwoPi);

		/// <summary>
		///   Gets or sets the range of the initial particle scales.
		/// </summary>
		public Range<float> ScaleRange { get; set; } = 1;

		/// <summary>
		///   Gets or sets the range of the initial particle life time.
		/// </summary>
		public Range<float> LiftetimeRange { get; set; }

		/// <summary>
		///   Gets or sets the range of the initial particle speeds.
		/// </summary>
		public Range<float> SpeedRange { get; set; }

		/// <summary>
		///   Gets or sets the texture that is used to draw the emitter's particles.
		/// </summary>
		public Texture Texture { get; set; }

		/// <summary>
		///   Gets a value indicating whether the emitter is completed, i.e., there are no living particles and no new particles will
		///   be emitted.
		/// </summary>
		public bool IsCompleted => _particleCount == 0 && _totalSeconds > Duration;

		/// <summary>
		///   Sets the emitter's spawn position, i.e., the location where all new particles will be spawned.
		/// </summary>
		public void SetSpawnPosition(Vector2 position)
		{
			_spawnPosition = position;
		}

		/// <summary>
		///   Resets the particle emitter, restarting it.
		/// </summary>
		internal void Reset()
		{
			_totalSeconds = 0;
			_particleCount = 0;
			_secondsSinceLastEmit = 0;
			_secondsSinceLastRemoval = 0;

			foreach (var modifier in Modifiers)
				modifier.ResetState();
		}

		/// <summary>
		///   Updates the particles already emitted by the emitter, removing dead ones. Emits new particles, if necessary and
		///   appropriate.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		internal void Update(float elapsedSeconds)
		{
			Validate();
			_totalSeconds += elapsedSeconds;

			RemoveParticles(elapsedSeconds);
			UpdateParticles(elapsedSeconds);
			EmitParticles(elapsedSeconds);

			foreach (var modifier in Modifiers)
				modifier.Execute(_particles, _particleCount, elapsedSeconds);
		}

		/// <summary>
		///   Emits new particles, if necessary and appropriate.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		private unsafe void EmitParticles(float elapsedSeconds)
		{
			if (_totalSeconds > Duration)
				return;

			_secondsSinceLastEmit += elapsedSeconds;
			var count = (int)(EmissionRate * _secondsSinceLastEmit);
			count = Math.Min(count, _particles.Capacity - _particleCount);

			if (count <= 0)
				return;

			var lifetimes = _particles.Lifetimes + _particleCount;
			var initialLifetimes = _particles.InitialLifetimes + _particleCount;
			var age = _particles.Ages + _particleCount;
			var positions = _particles.Positions + _particleCount;
			var velocities = _particles.Velocities + _particleCount;
			var initialSpeeds = _particles.InitialSpeeds + _particleCount;
			var colors = _particles.Colors + _particleCount;
			var orientations = _particles.Orientations + _particleCount;
			var scales = _particles.Scales + _particleCount;

			_secondsSinceLastEmit = 0;
			_particleCount += count;

			while (count-- > 0)
			{
				*positions = _spawnPosition;
				*initialSpeeds = RandomNumbers.NextSingle(SpeedRange.LowerBound, SpeedRange.UpperBound);
				*velocities =
					MathUtils.Rotate(Vector2.UnitX, RandomNumbers.NextSingle(Direction.LowerBound, Direction.UpperBound)) * *initialSpeeds;
				*initialLifetimes = RandomNumbers.NextSingle(LiftetimeRange.LowerBound, LiftetimeRange.UpperBound);
				*lifetimes = *initialLifetimes;
				*age = 1;
				*scales = RandomNumbers.NextSingle(ScaleRange.LowerBound, ScaleRange.UpperBound);
				*orientations = RandomNumbers.NextSingle(OrientationRange.LowerBound, OrientationRange.UpperBound);
				*colors = new Color(
					RandomNumbers.NextByte(ColorRange.LowerBound.Red, ColorRange.UpperBound.Red),
					RandomNumbers.NextByte(ColorRange.LowerBound.Green, ColorRange.UpperBound.Green),
					RandomNumbers.NextByte(ColorRange.LowerBound.Blue, ColorRange.UpperBound.Blue),
					RandomNumbers.NextByte(ColorRange.LowerBound.Alpha, ColorRange.UpperBound.Alpha));

				positions += 1;
				velocities += 1;
				initialSpeeds += 1;
				lifetimes += 1;
				initialLifetimes += 1;
				age += 1;
				colors += 1;
				orientations += 1;
				scales += 1;
			}
		}

		/// <summary>
		///   Updates the life times and the positions of the particles.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		private unsafe void UpdateParticles(float elapsedSeconds)
		{
			var lifetimes = _particles.Lifetimes;
			var initialLifetime = _particles.InitialLifetimes;
			var age = _particles.Ages;
			var positions = _particles.Positions;
			var velocities = _particles.Velocities;
			var count = _particleCount;

			while (count-- > 0)
			{
				var lifetime = *lifetimes - elapsedSeconds;
				lifetime = lifetime < 0 ? 0 : lifetime;

				*lifetimes = lifetime;
				*age = lifetime / *initialLifetime;
				*positions += *velocities * elapsedSeconds;

				++lifetimes;
				++initialLifetime;
				++age;
				++positions;
				++velocities;
			}
		}

		/// <summary>
		///   Removes all dead particles.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		private unsafe void RemoveParticles(float elapsedSeconds)
		{
			// We don't want to search for dead particles during each update for performance reasons.
			_secondsSinceLastRemoval += elapsedSeconds;
			if (_secondsSinceLastRemoval < 1.0f / RemovalRate)
				return;

			_secondsSinceLastRemoval = 0;
			for (var i = 0; i < _particleCount; ++i)
			{
				if (_particles.Lifetimes[i] > 0)
					continue;

				_particles.Copy(source: _particleCount - 1, target: i);
				--_particleCount;
			}
		}

		/// <summary>
		///   Draws the particles of the emitter to the given render output.
		/// </summary>
		/// <param name="spriteBatch">The SpriteBatch the particles should be drawn with.</param>
		internal unsafe void Draw(SpriteBatch spriteBatch)
		{
			Validate();

			if (_particleCount == 0)
				return;

			var quads = spriteBatch.AddQuads(_particleCount, Texture);
			var positions = _particles.Positions;
			var orientations = _particles.Orientations;
			var scales = _particles.Scales;
			var colors = _particles.Colors;
			var size = Texture.Size;
			var count = _particleCount;

			while (count-- > 0)
			{
				*quads = new Quad
				{
					Color = *colors,
					Orientation = *orientations,
					Position = *positions,
					Size = *scales * size,
					TextureCoordinates = Rectangle.Unit
				};

				++quads;
				++positions;
				++orientations;
				++scales;
				++colors;
			}
		}

		/// <summary>
		///   In debug builds, validates the configuration of the emitter.
		/// </summary>
		[Conditional("DEBUG"), DebuggerHidden]
		private void Validate()
		{
			Assert.InRange(EmissionRate, 0, Int32.MaxValue);
			Assert.InRange(Capacity, 1, Int32.MaxValue);
			Assert.That(LiftetimeRange.LowerBound >= 0, "Invalid particle life time.");
			Assert.That(LiftetimeRange.UpperBound >= 0, "Invalid particle life time.");
			Assert.That(Duration > 0 || Single.IsPositiveInfinity(Duration), "Invalid duration.");
			Assert.That(SpeedRange.LowerBound >= 0, "Invalid particle speed.");
			Assert.That(SpeedRange.UpperBound >= 0, "Invalid particle speed.");
			Assert.NotNull(Texture, "No texture has been specified.");
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_particles.SafeDispose();
		}
	}
}