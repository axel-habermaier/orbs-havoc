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

namespace PointWars.Gameplay.Entities.Server
{
	using System.Numerics;

	internal class Avatar : Entity
	{
		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="target">The target the ship should be facing, relative to the ship's position.</param>
		/// <param name="forward">Indicates whether the ship should move forward.</param>
		/// <param name="backward">Indicates whether the ship should move backward.</param>
		/// <param name="strafeLeft">Indicates whether the ship should strafe to the left.</param>
		/// <param name="strafeRight">Indicates whether the ship should strafe to the right.</param>
		/// <param name="warp">Indicates whether the ship should enable its warp drive.</param>
		public void HandlePlayerInput(Vector2 target, bool forward, bool backward, bool strafeLeft, bool strafeRight, bool warp)
		{
			if (forward)
				Velocity += new Vector2(1, 0);
			if (backward)
				Velocity += new Vector2(-1, 0);
			if (strafeLeft)
				Velocity += new Vector2(0, -1);
			if (strafeRight)
				Velocity += new Vector2(0, 1);
		}

		/// <summary>
		///   Updates the entity when the entity is used by a server.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		public override void Update(float elapsedSeconds)
		{
			Position += Velocity * elapsedSeconds;
		}
	}
}