// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using System.Numerics;
	using Memory;
	using Utilities;
	using static SDL2;

	/// <summary>
	///   Represents the state of the mouse.
	/// </summary>
	public class Mouse : DisposableObject
	{
		/// <summary>
		///   Stores whether a button is currently being double-clicked.
		/// </summary>
		private readonly bool[] _doubleClicked = new bool[Enum.GetValues(typeof(MouseButton)).Length + 1];

		/// <summary>
		///   The mouse button states.
		/// </summary>
		private readonly InputState[] _states = new InputState[Enum.GetValues(typeof(MouseButton)).Length + 1];

		/// <summary>
		///   The window that generates the mouse events.
		/// </summary>
		private readonly Window _window;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that generates the mouse events.</param>
		internal Mouse(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.MousePressed += ButtonPressed;
			_window.MouseReleased += ButtonReleased;
		}

		/// <summary>
		///   Gets the position of the mouse.
		/// </summary>
		public Vector2 Position
		{
			get
			{
				int x, y;
				SDL_GetMouseState(out x, out y);

				return new Vector2(x, y);
			}
		}

		/// <summary>
		///   Gets a value indicating whether the mouse is currently within the window.
		/// </summary>
		public unsafe bool InsideWindow => SDL_GetMouseFocus() != null;

		/// <summary>
		///   Gets the input state for the given button.
		/// </summary>
		/// <param name="button">The button the input state should be returned for.</param>
		public InputState this[MouseButton button]
		{
			get
			{
				Assert.ArgumentInRange(button, nameof(button));
				return _states[(int)button];
			}
		}

		/// <summary>
		///   Raised when the mouse wheel is scrolled.
		/// </summary>
		public event Action<MouseWheelDirection> Wheel
		{
			add { _window.MouseWheel += value; }
			remove { _window.MouseWheel -= value; }
		}

		/// <summary>
		///   Raised when a mouse button was pressed.
		/// </summary>
		public event Action<MouseButton, Vector2, bool> Pressed
		{
			add { _window.MousePressed += value; }
			remove { _window.MousePressed -= value; }
		}

		/// <summary>
		///   Raised when a mouse button was released.
		/// </summary>
		public event Action<MouseButton, Vector2> Released
		{
			add { _window.MouseReleased += value; }
			remove { _window.MouseReleased -= value; }
		}

		/// <summary>
		///   Raised when the mouse has been moved.
		/// </summary>
		public event Action<Vector2> Moved
		{
			add { _window.MouseMoved += value; }
			remove { _window.MouseMoved -= value; }
		}

		/// <summary>
		///   Invoked when a button has been pressed.
		/// </summary>
		private void ButtonPressed(MouseButton button, Vector2 position, bool doubleClicked)
		{
			_states[(int)button].Pressed();
			_doubleClicked[(int)button] |= doubleClicked;
		}

		/// <summary>
		///   Invoked when a button has been released.
		/// </summary>
		private void ButtonReleased(MouseButton button, Vector2 position)
		{
			_states[(int)button].Released();
		}

		/// <summary>
		///   Updates the mouse state.
		/// </summary>
		internal void Update()
		{
			for (var i = 0; i < _states.Length; ++i)
			{
				_states[i].Update();
				_doubleClicked[i] = false;
			}
		}

		/// <summary>
		///   Gets a value indicating whether the button is currently being pressed down.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool IsPressed(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].IsPressed;
		}

		/// <summary>
		///   Gets a value indicating whether the button was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentDown(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].WentDown;
		}

		/// <summary>
		///   Gets a value indicating whether the button was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentUp(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].WentUp;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_window.MousePressed -= ButtonPressed;
			_window.MouseReleased -= ButtonReleased;
		}
	}
}