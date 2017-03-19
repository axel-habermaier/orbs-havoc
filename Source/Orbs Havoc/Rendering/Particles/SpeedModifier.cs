// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using System.Numerics;

	/// <summary>
	///   Changes the speed of the particles.
	/// </summary>
	public sealed class SpeedModifier : Modifier
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