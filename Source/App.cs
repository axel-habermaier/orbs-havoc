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

namespace PointWars
{
	using Math;
	using Platform;
	using Platform.Graphics;
	using Platform.Input;
	using Rendering;
	using Utilities;
	using static GLFW3.GLFW;

	/// <summary>
	///   Represents the application.
	/// </summary>
	internal class App
	{
		/// <summary>
		///   The name of the application.
		/// </summary>
		public const string Name = "Point Wars";

		private readonly bool _isRunning = true;

		/// <summary>
		///   Initializes the application.
		/// </summary>
		public App()
		{
			Assert.That(Current == null, "The application has already been initialized.");
			Current = this;
		}

		/// <summary>
		///   Gets the current application instance.
		/// </summary>
		public static App Current { get; private set; }

		/// <summary>
		///   Gets the app's window.
		/// </summary>
		public Window Window { get; private set; }

		/// <summary>
		///   Gets the app's input device.
		/// </summary>
		public LogicalInputDevice Input { get; private set; }

		/// <summary>
		///   Initializes the application.
		/// </summary>
		private void Initialize()
		{
		}

		/// <summary>
		///   Updates the application state.
		/// </summary>
		private void Update()
		{
		}

		/// <summary>
		///   Draws the current frame.
		/// </summary>
		private void Draw()
		{
			Window.BackBuffer.Clear(Colors.Red);
		}

		/// <summary>
		///   Runs the application.
		/// </summary>
		public void Run()
		{
			using (Window = new Window(Name, new Size(1024, 768), false))
			using (Input = new LogicalInputDevice(Window))
			using (var frameSynchronizer = new FrameSynchronizer())
			{
				Initialize();

				while (_isRunning)
				{
					glfwPollEvents();
					Input.Update();

					Update();

					frameSynchronizer.BeginFrame();
					Draw();
					frameSynchronizer.EndFrame();

					Window.Present();
				}
			}
		}
	}
}