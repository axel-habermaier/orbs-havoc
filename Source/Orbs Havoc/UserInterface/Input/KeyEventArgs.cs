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

namespace OrbsHavoc.UserInterface.Input
{
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides information about key press and release events.
	/// </summary>
	public sealed class KeyEventArgs : InputEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly KeyEventArgs _cachedInstance = new KeyEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private KeyEventArgs()
		{
		}

		/// <summary>
		///   Gets the key that was pressed or released. The key depends on the keyboard layout.
		/// </summary>
		public Key Key { get; private set; }

		/// <summary>
		///   Gets the key's scan code. The scan code is independent of the keyboard layout but may differ between
		///   operating systems.
		/// </summary>
		public ScanCode ScanCode { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button is currently being pressed down.
		/// </summary>
		public InputState KeyState => Keyboard[ScanCode];

		/// <summary>
		///   Gets the keyboard that generated the event.
		/// </summary>
		public Keyboard Keyboard { get; private set; }

		/// <summary>
		///   Gets the set of key modifiers that were pressed when the event was raised.
		/// </summary>
		public KeyModifiers Modifiers { get; private set; }

		/// <summary>
		///   Indicates whether the event was raised because of a key being released or pressed.
		/// </summary>
		public InputEventKind Kind { get; private set; }

		/// <summary>
		///   Initializes a cached instance.
		/// </summary>
		/// <param name="keyboard">The keyboard device that raised the event.</param>
		/// <param name="key">The key that was pressed or released.</param>
		/// <param name="scanCode">The key's scan code.</param>
		/// <param name="kind">Indicates whether the event was raised because of a key being released or pressed.</param>
		internal static KeyEventArgs Create(Keyboard keyboard, Key key, ScanCode scanCode, InputEventKind kind)
		{
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			Assert.ArgumentInRange(kind, nameof(kind));

			_cachedInstance.Handled = false;
			_cachedInstance.Keyboard = keyboard;
			_cachedInstance.Key = key;
			_cachedInstance.ScanCode = scanCode;
			_cachedInstance.Modifiers = keyboard.GetModifiers();
			_cachedInstance.Kind = kind;

			return _cachedInstance;
		}
	}
}