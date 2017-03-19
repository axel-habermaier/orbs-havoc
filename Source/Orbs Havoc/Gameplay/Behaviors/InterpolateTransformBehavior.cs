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

namespace OrbsHavoc.Gameplay.Behaviors
{
	using System.Numerics;
	using Network;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Interpolates the transformation information of a client-side entity.
	/// </summary>
	internal class InterpolateTransformBehavior : Behavior<Entity>
	{
		private bool _firstUpdate;
		private uint _lastTransformUpdateSequenceNumber;
		private float _orientation;
		private Vector2 _position;
		private float _timeSinceLastUpdate;

		/// <summary>
		///   Updates the entity's transformation based on the data in the given message.
		/// </summary>
		/// <param name="position">The updated position.</param>
		/// <param name="orientation">The updated orientation.</param>
		/// <param name="sequenceNumber">The sequence number of the dispatched message.</param>
		public void UpdateTransform(Vector2 position, float orientation, uint sequenceNumber)
		{
			if (!Entity.AcceptUpdate(ref _lastTransformUpdateSequenceNumber, sequenceNumber))
				return;

			_position = position;
			_orientation = orientation;
			_timeSinceLastUpdate = 0;

			if (!_firstUpdate)
				return;

			SceneNode.ChangeLocalTransformation(_position, _orientation);
			_firstUpdate = false;
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			_timeSinceLastUpdate += elapsedSeconds;

			var percentage = _timeSinceLastUpdate * NetworkProtocol.ServerUpdateFrequency;
			var position = Vector2.Lerp(SceneNode.Position, _position, percentage);
			SceneNode.ChangeLocalTransformation(position, _orientation);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		public static InterpolateTransformBehavior Create(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var interpolator = allocator.Allocate<InterpolateTransformBehavior>();
			interpolator._position = Vector2.Zero;
			interpolator._orientation = 0;
			interpolator._lastTransformUpdateSequenceNumber = 0;
			interpolator._firstUpdate = true;
			interpolator._timeSinceLastUpdate = 0;

			return interpolator;
		}
	}
}