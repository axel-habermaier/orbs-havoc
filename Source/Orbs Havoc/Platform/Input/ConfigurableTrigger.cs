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
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents a trigger with the actual trigger evaluation being performed by a trigger held by a cvar. The cvar's value
	///   can be changed at any time, changing the behavior of the configurable trigger accordingly.
	/// </summary>
	internal class ConfigurableTrigger : InputTrigger
	{
		/// <summary>
		///   The cvar that holds the actual configurable input.
		/// </summary>
		private readonly Cvar<ConfigurableInput> _configurableInput;

		/// <summary>
		///   Determines the type of a key input trigger.
		/// </summary>
		private readonly KeyTriggerType _keyTriggerType;

		/// <summary>
		///   Determines the type of a mouse input trigger.
		/// </summary>
		private readonly MouseTriggerType _mouseTriggerType;

		/// <summary>
		///   The logical input device that is currently used to evaluate the trigger.
		/// </summary>
		private LogicalInputDevice _device;

		/// <summary>
		///   The input trigger generated from the configurable input.
		/// </summary>
		private InputTrigger _inputTrigger;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="configurableInput">The configurable input that triggers the logical input.</param>
		/// <param name="keyTriggerType">Determines the type of a key input trigger.</param>
		/// <param name="mouseTriggerType">Determines the type of a mouse input trigger.</param>
		internal ConfigurableTrigger(Cvar<ConfigurableInput> configurableInput, KeyTriggerType keyTriggerType, MouseTriggerType mouseTriggerType)
		{
			Assert.ArgumentNotNull(configurableInput, nameof(configurableInput));
			Assert.ArgumentInRange(keyTriggerType, nameof(keyTriggerType));
			Assert.ArgumentInRange(mouseTriggerType, nameof(mouseTriggerType));

			_configurableInput = configurableInput;
			_keyTriggerType = keyTriggerType;
			_mouseTriggerType = mouseTriggerType;

			_inputTrigger = configurableInput.Value.ToInputTrigger(keyTriggerType, mouseTriggerType);
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

			if (device != null && _device == null)
				_configurableInput.Changed += OnConfigurableInputChanged;

			if (device == null && _device != null)
				_configurableInput.Changed -= OnConfigurableInputChanged;

			_device = device;
			_inputTrigger.SetLogicalDevice(device);
		}

		/// <summary>
		///   Recreates the input trigger created from the configurable input.
		/// </summary>
		private void OnConfigurableInputChanged()
		{
			_inputTrigger = _configurableInput.Value.ToInputTrigger(_keyTriggerType, _mouseTriggerType);
		}

		/// <summary>
		///   Evaluates the trigger, returning true to indicate that the trigger has fired.
		/// </summary>
		/// <param name="device">The logical input device that should be used to evaluate the trigger.</param>
		internal override bool Evaluate(LogicalInputDevice device)
		{
			Assert.NotNull(_inputTrigger, "Input trigger not created.");
			return _inputTrigger.Evaluate(device);
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"Cvar({_configurableInput.Name}: {_inputTrigger})";
		}
	}
}