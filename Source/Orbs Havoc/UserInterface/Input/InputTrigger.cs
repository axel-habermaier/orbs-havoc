namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using System.Text;
	using Utilities;

	public struct InputTrigger : IEquatable<InputTrigger>
	{
		public InputTrigger(Key key, KeyModifiers? modifiers = KeyModifiers.None)
			: this()
		{
			Assert.ArgumentInRange(key, nameof(key));

			Key = key;
			Modifiers = modifiers;
		}

		public InputTrigger(MouseButton mouseButton, KeyModifiers? modifiers = KeyModifiers.None)
			: this()
		{
			Assert.ArgumentInRange(mouseButton, nameof(mouseButton));

			MouseButton = mouseButton;
			Modifiers = modifiers;
		}

		public InputTrigger(MouseWheelDirection direction, KeyModifiers? modifiers = KeyModifiers.None)
			: this()
		{
			Assert.ArgumentInRange(direction, nameof(direction));

			MouseWheelDirection = direction;
			Modifiers = modifiers;
		}

		public Key? Key { get; }
		public MouseButton? MouseButton { get; }
		public KeyModifiers? Modifiers { get; }
		public MouseWheelDirection? MouseWheelDirection { get; }

		public static implicit operator InputTrigger(Key key)
		{
			return new InputTrigger(key, KeyModifiers.None);
		}

		public static implicit operator InputTrigger(MouseButton mouseButton)
		{
			return new InputTrigger(mouseButton, KeyModifiers.None);
		}

		public static implicit operator InputTrigger(MouseWheelDirection direction)
		{
			return new InputTrigger(direction, KeyModifiers.None);
		}

		public bool Equals(InputTrigger other)
		{
			return Modifiers == other.Modifiers &&
				   Key == other.Key &&
				   MouseButton == other.MouseButton &&
				   MouseWheelDirection == other.MouseWheelDirection;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is InputTrigger && Equals((InputTrigger)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Key.GetHashCode();
				hashCode = (hashCode * 397) ^ MouseButton.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)(Modifiers ?? 0);
				return hashCode;
			}
		}

		public static bool operator ==(InputTrigger left, InputTrigger right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(InputTrigger left, InputTrigger right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("[");

			if (MouseWheelDirection is MouseWheelDirection direction)
				builder.Append("MouseWheel." + direction);

			if (Key is Key key)
				builder.Append("Key." + key);

			if (MouseButton is MouseButton button)
				builder.Append("Mouse." + button);

			if (Modifiers is KeyModifiers modifiers)
			{
				if ((modifiers & KeyModifiers.Alt) != 0)
					builder.Append("+Alt");

				if ((modifiers & KeyModifiers.Shift) != 0)
					builder.Append("+Shift");

				if ((modifiers & KeyModifiers.Control) != 0)
					builder.Append("+Control");
			}
			else
				builder.Append("+Any");

			builder.Append("]");
			return builder.ToString();
		}

		public bool IsTriggered(Keyboard keyboard, Mouse mouse, TriggerType triggerType = TriggerType.Pressed)
		{
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentInRange(triggerType, nameof(triggerType));

			if (Keyboard.TextInputEnabled)
				return false;

			if (Key is Key key)
			{
				switch (triggerType)
				{
					case TriggerType.Pressed when keyboard.IsPressed(key):
					case TriggerType.WentDown when keyboard.WentDown(key):
					case TriggerType.Released when !keyboard.IsPressed(key):
					case TriggerType.WentUp when keyboard.WentUp(key):
					case TriggerType.Repeated when keyboard.IsRepeated(key):
						break;
					default:
						return false;
				}
			}

			if (MouseButton is MouseButton button)
			{
				switch (triggerType)
				{
					case TriggerType.Pressed when mouse.IsPressed(button):
					case TriggerType.WentDown when mouse.WentDown(button):
					case TriggerType.Released when !mouse.IsPressed(button):
					case TriggerType.WentUp when mouse.WentUp(button):
						break;
					default:
						return false;
				}
			}

			if (MouseWheelDirection is MouseWheelDirection direction && !mouse.WasTurned(direction))
				return false;

			return Modifiers == null || Modifiers == keyboard.Modifiers;
		}
	}
}