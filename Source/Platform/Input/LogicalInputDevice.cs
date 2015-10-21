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
	using System.Collections.Generic;
	using Memory;
	using Utilities;
	using static GLFW3.GLFW;

	/// <summary>
	///   Manages logical inputs that are triggered by input triggers. The logical input device listens for the corresponding
	///   input events on its associated window.
	/// </summary>
	public sealed class LogicalInputDevice : DisposableObject
	{
		private readonly List<LogicalInput> _inputs = new List<LogicalInput>(64);
		private readonly InputState[] _keyStates = new InputState[GLFW_KEY_LAST + 1];
		private readonly InputState[] _mouseButtonStates = new InputState[Enum.GetValues(typeof(MouseButton)).Length + 1];
		private readonly Window _window;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public LogicalInputDevice(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.KeyPressed += OnKeyDown;
			_window.KeyReleased += OnKeyUp;
			_window.MouseWheel += OnMouseWheel;
			_window.MousePressed += OnMouseDown;
			_window.MouseReleased += OnMouseUp;
		}

		/// <summary>
		///   Gets a value indicating whether the user is currently inputting some text.
		/// </summary>
		public bool TextInputEnabled { get; private set; }

		/// <summary>
		///   Raises the mouse wheel event.
		/// </summary>
		private void OnMouseWheel(MouseWheelDirection direction)
		{
			MouseWheel?.Invoke(direction);
		}

		/// <summary>
		///   Updates the mouse button state of the mouse button the event has been raised for.
		/// </summary>
		private void OnMouseDown(MouseButton button, KeyModifiers modifiers)
		{
			Assert.ArgumentInRange(button, nameof(button));

			_mouseButtonStates[(int)button].Pressed();
			MousePressed?.Invoke(button, modifiers);
		}

		/// <summary>
		///   Updates the mouse button state of the mouse button the event has been raised for.
		/// </summary>
		private void OnMouseUp(MouseButton button, KeyModifiers modifiers)
		{
			Assert.ArgumentInRange(button, nameof(button));

			_mouseButtonStates[(int)button].Released();
			MouseReleased?.Invoke(button, modifiers);
		}

		/// <summary>
		///   Updates the key state of the key the event has been raised for.
		/// </summary>
		private void OnKeyDown(Key key, int scanCode, KeyModifiers modifiers)
		{
			Assert.ArgumentInRange(key, nameof(key));

			_keyStates[(int)key].Pressed();
			KeyPressed?.Invoke(key, scanCode, modifiers);
		}

		/// <summary>
		///   Updates the key state of the key the event has been raised for.
		/// </summary>
		private void OnKeyUp(Key key, int scanCode, KeyModifiers modifiers)
		{
			Assert.ArgumentInRange(key, nameof(key));

			_keyStates[(int)key].Released();
			KeyReleased?.Invoke(key, scanCode, modifiers);
		}

		/// <summary>
		///   Registers a logical input on the logical input device. Subsequently, the logical input's IsTriggered
		///   property can be used to determine whether the logical input is currently triggered.
		/// </summary>
		/// <param name="input">The logical input that should be registered on the device.</param>
		public void Add(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(!input.IsRegistered, nameof(input), "The input is already registered on a device.");

			_inputs.Add(input);
			input.SetLogicalDevice(this);
		}

		/// <summary>
		///   Removes the logical input from the logical input device.
		/// </summary>
		/// <param name="input">The logical input that should be removed.</param>
		public void Remove(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(input.IsRegistered, nameof(input), "The input trigger is not registered.");

			if (_inputs.Remove(input))
				input.SetLogicalDevice(null);
		}

		/// <summary>
		///   Updates the device state.
		/// </summary>
		public unsafe void Update()
		{
			// Update all logical inputs
			foreach (var input in _inputs)
				input.Update(this);

			// Update all key states
			for (var i = 0; i < _keyStates.Length; ++i)
			{
				// We might have missed a key up event for a pressed key, so update the input state accordingly
				if (_keyStates[i].IsPressed && glfwGetKey(_window, i) == GLFW_RELEASE)
					_keyStates[i].Released();

				_keyStates[i].Update();
			}

			// Update all mouse button states
			for (var i = 0; i < _mouseButtonStates.Length; ++i)
			{
				// We might have missed a mouse up event for a pressed mouse button, so update the input state accordingly
				if (_mouseButtonStates[i].IsPressed && glfwGetMouseButton(_window, i) == GLFW_RELEASE)
					_mouseButtonStates[i].Released();

				_mouseButtonStates[i].Update();
			}
		}

		/// <summary>
		///   Gets a value indicating whether the key is currently being pressed down.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsPressed(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _keyStates[(int)key].IsPressed;
		}

		/// <summary>
		///   Gets a value indicating whether the key was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentDown(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _keyStates[(int)key].WentDown;
		}

		/// <summary>
		///   Gets a value indicating whether the key was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentUp(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _keyStates[(int)key].WentUp;
		}

		/// <summary>
		///   Gets a value indicating whether a system key repeat event occurred. IsRepeated is also true
		///   when the key is pressed, i.e., when WentDown is true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsRepeated(Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return _keyStates[(int)key].IsRepeated;
		}

		/// <summary>
		///   Gets a value indicating whether the button is currently being pressed down.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool IsPressed(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _mouseButtonStates[(int)button].IsPressed;
		}

		/// <summary>
		///   Gets a value indicating whether the button was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentDown(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _mouseButtonStates[(int)button].WentDown;
		}

		/// <summary>
		///   Gets a value indicating whether the button was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentUp(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _mouseButtonStates[(int)button].WentUp;
		}

		/// <summary>
		///   Raised when a key was pressed.
		/// </summary>
		public event Action<Key, int, KeyModifiers> KeyPressed;

		/// <summary>
		///   Raised when a key was released.
		/// </summary>
		public event Action<Key, int, KeyModifiers> KeyReleased;

		/// <summary>
		///   Raised when a mouse button was pressed.
		/// </summary>
		public event Action<MouseButton, KeyModifiers> MousePressed;

		/// <summary>
		///   Raised when a mouse button was released.
		/// </summary>
		public event Action<MouseButton, KeyModifiers> MouseReleased;

		/// <summary>
		///   Raised when the mouse wheel was moved.
		/// </summary>
		public event Action<MouseWheelDirection> MouseWheel;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_window.KeyPressed -= OnKeyDown;
			_window.KeyReleased -= OnKeyUp;
			_window.MouseWheel -= OnMouseWheel;
			_window.MousePressed -= OnMouseDown;
			_window.MouseReleased -= OnMouseUp;
		}
	}
}