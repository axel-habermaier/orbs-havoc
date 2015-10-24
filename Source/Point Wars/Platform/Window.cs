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

namespace PointWars.Platform
{
	using System;
	using System.Numerics;
	using System.Text;
	using Graphics;
	using Input;
	using Logging;
	using Memory;
	using Scripting;
	using Utilities;
	using static SDL2;

	/// <summary>
	///   Represents a window the application can be drawn to.
	/// </summary>
	public unsafe class Window : DisposableObject
	{
		/// <summary>
		///   The minimum overlap of a window that must always be visible.
		/// </summary>
		private const int MinimumOverlap = 100;

		/// <summary>
		///   The minimum supported window size.
		/// </summary>
		public static readonly Size MinimumSize = new Size(320, 240);

		/// <summary>
		///   The maximum supported window size.
		/// </summary>
		public static readonly Size MaximumSize = new Size(4096, 2160);

		private readonly void* _window;

		private bool _shouldClose;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device used to draw the window's contents.</param>
		/// <param name="title">The window's title.</param>
		/// <param name="position">The window's initial position.</param>
		/// <param name="size">The window's initial size.</param>
		/// <param name="mode">The initial mode of the window.</param>
		public Window(GraphicsDevice graphicsDevice, string title, Vector2 position, Size size, WindowMode mode)
		{
			Assert.ArgumentNotNull(graphicsDevice, nameof(graphicsDevice));
			Assert.ArgumentNotNullOrWhitespace(title, nameof(title));
			Assert.ArgumentInRange(mode, nameof(mode));

			ConstrainWindowPlacement(ref position, ref size);

			var flags = SDL_WINDOW_RESIZABLE | SDL_WINDOW_OPENGL;
			switch (mode)
			{
				case WindowMode.Fullscreen:
					flags |= SDL_WINDOW_FULLSCREEN_DESKTOP | SDL_WINDOW_INPUT_GRABBED;
					break;
				case WindowMode.Maximized:
					flags |= SDL_WINDOW_MAXIMIZED;
					break;
			}

			_window = SDL_CreateWindow(title, (int)position.X, (int)position.Y, size.IntegralWidth, size.IntegralHeight, flags);
			if (_window == null)
				Log.Die("Failed to create window: {0}", SDL_GetError());

			SDL_SetWindowMinimumSize(_window, MinimumSize.IntegralWidth, MinimumSize.IntegralHeight);
			SDL_SetWindowMaximumSize(_window, MaximumSize.IntegralWidth, MaximumSize.IntegralHeight);

			GraphicsDevice = graphicsDevice;
			BackBuffer = new RenderTarget(this);
		}

		/// <summary>
		///   Gets the graphics device used to draw the window's contents.
		/// </summary>
		public GraphicsDevice GraphicsDevice { get; }

		/// <summary>
		///   Gets the render target representing the window's back buffer.
		/// </summary>
		public RenderTarget BackBuffer { get; }

		/// <summary>
		///   Gets a value indicating whether the window currently has the focus.
		/// </summary>
		internal bool HasFocus { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the user requested to close the window.
		/// </summary>
		internal bool ShouldClose
		{
			get
			{
				Assert.NotDisposed(this);

				var shouldClose = _shouldClose;
				_shouldClose = false;
				return shouldClose;
			}
		}

		/// <summary>
		///   Gets the position of the window.
		/// </summary>
		public Vector2 Position
		{
			get
			{
				Assert.NotDisposed(this);

				int x, y;
				SDL_GetWindowPosition(_window, out x, out y);

				return new Vector2(x, y);
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

				int width, height;
				SDL_GetWindowSize(_window, out width, out height);

				return ClampSize(width, height);
			}
		}

		/// <summary>
		///   Gets the window's current mode.
		/// </summary>
		public WindowMode Mode
		{
			get
			{
				var flags = SDL_GetWindowFlags(_window);

				if ((flags & SDL_WINDOW_FULLSCREEN_DESKTOP) == SDL_WINDOW_FULLSCREEN_DESKTOP)
					return WindowMode.Fullscreen;

				if ((flags & SDL_WINDOW_MAXIMIZED) == SDL_WINDOW_MAXIMIZED)
					return WindowMode.Maximized;

				if ((flags & SDL_WINDOW_MINIMIZED) == SDL_WINDOW_MINIMIZED)
					return WindowMode.Minimized;

				return WindowMode.Normal;
			}
		}

		/// <summary>
		///   Clamps the window size to sensible values.
		/// </summary>
		private static Size ClampSize(int width, int height)
		{
			// We have to allow the size to become smaller than the window minimum size, as GLFW cannot guarantee
			// us that the window will always be larger; if we don't allow that, we get all sorts of strange
			// rendering artifacts. Do, however, clamp the size to some sensible min and max values
			return new Size(
				MathUtils.Clamp(width, MinimumSize.Width, MaximumSize.Width),
				MathUtils.Clamp(height, MinimumSize.Height, MaximumSize.Height));
		}

		/// <summary>
		///   Presents the contents of the back buffer.
		/// </summary>
		public void Present()
		{
			SDL_GL_SwapWindow(_window);
		}

		/// <summary>
		///   Raised when the window has been resized.
		/// </summary>
		public event Action<Size> Resized;

		/// <summary>
		///   Raised when a key was pressed.
		/// </summary>
		public event Action<Key, ScanCode, KeyModifiers> KeyPressed;

		/// <summary>
		///   Raised when a key was released.
		/// </summary>
		public event Action<Key, ScanCode, KeyModifiers> KeyReleased;

		/// <summary>
		///   Raised when a mouse button was pressed.
		/// </summary>
		public event Action<MouseButton, Vector2, bool> MousePressed;

		/// <summary>
		///   Raised when a mouse button was released.
		/// </summary>
		public event Action<MouseButton, Vector2> MouseReleased;

		/// <summary>
		///   Raised when the mouse wheel was moved.
		/// </summary>
		public event Action<MouseWheelDirection> MouseWheel;

		/// <summary>
		///   Raised when a text was entered.
		/// </summary>
		public event Action<string> TextEntered;

		/// <summary>
		///   Raised when the user requested the window to be closed.
		/// </summary>
		public event Action Closing;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			GraphicsDevice.MakeCurrent();

			BackBuffer.SafeDispose();
			SDL_DestroyWindow(_window);
		}

		/// <summary>
		///   Casts the window to its underlying SDL2 window pointer.
		/// </summary>
		public static implicit operator void*(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));
			return window._window;
		}

		/// <summary>
		///   Toggles between fullscreen and windowed mode when ALT+Enter is pressed.
		/// </summary>
		private void ToggleFullscreen(SDL_Keysym key)
		{
			if ((key.sym != Key.Enter && key.sym != Key.NumpadEnter) || (key.mod & KeyModifiers.LeftAlt) != KeyModifiers.LeftAlt)
				return;

			if (Mode == WindowMode.Fullscreen)
			{
				if (SDL_SetWindowFullscreen(_window, 0) != 0)
					Log.Die("Failed to switch to windowed mode: {0}", SDL_GetError());

				SDL_SetWindowGrab(_window, 0);
			}
			else
			{
				if (SDL_SetWindowFullscreen(_window, SDL_WINDOW_FULLSCREEN_DESKTOP) != 0)
					Log.Die("Failed to switch to fullscreen mode: {0}", SDL_GetError());

				SDL_SetWindowGrab(_window, 1);
			}
		}

		/// <summary>
		///   Handles all pending OS events.
		/// </summary>
		public void HandleEvents()
		{
			var size = Size;
			SDL_Event e;

			while (SDL_PollEvent(out e) != 0)
			{
				switch (e.type)
				{
					case SDL_WINDOWEVENT:
					{
						switch (e.window.windowEvent)
						{
							case SDL_WINDOWEVENT_SHOWN:
							case SDL_WINDOWEVENT_HIDDEN:
							case SDL_WINDOWEVENT_EXPOSED:
							case SDL_WINDOWEVENT_RESIZED:
							case SDL_WINDOWEVENT_ENTER:
							case SDL_WINDOWEVENT_LEAVE:
							case SDL_WINDOWEVENT_MOVED:
							case SDL_WINDOWEVENT_SIZE_CHANGED:
							case SDL_WINDOWEVENT_MINIMIZED:
							case SDL_WINDOWEVENT_MAXIMIZED:
							case SDL_WINDOWEVENT_RESTORED:
								// Don't care
								break;
							case SDL_WINDOWEVENT_FOCUS_GAINED:
								HasFocus = true;
								break;
							case SDL_WINDOWEVENT_FOCUS_LOST:
								HasFocus = false;
								break;
							case SDL_WINDOWEVENT_CLOSE:
								_shouldClose = true;
								Closing?.Invoke();
								break;
							default:
								Log.Debug("Unsupported SDL event.");
								break;
						}

						break;
					}
					case SDL_KEYDOWN:
						KeyPressed?.Invoke(e.key.keysym.sym, e.key.keysym.scancode, e.key.keysym.mod);
						ToggleFullscreen(e.key.keysym);
						break;
					case SDL_KEYUP:
						KeyReleased?.Invoke(e.key.keysym.sym, e.key.keysym.scancode, e.key.keysym.mod);
						break;
					case SDL_TEXTINPUT:
						var count = 0;
						while (e.text.text[count] != 0)
							++count;

						TextEntered?.Invoke(new string((sbyte*)e.text.text, 0, count, Encoding.UTF8));
						break;
					case SDL_MOUSEBUTTONDOWN:
						MousePressed?.Invoke(e.button.button, new Vector2(e.button.x, e.button.y), e.button.clicks == 2);
						break;
					case SDL_MOUSEBUTTONUP:
						MouseReleased?.Invoke(e.button.button, new Vector2(e.button.x, e.button.y));
						break;
					case SDL_MOUSEWHEEL:
						MouseWheel?.Invoke(e.wheel.y < 0 ? MouseWheelDirection.Down : MouseWheelDirection.Up);
						break;
					case SDL_TEXTEDITING:
					case SDL_MOUSEMOTION:
					case SDL_QUIT:
						// Don't care
						break;
					default:
						Log.Debug("Unsupported SDL event.");
						break;
				}
			}

			if (size != Size)
				Resized?.Invoke(Size);

			if (Cvars.WindowPosition != Position && Mode == WindowMode.Normal)
				Cvars.WindowPosition = Position;

			if (Cvars.WindowSize != Size && Mode == WindowMode.Normal)
				Cvars.WindowSize = Size;

			if (Cvars.WindowMode != Mode)
				Cvars.WindowMode = Mode;
		}

		/// <summary>
		///   Gets the area of the desktop.
		/// </summary>
		private static Rectangle GetDesktopArea()
		{
			int left = Int32.MaxValue, top = Int32.MaxValue, bottom = Int32.MinValue, right = Int32.MinValue;
			var num = SDL_GetNumVideoDisplays();

			if (num <= 0)
				Log.Die("Failed to determine the number of displays: {0}", SDL_GetError());

			for (var i = 0; i < num; ++i)
			{
				SDL_Rect bounds;
				if (SDL_GetDisplayBounds(i, out bounds) != 0)
					Log.Die("Failed to retrieve display bounds of display {0}: {1}", i, SDL_GetError());

				left = bounds.x < left ? bounds.x : left;
				right = bounds.x + bounds.w > right ? bounds.x + bounds.w : right;
				top = bounds.y + top >= bounds.y ? bounds.y : top;
				bottom = bounds.y + bounds.h > bottom ? bounds.y + bounds.h : bottom;
			}

			return new Rectangle(left, top, right - left, bottom - top);
		}

		/// <summary>
		///   Constrains the placement and size of the window such that it is always visible.
		/// </summary>
		/// <param name="position">The position of the window.</param>
		/// <param name="size">The size of the window.</param>
		private static void ConstrainWindowPlacement(ref Vector2 position, ref Size size)
		{
			Assert.ArgumentInRange(position.X, -MaximumSize.Width, MaximumSize.Width, nameof(position));
			Assert.ArgumentInRange(position.Y, -MaximumSize.Height, MaximumSize.Height, nameof(position));
			Assert.ArgumentSatisfies(size.Width <= MaximumSize.Width, nameof(size), "Invalid window width.");
			Assert.ArgumentSatisfies(size.Height <= MaximumSize.Height, nameof(size), "Invalid window height.");

			var desktopArea = GetDesktopArea();

			// The window's size must not exceed the size of the desktop
			size.Width = MathUtils.Clamp(size.Width, MinimumSize.Width, desktopArea.Width);
			size.Height = MathUtils.Clamp(size.Height, MinimumSize.Height, desktopArea.Height);

			// Move the window's desired position such that at least MinOverlap pixels of the window are visible 
			// both vertically and horizontally
			position.X = MathUtils.Clamp(position.X, desktopArea.Left - size.Width + MinimumOverlap,
				desktopArea.Left + desktopArea.Width - MinimumOverlap);
			position.Y = MathUtils.Clamp(position.Y, desktopArea.Top - size.Height + MinimumOverlap,
				desktopArea.Top + desktopArea.Height - MinimumOverlap);
		}
	}
}