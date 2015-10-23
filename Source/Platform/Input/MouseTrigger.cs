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
	///   Represents a trigger that triggers if a mouse button is in a certain state.
	/// </summary>
	internal class MouseTrigger : InputTrigger
	{
		/// <summary>
		///   The mouse button that is monitored by the trigger.
		/// </summary>
		private readonly MouseButton _button;

		/// <summary>
		///   Determines the type of the trigger.
		/// </summary>
		private readonly MouseTriggerType _triggerType;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="triggerType"> Determines the type of the trigger.</param>
		/// <param name="button">The mouse button that is monitored by the trigger.</param>
		internal MouseTrigger(MouseTriggerType triggerType, MouseButton button)
		{
			Assert.ArgumentInRange(triggerType, nameof(triggerType));
			Assert.ArgumentInRange(button, nameof(button));

			_triggerType = triggerType;
			_button = button;
		}

		/// <summary>
		///   Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		///   true if the current object is equal to other; otherwise, false.
		/// </returns>
		public override bool Equals(InputTrigger other)
		{
			var trigger = other as MouseTrigger;
			if (trigger == null)
				return false;

			return _button == trigger._button && _triggerType == trigger._triggerType;
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
				case MouseTriggerType.Released:
					return !device.Mouse.IsPressed(_button);
				case MouseTriggerType.WentDown:
					return device.Mouse.WentDown(_button);
				case MouseTriggerType.Pressed:
					return device.Mouse.IsPressed(_button);
				case MouseTriggerType.WentUp:
					return device.Mouse.WentUp(_button);
				default:
					return false;
			}
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return String.Format("Mouse({1}, {0})", _triggerType, _button);
		}
	}
}