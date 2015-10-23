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
	using Memory;
	using Utilities;
	using static GLFW3.GLFW;

	/// <summary>
	///   Represents the state of the keyboard.
	/// </summary>
	public class Keyboard : DisposableObject
	{
		/// <summary>
		///   The key states.
		/// </summary>
		private readonly InputState[] _states = new InputState[GLFW_KEY_LAST + 1];

		/// <summary>
		///   The window that generates the key events.
		/// </summary>
		private readonly Window _window;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that generates the key events.</param>
		internal Keyboard(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.KeyPressed += OnKeyPressed;
			_window.KeyReleased += OnKeyReleased;
			_window.CharacterEntered += OnCharacterEntered;
		}

		/// <summary>
		///   Raised when a text character was entered.
		/// </summary>
		public event Action<char> CharacterEntered;

		/// <summary>
		///   Raised when a key was pressed.
		/// </summary>
		public event Action<Key, int, KeyModifiers> KeyPressed
		{
			add { _window.KeyPressed += value; }
			remove { _window.KeyPressed -= value; }
		}

		/// <summary>
		///   Raised when a key was released.
		/// </summary>
		public event Action<Key, int, KeyModifiers> KeyReleased
		{
			add { _window.KeyReleased += value; }
			remove { _window.KeyReleased -= value; }
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_window.KeyPressed -= OnKeyPressed;
			_window.KeyReleased -= OnKeyReleased;
			_window.CharacterEntered -= OnCharacterEntered;
		}

		/// <summary>
		///   Invoked when a character has been entered.
		/// </summary>
		/// <param name="character">Identifies the character that has been entered.</param>
		private void OnCharacterEntered(uint character)
		{
			// Only raise the character entered event if the character is a printable ASCII character
			if (character < 32 || character > 255 || Char.IsControl((char)character))
				return;

			CharacterEntered?.Invoke((char)character);
		}

		/// <summary>
		///   Invoked when a key has been released.
		/// </summary>
		private void OnKeyReleased(Key key, int scanCode, KeyModifiers modifiers)
		{
			_states[(int)key].Released();
		}

		/// <summary>
		///   Invoked when a key has been pressed.
		/// </summary>
		private void OnKeyPressed(Key key, int scanCode, KeyModifiers modifiers)
		{
			_states[(int)key].Pressed();
		}

		/// <summary>
		///   Updates the keyboard state.
		/// </summary>
		internal void Update()
		{
			for (var i = 0; i < _states.Length; ++i)
				_states[i].Update();
		}

		/// <summary>
		///   Gets a value indicating whether the key is currently being pressed down.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsPressed(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _states[(int)key].IsPressed;
		}

		/// <summary>
		///   Gets a value indicating whether the key was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentDown(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _states[(int)key].WentDown;
		}

		/// <summary>
		///   Gets a value indicating whether the key was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentUp(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _states[(int)key].WentUp;
		}

		/// <summary>
		///   Gets a value indicating whether a system key repeat event occurred. IsRepeated is also true
		///   when the key is pressed, i.e., when WentDown is true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsRepeated(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _states[(int)key].IsRepeated;
		}
	}
}