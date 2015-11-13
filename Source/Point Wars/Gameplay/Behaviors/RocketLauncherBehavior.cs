﻿// The MIT License (MIT)
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
	///   Fires a rocket when the weapon is triggered.
	/// </summary>
	internal class RocketLauncherBehavior : WeaponBehavior
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public RocketLauncherBehavior()
		{
			Template = Game.RocketLauncherTemplate;
		}

		/// <summary>
		///   Fires a single shot of a non-continuous weapon.
		/// </summary>
		protected override void Fire()
		{
			var spread = RandomNumberGenerator.NextSingle(Game.RocketLauncherTemplate.MinSpread, Game.RocketLauncherTemplate.MaxSpread);
			spread *= RandomNumberGenerator.NextInteger() % 2 == 0 ? 1 : -1;

			var directionVector = MathUtils.FromAngle(SceneNode.Orientation + spread);
			var velocity = Vector2.Normalize(directionVector) * Template.Speed;

			Rocket.Create(SceneNode.GameSession, SceneNode.Player, SceneNode.WorldPosition, velocity, SceneNode.Orientation);
		}

		/// <summary>
		///   Allocates an instance using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the behavior.</param>
		public static RocketLauncherBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			return allocator.Allocate<RocketLauncherBehavior>();
		}
	}
}