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
	using System;
	using System.Text;
	using Utilities;

	/// <summary>
	///   Represents a combination of keys and mouse buttons that trigger an input.
	/// </summary>
	public struct InputTrigger : IEquatable<InputTrigger>
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="key">The key that triggers the input.</param>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public InputTrigger(Key key, KeyModifiers modifiers)
			: this()
		{
			Assert.ArgumentInRange(key, nameof(key));

			Key = key;
			Modifiers = modifiers;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="mouseButton">The mouse button that triggers the input.</param>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public InputTrigger(MouseButton mouseButton, KeyModifiers modifiers)
			: this()
		{
			Assert.ArgumentInRange(mouseButton, nameof(mouseButton));

			MouseButton = mouseButton;
			Modifiers = modifiers;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="direction">The direction the mouse wheel must be turned in that triggers the input.</param>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public InputTrigger(MouseWheelDirection direction, KeyModifiers modifiers)
			: this()
		{
			Assert.ArgumentInRange(direction, nameof(direction));

			MouseWheelDirection = direction;
			Modifiers = modifiers;
		}

		/// <summary>
		///   Gets the key that triggers the input, or null if a mouse button or the mouse wheel triggers the input.
		/// </summary>
		public Key? Key { get; }

		/// <summary>
		///   Gets the mouse button that triggers the input, or null if a key or the mouse wheel triggers the input.
		/// </summary>
		public MouseButton? MouseButton { get; }

		/// <summary>
		///   Gets the modifier keys that must be pressed for the input to trigger.
		/// </summary>
		public KeyModifiers Modifiers { get; }

		/// <summary>
		///   Gets the direction the mouse wheel must be turned in that triggers the input, or null if a key or mouse button triggers
		///   the input.
		/// </summary>
		public MouseWheelDirection? MouseWheelDirection { get; }

		/// <summary>
		///   Implicitly creates a configurable input for the given key.
		/// </summary>
		/// <param name="key">The key the configurable input should be created for.</param>
		public static implicit operator InputTrigger(Key key)
		{
			return new InputTrigger(key, KeyModifiers.None);
		}

		/// <summary>
		///   Implicitly creates a configurable input for the given key.
		/// </summary>
		/// <param name="mouseButton">The mouse button the configurable input should be created for.</param>
		public static implicit operator InputTrigger(MouseButton mouseButton)
		{
			return new InputTrigger(mouseButton, KeyModifiers.None);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="direction">The direction the mouse wheel must be turned in that triggers the input.</param>
		public static implicit operator InputTrigger(MouseWheelDirection direction)
		{
			return new InputTrigger(direction, KeyModifiers.None);
		}

		/// <summary>
		///   Checks if the inputs are equal.
		/// </summary>
		public bool Equals(InputTrigger other)
		{
			return Modifiers == other.Modifiers &&
				   Key == other.Key &&
				   MouseButton == other.MouseButton &&
				   MouseWheelDirection == other.MouseWheelDirection;
		}

		/// <summary>
		///   Checks if the inputs are equal.
		/// </summary>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is InputTrigger && Equals((InputTrigger)obj);
		}

		/// <summary>
		///   Gets a hash code.
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Key.GetHashCode();
				hashCode = (hashCode * 397) ^ MouseButton.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)Modifiers;
				return hashCode;
			}
		}

		/// <summary>
		///   Checks if the inputs are equal.
		/// </summary>
		public static bool operator ==(InputTrigger left, InputTrigger right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Checks if the inputs are not equal.
		/// </summary>
		public static bool operator !=(InputTrigger left, InputTrigger right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///   Returns a string representation of the configurable input.
		/// </summary>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("[");

			if (MouseWheelDirection != null)
				builder.Append("MouseWheel." + MouseWheelDirection.Value);

			if (Key != null)
				builder.Append("Key." + Key.Value);

			if (MouseButton != null)
				builder.Append("Mouse." + MouseButton.Value);

			if ((Modifiers & KeyModifiers.Alt) != 0)
				builder.Append("+Alt");

			if ((Modifiers & KeyModifiers.Shift) != 0)
				builder.Append("+Shift");

			if ((Modifiers & KeyModifiers.Control) != 0)
				builder.Append("+Control");

			builder.Append("]");
			return builder.ToString();
		}
	}
}