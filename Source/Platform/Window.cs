// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace PointWars.Platform
{
	using System;
	using GLFW3;
	using Graphics;
	using Input;
	using Logging;
	using Math;
	using Memory;
	using Utilities;
	using static GLFW3.GLFW;

	/// <summary>
	///   Represents a window the application can be drawn to.
	/// </summary>
	public unsafe class Window : DisposableObject
	{
		/// <summary>
		///   The minimum supported window size.
		/// </summary>
		public static readonly Size MinimumSize = new Size(320, 240);

		/// <summary>
		///   The maximum supported window size.
		/// </summary>
		public static readonly Size MaximumSize = new Size(4096, 2160);

		private static bool _initialized;
		private readonly bool _fullscreen;
		private readonly GLFWwindow* _window;

		private GLFWcharfun _characterCallback;
		private GLFWkeyfun _keyCallback;
		private GLFWmousebuttonfun _mouseCallback;
		private GLFWscrollfun _scrollCallback;
		private GLFWframebuffersizefun _sizeCallback;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="title">The window's title.</param>
		/// <param name="size">The window's initial size.</param>
		/// <param name="fullscreen">Indicates whether the window should be opened in fullscreen mode.</param>
		public Window(string title, Size size, bool fullscreen)
		{
			Assert.That(!_initialized, "Only one window can be opened.");
			_initialized = true;

			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
			glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
			glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
			glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, 1);
			glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, PlatformInfo.IsDebug ? 1 : 0);

			var monitor = fullscreen ? glfwGetPrimaryMonitor() : null;
			_fullscreen = fullscreen;
			_window = glfwCreateWindow(size.IntegralWidth, size.IntegralHeight, title, monitor, null);

			if (_window == null)
				Log.Die("Window creation failed.");

			glfwMakeContextCurrent(_window);
			InitializeCallbacks();
			BackBuffer = new RenderTarget();
		}

		/// <summary>
		///   Gets the render target representing the window's back buffer.
		/// </summary>
		public RenderTarget BackBuffer { get; }

		/// <summary>
		///   Gets a value indicating whether the window currently has the focus.
		/// </summary>
		internal bool HasFocus
		{
			get
			{
				Assert.NotDisposed(this);
				return glfwGetWindowAttrib(_window, GLFW_FOCUSED) != 0;
			}
		}

		/// <summary>
		///   Gets a value indicating whether the user requested to close the window.
		/// </summary>
		internal bool ShouldClose
		{
			get
			{
				Assert.NotDisposed(this);
				return glfwWindowShouldClose(_window) != 0;
			}
		}

		/// <summary>
		///   Gets the size of the window's rendering area.
		/// </summary>
		public Size Size
		{
			get
			{
				Assert.NotDisposed(this);

				int left, top, right, bottom;
				glfwGetWindowFrameSize(_window, &left, &top, &right, &bottom);

				// We have to allow the size to become smaller than the window minimum size, as GLFW cannot guarantee
				// us that the window will always be larger; if we don't allow that, we get all sorts of strange
				// rendering artifacts. Do, however, clamp the size to some sensible min and max values
				var width = MathUtils.Clamp(right - left, MinimumSize.Width, MaximumSize.Width);
				var height = MathUtils.Clamp(top - bottom, MinimumSize.Height, MaximumSize.Height);

				return new Size(width, height);
			}
		}

		/// <summary>
		///   Gets a value indicating whether the window is in fullscreen mode.
		/// </summary>
		public bool IsFullscreen
		{
			get
			{
				Assert.NotDisposed(this);
				return _fullscreen;
			}
		}

		/// <summary>
		///   Presents the contents of the back buffer.
		/// </summary>
		public void Present()
		{
			glfwSwapBuffers(_window);
		}

		/// <summary>
		///   Initializes the window's GLFW callbacks.
		/// </summary>
		private void InitializeCallbacks()
		{
			_characterCallback = (window, codepoint) => CharacterEntered?.Invoke(codepoint);
			_scrollCallback = (window, x, y) => MouseWheel?.Invoke(y > 0 ? MouseWheelDirection.Up : MouseWheelDirection.Down);
			_sizeCallback = (window, width, height) => Resized?.Invoke(new Size(width, height));
			_keyCallback = (window, key, scancode, action, mods) =>
			{
				Assert.ArgumentInRange((Key)key, nameof(key));

				if (action == GLFW_PRESS || action == GLFW_REPEAT)
					KeyPressed?.Invoke((Key)key, scancode, (KeyModifiers)mods);
				else
					KeyReleased?.Invoke((Key)key, scancode, (KeyModifiers)mods);
			};
			_mouseCallback = (window, button, action, mods) =>
			{
				Assert.ArgumentInRange((MouseButton)button, nameof(button));

				if (action == GLFW_PRESS || action == GLFW_REPEAT)
					MousePressed?.Invoke((MouseButton)button, (KeyModifiers)mods);
				else
					MouseReleased?.Invoke((MouseButton)button, (KeyModifiers)mods);
			};

			glfwSetCharCallback(_window, _characterCallback);
			glfwSetScrollCallback(_window, _scrollCallback);
			glfwSetFramebufferSizeCallback(_window, _sizeCallback);
			glfwSetKeyCallback(_window, _keyCallback);
			glfwSetMouseButtonCallback(_window, _mouseCallback);
		}

		/// <summary>
		///   Raised when the window has been resized.
		/// </summary>
		public event Action<Size> Resized;

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
		///   Raised when a character was entered.
		/// </summary>
		public event Action<uint> CharacterEntered;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			BackBuffer.SafeDispose();

			glfwDestroyWindow(_window);
			_initialized = false;
		}

		/// <summary>
		///   Casts the window to its underlying GLFW window handle.
		/// </summary>
		public static implicit operator GLFWwindow*(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));
			return window._window;
		}
	}
}