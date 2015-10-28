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

namespace PointWars.UserInterface.Input
{
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides information about key press and release events.
	/// </summary>
	public sealed class KeyEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly KeyEventArgs CachedInstance = new KeyEventArgs();

		/// <summary>
		///   The state of the key that was pressed or released.
		/// </summary>
		private InputState _state;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		internal KeyEventArgs()
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
		public bool IsPressed => _state.IsPressed;

		/// <summary>
		///   Gets a value indicating whether the key or button was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		public bool WentDown => _state.WentDown;

		/// <summary>
		///   Gets a value indicating whether the key or button was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		public bool WentUp => _state.WentUp;

		/// <summary>
		///   Gets a value indicating whether a key or button repeat event occurred. IsRepeated is also true
		///   when the key or button is pressed, i.e., when WentDown is true.
		/// </summary>
		public bool IsRepeated => _state.IsRepeated;

		/// <summary>
		///   Gets the set of key modifiers that were pressed when the event was raised.
		/// </summary>
		public KeyModifiers Modifiers { get; private set; }

		/// <summary>
		///   Initializes a cached instance.
		/// </summary>
		/// <param name="keyboard">The keyboard device that raised the event.</param>
		/// <param name="key">The key that was pressed or released.</param>
		/// <param name="scanCode">The key's scan code.</param>
		/// <param name="state">The state of the key.</param>
		internal static KeyEventArgs Create(Keyboard keyboard, Key key, ScanCode scanCode, InputState state)
		{
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentInRange(scanCode, nameof(scanCode));

			CachedInstance.Key = key;
			CachedInstance.ScanCode = scanCode;
			CachedInstance._state = state;
			CachedInstance.Modifiers = keyboard.GetModifiers();

			return CachedInstance;
		}
	}
}