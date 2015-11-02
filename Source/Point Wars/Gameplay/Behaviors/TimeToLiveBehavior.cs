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
	using Platform.Memory;
	using Scene;
	using Utilities;

	/// <summary>
	///   Removes scene nodes after a specific amount of time.
	/// </summary>
	public class TimeToLiveBehavior : Behavior<SceneNode>
	{
		/// <summary>
		///   The remaining number of seconds before the scene node is removed.
		/// </summary>
		private float _remainingTime;

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			_remainingTime -= elapsedSeconds;

			if (_remainingTime < 0)
				SceneNode.Remove();
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="seconds">The amount of time to wait in seconds before the scene node is removed.</param>
		public static TimeToLiveBehavior Create(PoolAllocator allocator, float seconds)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var behavior = allocator.Allocate<TimeToLiveBehavior>();
			behavior._remainingTime = seconds;
			return behavior;
		}
	}
}