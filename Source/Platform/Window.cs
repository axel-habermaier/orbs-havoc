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
	using GLFW3;
	using Graphics;
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
		private readonly GLFWwindow* _window;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="title">The window's title.</param>
		/// <param name="position">The window's initial position on the screen.</param>
		/// <param name="size">The window's initial size.</param>
		/// <param name="windowMode">The window's initial mode.</param>
		public Window(string title, Vector2 position, Size size, WindowMode windowMode)
		{
			Assert.ArgumentInRange(windowMode, nameof(windowMode));

			glfwWindowHint(GLFW_CONTEXT_VERSION_MAJOR, 3);
			glfwWindowHint(GLFW_CONTEXT_VERSION_MINOR, 3);
			glfwWindowHint(GLFW_OPENGL_PROFILE, GLFW_OPENGL_CORE_PROFILE);
			glfwWindowHint(GLFW_OPENGL_FORWARD_COMPAT, 1);
			glfwWindowHint(GLFW_OPENGL_DEBUG_CONTEXT, PlatformInfo.IsDebug ? 1 : 0);

			var monitor = windowMode == WindowMode.Fullscreen ? glfwGetPrimaryMonitor() : null;
			_window = glfwCreateWindow(size.IntegralWidth, size.IntegralHeight, title, monitor, null);

			if (_window == null)
				Log.Die("Window creation failed.");

			glfwSetWindowPos(_window, position.IntegralX, position.IntegralY);
			glfwMakeContextCurrent(_window);

			OpenGL.Initialize();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			glfwDestroyWindow(_window);
		}
	}
}