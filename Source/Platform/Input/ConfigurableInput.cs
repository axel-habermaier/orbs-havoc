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
	using System.Text;
	using Utilities;

	/// <summary>
	///   Represents a configurable input.
	/// </summary>
	public struct ConfigurableInput
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="key">The key that triggers the input.</param>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public ConfigurableInput(Key key, KeyModifiers modifiers)
			: this()
		{
			Key = key;
			Modifiers = modifiers;
		}

		/// <summary>
		///   Initializes a new instance of the <see cref="T:System.Object" /> class.
		/// </summary>
		/// <param name="mouseButton">The mouse button that triggers the input.</param>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public ConfigurableInput(MouseButton mouseButton, KeyModifiers modifiers)
			: this()
		{
			MouseButton = mouseButton;
			Modifiers = modifiers;
		}

		/// <summary>
		///   Gets the key that triggers the input, or null if a mouse button triggers the input.
		/// </summary>
		public Key? Key { get; private set; }

		/// <summary>
		///   Gets the mouse button that triggers the input, or null if a key triggers the input.
		/// </summary>
		public MouseButton? MouseButton { get; private set; }

		/// <summary>
		///   Gets the modifier keys that must be pressed for the input to trigger.
		/// </summary>
		public KeyModifiers Modifiers { get; private set; }

		/// <summary>
		///   Creates a copy of the current instance with the given modifiers.
		/// </summary>
		/// <param name="modifiers">The modifier keys that must be pressed for the input to trigger.</param>
		public ConfigurableInput WithModifiers(KeyModifiers modifiers)
		{
			return new ConfigurableInput { Key = Key, MouseButton = MouseButton, Modifiers = modifiers };
		}

		/// <summary>
		///   Implicitly creates a configurable input for the given key.
		/// </summary>
		/// <param name="key">The key the configurable input should be created for.</param>
		public static implicit operator ConfigurableInput(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new ConfigurableInput(key, KeyModifiers.None);
		}

		/// <summary>
		///   Implicitly creates a configurable input for the given key.
		/// </summary>
		/// <param name="mouseButton">The mouse button the configurable input should be created for.</param>
		public static implicit operator ConfigurableInput(MouseButton mouseButton)
		{
			Assert.ArgumentInRange(mouseButton, nameof(mouseButton));
			return new ConfigurableInput(mouseButton, KeyModifiers.None);
		}

		/// <summary>
		///   Returns a string representation of the configurable input.
		/// </summary>
		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Clear();
			builder.Append("[");

			if (Key != null)
				builder.Append("Key." + Key.Value);

			if (MouseButton != null)
				builder.Append("Mouse." + MouseButton.Value);

			if ((Modifiers & KeyModifiers.Alt) == KeyModifiers.Alt)
				builder.Append("+Alt");

			if ((Modifiers & KeyModifiers.Shift) == KeyModifiers.Shift)
				builder.Append("+Shift");

			if ((Modifiers & KeyModifiers.Control) == KeyModifiers.Control)
				builder.Append("+Control");

			builder.Append("]");
			return builder.ToString();
		}
	}
}