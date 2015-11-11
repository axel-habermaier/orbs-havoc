// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Gameplay.Client
{
	using System.Numerics;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Reflects particles when the hit a level wall.
	/// </summary>
	public class ParticleReflectionModifier : Modifier
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
			var radius = size.Width < size.Height ? size.Height : size.Width;

			while (--count > 0)
			{
				var circle = new Circle(*positions, *scales * radius);
				var collisionInfo = _level.CheckWallCollision(circle);

				if (collisionInfo != null)
				{
					var v = *velocities;
					var vn = Vector2.Reflect(v, collisionInfo.Value.Normal);
					*velocities = vn;
				}

				positions += 1;
				velocities += 1;
				scales += 1;
			}
		}
	}
}