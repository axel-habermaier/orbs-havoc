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
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Allows a player to control an entity.
	/// </summary>
	internal class PlayerInputBehavior : Behavior<Entity>
	{
		private const float MaxSpeed = 4000;
		private const float MaxAcceleration = 4000;
		private const float Drag = .85f;
		private Vector2 _acceleration;

		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="target">The target the ship should be facing, relative to the ship's position.</param>
		/// <param name="moveUp">Indicates whether the player should move up.</param>
		/// <param name="moveDown">Indicates whether the player should move down.</param>
		/// <param name="moveLeft">Indicates whether the player should move to the left.</param>
		/// <param name="moveRight">Indicates whether the player should move to the right.</param>
		public void Handle(Vector2 target, bool moveUp, bool moveDown, bool moveLeft, bool moveRight)
		{
			// Compute the angular velocity, considering the target orientation.
			// We always use the shortest path to rotate towards the target.
			SceneNode.Orientation = MathUtils.ToAngle(target);

			// Update the acceleration of the entity
			_acceleration = Vector2.Zero;

			if (moveLeft)
				_acceleration += new Vector2(-1, 0);
			if (moveRight)
				_acceleration += new Vector2(1, 0);
			if (moveUp)
				_acceleration += new Vector2(0, -1);
			if (moveDown)
				_acceleration += new Vector2(0, 1);

			if (_acceleration != Vector2.Zero)
				_acceleration = Vector2.Normalize(_acceleration);
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			SceneNode.Velocity += _acceleration * MaxAcceleration * elapsedSeconds;
			SceneNode.Velocity *= Drag;

			if (SceneNode.Velocity.LengthSquared() > MaxSpeed * MaxSpeed)
				SceneNode.Velocity = MaxSpeed * Vector2.Normalize(SceneNode.Velocity);

			SceneNode.Position += SceneNode.Velocity * elapsedSeconds;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static PlayerInputBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var input = allocator.Allocate<PlayerInputBehavior>();
			input._acceleration = Vector2.Zero;
			return input;
		}
	}
}