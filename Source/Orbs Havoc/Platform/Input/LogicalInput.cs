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

namespace OrbsHavoc.Platform.Input
{
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents a logical input, that is, an abstraction of physical input device states.
	/// </summary>
	public class LogicalInput
	{
		/// <summary>
		///   The cvar that holds the trigger, if any.
		/// </summary>
		private readonly Cvar<InputTrigger> _cvar;

		/// <summary>
		///   Determines when the input is considered to be triggered.
		/// </summary>
		private readonly TriggerType _triggerType;

		/// <summary>
		///   The input device the input is registered on.
		/// </summary>
		private LogicalInputDevice _device;

		/// <summary>
		///   Indicates whether the mouse wheel has been moved since the last update.
		/// </summary>
		private bool _mouseWheelMoved;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The trigger that triggers the logical input.</param>
		/// <param name="triggerType">Determines when the input is considered to be triggered.</param>
		public LogicalInput(InputTrigger trigger, TriggerType triggerType = TriggerType.Pressed)
		{
			Assert.ArgumentInRange(triggerType, nameof(triggerType));

			Trigger = trigger;
			_triggerType = triggerType;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The trigger that triggers the logical input.</param>
		/// <param name="triggerType">Determines when the input is considered to be triggered.</param>
		public LogicalInput(Cvar<InputTrigger> trigger, TriggerType triggerType = TriggerType.Pressed)
			: this(trigger.Value, triggerType)
		{
			_cvar = trigger;
		}

		/// <summary>
		///   Gets the trigger that triggers the input.
		/// </summary>
		public InputTrigger Trigger { get; private set; }

		/// <summary>
		///   Gets or sets a value indicating whether the input is registered on a logical input device.
		/// </summary>
		public bool IsRegistered => _device != null;

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
			_device = device;
			IsTriggered = false;

			if (_cvar != null && _device != null)
				_cvar.Changed += OnCvarChanged;
			else if (_cvar != null && _device == null)
				_cvar.Changed -= OnCvarChanged;

			OnTriggerOrDeviceChanged();
		}

		/// <summary>
		///   Handles trigger changes.
		/// </summary>
		private void OnCvarChanged()
		{
			Trigger = _cvar.Value;
			OnTriggerOrDeviceChanged();
		}

		/// <summary>
		///   Handles trigger or input device changes.
		/// </summary>
		private void OnTriggerOrDeviceChanged()
		{
			Assert.That(Trigger.Key != null || Trigger.MouseButton != null || Trigger.MouseWheelDirection != null,
				"Invalid trigger: Neither key nor mouse button nor mouse wheel is set.");

			if (_device == null)
				return;

			_device.Mouse.Wheel -= OnMouseWheel;

			if (Trigger.MouseWheelDirection != null)
				_device.Mouse.Wheel += OnMouseWheel;
		}

		/// <summary>
		///   Handles mouse wheel inputs.
		/// </summary>
		private void OnMouseWheel(MouseWheelDirection direction)
		{
			if (Trigger.MouseWheelDirection != null)
				_mouseWheelMoved = direction == Trigger.MouseWheelDirection.Value;
		}

		/// <summary>
		///   Evaluates the input's trigger and stores the result in the IsTriggered property.
		/// </summary>
		internal void Update()
		{
			IsTriggered = false;

			if (Trigger.Key != null)
			{
				switch (_triggerType)
				{
					case TriggerType.Released:
						if (_device.Keyboard.IsPressed(Trigger.Key.Value))
							return;
						break;
					case TriggerType.WentDown:
						if (!_device.Keyboard.WentDown(Trigger.Key.Value))
							return;
						break;
					case TriggerType.Pressed:
						if (!_device.Keyboard.IsPressed(Trigger.Key.Value))
							return;
						break;
					case TriggerType.WentUp:
						if (!_device.Keyboard.WentUp(Trigger.Key.Value))
							return;
						break;
					case TriggerType.Repeated:
						if (!_device.Keyboard.IsRepeated(Trigger.Key.Value))
							return;
						break;
					default:
						Assert.NotReached("Unsupported trigger type.");
						break;
				}
			}

			if (Trigger.MouseButton != null)
			{
				switch (_triggerType)
				{
					case TriggerType.Released:
						if (_device.Mouse.IsPressed(Trigger.MouseButton.Value))
							return;
						break;
					case TriggerType.WentDown:
						if (!_device.Mouse.WentDown(Trigger.MouseButton.Value))
							return;
						break;
					case TriggerType.Pressed:
						if (!_device.Mouse.IsPressed(Trigger.MouseButton.Value))
							return;
						break;
					case TriggerType.WentUp:
						if (!_device.Mouse.WentUp(Trigger.MouseButton.Value))
							return;
						break;
					default:
						Assert.NotReached("Unsupported trigger type.");
						break;
				}
			}

			if (Trigger.MouseWheelDirection != null)
			{
				if (!_mouseWheelMoved)
					return;

				_mouseWheelMoved = false;
			}

			var modifiers = Trigger.Modifiers;
			var alt = _device.Keyboard.IsPressed(Key.LeftAlt) || _device.Keyboard.IsPressed(Key.RightAlt);
			var shift = _device.Keyboard.IsPressed(Key.LeftShift) || _device.Keyboard.IsPressed(Key.RightShift);
			var control = _device.Keyboard.IsPressed(Key.LeftControl) || _device.Keyboard.IsPressed(Key.RightControl);

			if ((modifiers & KeyModifiers.Alt) != 0 && (modifiers & KeyModifiers.Shift) != 0 && (modifiers & KeyModifiers.Control) != 0)
				IsTriggered = alt && shift && control;
			else if ((modifiers & KeyModifiers.Shift) != 0 && (modifiers & KeyModifiers.Control) != 0)
				IsTriggered = !alt && shift && control;
			else if ((modifiers & KeyModifiers.Alt) != 0 && (modifiers & KeyModifiers.Control) != 0)
				IsTriggered = alt && !shift && control;
			else if ((modifiers & KeyModifiers.Alt) != 0 && (modifiers & KeyModifiers.Shift) != 0)
				IsTriggered = alt && shift && !control;
			else if ((modifiers & KeyModifiers.Alt) != 0)
				IsTriggered = alt && !shift && !control;
			else if ((modifiers & KeyModifiers.Shift) != 0)
				IsTriggered = !alt && shift && !control;
			else if ((modifiers & KeyModifiers.Control) != 0)
				IsTriggered = !alt && !shift && control;
			else
				IsTriggered = true;
		}
	}
}