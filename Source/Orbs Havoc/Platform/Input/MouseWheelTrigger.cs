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

namespace OrbsHavoc.Platform.Input
{
	using Utilities;

	/// <summary>
	///   Represents a trigger that triggers if the mouse wheel is turned in a certain direction.
	/// </summary>
	internal class MouseWheelTrigger : InputTrigger
	{
		/// <summary>
		///   The direction the mouse wheel must be turned in order to trigger the trigger.
		/// </summary>
		private readonly MouseWheelDirection _direction;

		private LogicalInputDevice _device;
		private bool _isTriggered;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="direction">The direction the mouse wheel must be turned in order to trigger the trigger.</param>
		internal MouseWheelTrigger(MouseWheelDirection direction)
		{
			Assert.ArgumentInRange(direction, nameof(direction));
			_direction = direction;
		}

		/// <summary>
		///   Evaluates the trigger, returning true to indicate that the trigger has fired.
		/// </summary>
		/// <param name="device">The logical input device that should be used to evaluate the trigger.</param>
		internal override bool Evaluate(LogicalInputDevice device)
		{
			Assert.ArgumentNotNull(device, nameof(device));

			var isTriggered = _isTriggered;
			_isTriggered = false;
			return isTriggered;
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
			if (device == _device)
				return;

			if (_device != null)
				_device.Mouse.Wheel -= OnMouseWheel;

			if (device != null)
				device.Mouse.Wheel += OnMouseWheel;

			_device = device;
		}

		/// <summary>
		///   Handles mouse wheel inputs.
		/// </summary>
		private void OnMouseWheel(MouseWheelDirection direction)
		{
			_isTriggered = direction == _direction;
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"MouseWheel.{_direction}";
		}
	}
}