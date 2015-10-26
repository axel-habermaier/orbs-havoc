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

namespace PointWars.Platform.Input
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents a binary trigger that combines two sub-triggers.
	/// </summary>
	internal sealed class BinaryInputTrigger : InputTrigger
	{
		/// <summary>
		///   The left sub-trigger.
		/// </summary>
		private readonly InputTrigger _left;

		/// <summary>
		///   The right sub-trigger.
		/// </summary>
		private readonly InputTrigger _right;

		/// <summary>
		///   Determines the type of the trigger.
		/// </summary>
		private readonly BinaryInputTriggerType _triggerType;

		/// <summary>
		///   Indicates whether a ChordOnce trigger should trigger again.
		/// </summary>
		private bool _shouldTrigger = true;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="triggerType">Determines the type of the trigger.</param>
		/// <param name="left">The left sub-trigger.</param>
		/// <param name="right">The right sub-trigger.</param>
		internal BinaryInputTrigger(BinaryInputTriggerType triggerType, InputTrigger left, InputTrigger right)
		{
			Assert.ArgumentInRange(triggerType, nameof(triggerType));
			Assert.ArgumentNotNull(left, nameof(left));
			Assert.ArgumentNotNull(right, nameof(right));

			_triggerType = triggerType;
			_left = left;
			_right = right;
		}

		/// <summary>
		///   Evaluates the trigger, returning true to indicate that the trigger has fired.
		/// </summary>
		/// <param name="device">The logical input device that should be used to evaluate the trigger.</param>
		internal override bool Evaluate(LogicalInputDevice device)
		{
			Assert.ArgumentNotNull(device, nameof(device));

			switch (_triggerType)
			{
				case BinaryInputTriggerType.ChordOnce:
					var isTriggered = _left.Evaluate(device) && _right.Evaluate(device);
					if (isTriggered && _shouldTrigger)
					{
						_shouldTrigger = false;
						return true;
					}

					if (!isTriggered)
						_shouldTrigger = true;

					return false;

				case BinaryInputTriggerType.Chord:
					return _left.Evaluate(device) && _right.Evaluate(device);

				case BinaryInputTriggerType.Alias:
					return _left.Evaluate(device) || _right.Evaluate(device);

				default:
					return false;
			}
		}

		/// <summary>
		///   Sets the logical input device the logical input is currently registered on.
		/// </summary>
		/// <param name="device">
		///   The logical input device the logical input is currently registered on. Null should be passed to
		///   indicate that the logical input is currently not registered on any device.
		/// </param>
		internal override void SetLogicalDevice(LogicalInputDevice device)
		{
			_left.SetLogicalDevice(device);
			_right.SetLogicalDevice(device);
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return String.Format("({0} {2} {1})", _left, _right, _triggerType.ToExpressionString());
		}
	}
}