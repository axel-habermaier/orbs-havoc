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
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents a logical input, that is, an abstraction of physical input device states. In the simplest case, a logical
	///   input corresponds to a key press or mouse button press. More complex chords, i.e., mixtures of several physical
	///   input devices or more than one key/button, are also supported; for instance, a logical input might occur if a
	///   keyboard key 'left control' is pressed while the left mouse button just went down. Furthermore, logical inputs allow
	///   the specification of aliased inputs, i.e., alternatives where each alternative triggers the same action. So in
	///   addition to left control + left mouse triggering the input as in the previous example, right mouse alone might
	///   also trigger the same logical input. All of this is handled by the logical input framework, so that the application
	///   does not have to worry about any of this.
	/// </summary>
	public class LogicalInput
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The trigger that triggers the logical input.</param>
		public LogicalInput(InputTrigger trigger)
		{
			Assert.ArgumentNotNull(trigger, nameof(trigger));
			Trigger = trigger;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="configurableInput">The configurable input that triggers the logical input.</param>
		/// <param name="keyTriggerType">Determines the type of a key input trigger.</param>
		/// <param name="mouseTriggerType">Determines the type of a mouse input trigger.</param>
		public LogicalInput(Cvar<ConfigurableInput> configurableInput, KeyTriggerType keyTriggerType, MouseTriggerType mouseTriggerType)
		{
			Assert.ArgumentNotNull(configurableInput, nameof(configurableInput));
			Assert.ArgumentInRange(keyTriggerType, nameof(keyTriggerType));
			Assert.ArgumentInRange(mouseTriggerType, nameof(mouseTriggerType));

			Trigger = new ConfigurableTrigger(configurableInput, keyTriggerType, mouseTriggerType);
		}

		/// <summary>
		///   The trigger that triggers the logical input.
		/// </summary>
		internal InputTrigger Trigger { get; }

		/// <summary>
		///   Gets or sets a value indicating whether the input is registered on a logical input device.
		/// </summary>
		public bool IsRegistered { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the input is currently triggered.
		/// </summary>
		public bool IsTriggered { get; private set; }

		/// <summary>
		///   Sets the logical input device the logical input is currently registered on.
		/// </summary>
		/// <param name="device">
		///   The logical input device the logical input is currently registered on. Null should be passed to
		///   indicate that the logical input is currently not registered on any device.
		/// </param>
		internal void SetLogicalDevice(LogicalInputDevice device)
		{
			IsRegistered = device != null;
			IsTriggered = false;

			Trigger.SetLogicalDevice(device);
		}

		/// <summary>
		///   Evaluates the input's trigger and stores the result in the IsTriggered property.
		/// </summary>
		/// <param name="device">The logical input device that should be used to evaluate trigger.</param>
		/// <param name="textInputEnabled">Indicates whether text input is currently enabled.</param>
		internal void Update(LogicalInputDevice device, bool textInputEnabled)
		{
			IsTriggered = !textInputEnabled && Trigger.Evaluate(device);
		}
	}
}