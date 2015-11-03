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

namespace PointWars.Gameplay.Behaviors
{
	using System.Numerics;
	using Entities;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Allows a player to control an entity.
	/// </summary>
	internal class PlayerInputBehavior : Behavior<Entity>
	{
		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="target">The target the ship should be facing, relative to the ship's position.</param>
		/// <param name="forward">Indicates whether the ship should move forward.</param>
		/// <param name="backward">Indicates whether the ship should move backward.</param>
		/// <param name="strafeLeft">Indicates whether the ship should strafe to the left.</param>
		/// <param name="strafeRight">Indicates whether the ship should strafe to the right.</param>
		public void Handle(Vector2 target, bool forward, bool backward, bool strafeLeft, bool strafeRight)
		{
			if (forward)
				SceneNode.Velocity += new Vector2(1, 0);
			if (backward)
				SceneNode.Velocity += new Vector2(-1, 0);
			if (strafeLeft)
				SceneNode.Velocity += new Vector2(0, -1);
			if (strafeRight)
				SceneNode.Velocity += new Vector2(0, 1);
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			SceneNode.Position += SceneNode.Velocity * elapsedSeconds;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static PlayerInputBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			return allocator.Allocate<PlayerInputBehavior>();
		}
	}
}