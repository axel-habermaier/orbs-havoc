﻿namespace OrbsHavoc.Platform.Input
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

			Validate();
		}

		/// <summary>
		///   Handles trigger changes.
		/// </summary>
		private void OnCvarChanged()
		{
			Trigger = _cvar.Value;
			Validate();
		}

		/// <summary>
		///   Checks whether the input configuration is valid.
		/// </summary>
		private void Validate()
		{
			Assert.That(Trigger.Key != null || Trigger.MouseButton != null || Trigger.MouseWheelDirection != null,
				"Invalid trigger: Neither key nor mouse button nor mouse wheel is set.");
		}

		/// <summary>
		///   Evaluates the input's trigger and stores the result in the IsTriggered property.
		/// </summary>
		internal void Update()
		{
			IsTriggered = false;

			if (Trigger.Key is Key key)
			{
				switch (_triggerType)
				{
					case TriggerType.Released:
						if (_device.Keyboard.IsPressed(key))
							return;
						break;
					case TriggerType.WentDown:
						if (!_device.Keyboard.WentDown(key))
							return;
						break;
					case TriggerType.Pressed:
						if (!_device.Keyboard.IsPressed(key))
							return;
						break;
					case TriggerType.WentUp:
						if (!_device.Keyboard.WentUp(key))
							return;
						break;
					case TriggerType.Repeated:
						if (!_device.Keyboard.IsRepeated(key))
							return;
						break;
					default:
						Assert.NotReached("Unsupported trigger type.");
						break;
				}
			}

			if (Trigger.MouseButton is MouseButton button)
			{
				switch (_triggerType)
				{
					case TriggerType.Released:
						if (_device.Mouse.IsPressed(button))
							return;
						break;
					case TriggerType.WentDown:
						if (!_device.Mouse.WentDown(button))
							return;
						break;
					case TriggerType.Pressed:
						if (!_device.Mouse.IsPressed(button))
							return;
						break;
					case TriggerType.WentUp:
						if (!_device.Mouse.WentUp(button))
							return;
						break;
					default:
						Assert.NotReached("Unsupported trigger type.");
						break;
				}
			}

			if (Trigger.MouseWheelDirection is MouseWheelDirection direction)
			{
				if (!_device.Mouse.WasTurned(direction))
					return;
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