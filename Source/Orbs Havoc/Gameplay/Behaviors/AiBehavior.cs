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

namespace OrbsHavoc.Gameplay.Behaviors
{
	using System;
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Represents an AI that controls an orb.
	/// </summary>
	internal class AiBehavior : Behavior<Orb>
	{
		private PlayerInputBehavior _inputBehavior;

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			SceneNode.AddBehavior(_inputBehavior);
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			var up = false;
			var down = false;
			var left = false;
			var right = false;

			var target = Vector2.Zero;
			var enemy = GetNearestAvatar();

			if (enemy != null)
			{
				if (Vector2.DistanceSquared(SceneNode.WorldPosition, enemy.WorldPosition) > 40000)
				{
					up = SceneNode.WorldPosition.Y > enemy.WorldPosition.Y;
					left = SceneNode.WorldPosition.X > enemy.WorldPosition.X;
					down = !up;
					right = !left;
				}

				target = enemy.WorldPosition - SceneNode.WorldPosition;
			}

			_inputBehavior.HandleInput(target, up, down, left, right, target != Vector2.Zero, false);
		}

		/// <summary>
		///   Gets the orb closest to the AI.
		/// </summary>
		private Orb GetNearestAvatar()
		{
			Orb nearest = null;
			var nearestDistance = Single.MaxValue;

			foreach (var orb in SceneNode.SceneGraph.EnumeratePostOrder<Orb>())
			{
				if (orb == SceneNode || orb.PowerUp == EntityType.Invisibility)
					continue;

				var distance = Vector2.DistanceSquared(orb.WorldPosition, SceneNode.WorldPosition);
				if (!(distance < nearestDistance))
					continue;

				nearestDistance = distance;
				nearest = orb;
			}

			return nearest;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static AiBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var ai = allocator.Allocate<AiBehavior>();
			ai._inputBehavior = PlayerInputBehavior.Create(allocator);
			return ai;
		}
	}
}