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

namespace OrbsHavoc
{
	using System.Threading;
	using Assets;
	using Platform;
	using Platform.Graphics;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Scripting;
	using Utilities;
	using Views;

	/// <summary>
	///   Represents the application.
	/// </summary>
	internal class Application
	{
		/// <summary>
		///   The name of the application.
		/// </summary>
		public const string Name = "Orbs Havoc";

		private Renderer _renderer;
		private bool _running;
		private ViewCollection _views;

		/// <summary>
		///   Gets the application's window.
		/// </summary>
		public Window Window { get; private set; }

		/// <summary>
		///   Gets the application's input device.
		/// </summary>
		public LogicalInputDevice InputDevice { get; private set; }

		/// <summary>
		///   Gets thhe graphics device used by the application.
		/// </summary>
		public GraphicsDevice GraphicsDevice { get; private set; }

		/// <summary>
		///   Initializes the application.
		/// </summary>
		private void Initialize()
		{
			_views.Initialize();

			Commands.OnExit += Exit;
			Commands.Bind(Key.F1, "start_server");
			Commands.Bind(Key.F2, "stop_server");
			Commands.Bind(Key.F3, "connect ::1");
			Commands.Bind(Key.F4, "disconnect");
			Commands.Bind(Key.F5, "reload_assets");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control), "add_bot");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control | KeyModifiers.Shift), "remove_bot");
			Commands.Bind(new InputTrigger(Key.Escape, KeyModifiers.LeftShift), "exit");
			Commands.Bind(Key.F10, "toggle show_debug_overlay");
		}

		/// <summary>
		///   Runs the application.
		/// </summary>
		public unsafe void Run()
		{
			_running = true;

			using (GraphicsDevice = new GraphicsDevice())
			using (Window = new Window(GraphicsDevice, Name, Cvars.WindowPosition, Cvars.WindowSize, Cvars.WindowMode))
			using (InputDevice = new LogicalInputDevice(Window))
			using (var bindings = new BindingCollection(InputDevice))
			using (new AssetBundle())
			using (_renderer = new Renderer(Window))
			using (_views = new ViewCollection(this))
			{
				Initialize();
				Commands.Help();

				while (_running)
				{
					double updateTime, drawTime;

					// Perform the necessary updates for the frame
					using (TimeMeasurement.Measure(&updateTime))
					{
						HandleInput();
						bindings.Update();

						_views.Update();
					}

					// Ensure that CPU and GPU are synchronized after this point, i.e., the GPU lags behind by
					// at most GraphicsDevice.FrameLag frames
					GraphicsDevice.SyncWithCpu();

					// Draw the current frame
					using (TimeMeasurement.Measure(&drawTime))
					{
						GraphicsDevice.BeginFrame();

						_renderer.ClearRenderTarget(Window.BackBuffer, Colors.Black);
						_views.Draw(_renderer);
						_renderer.Render();

						GraphicsDevice.EndFrame();
					}

					// Present the contents of the window's backbuffer
					Window.Present();

					// Save CPU when the window is not focused
					if (!Window.HasFocus)
						Thread.Sleep(10);

					// Update the debug overlay
					_views.DebugOverlay.GpuFrameTime = GraphicsDevice.FrameTime;
					_views.DebugOverlay.CpuUpdateTime = updateTime;
					_views.DebugOverlay.CpuRenderTime = drawTime;
				}
			}
		}

		/// <summary>
		///   Handles all user input.
		/// </summary>
		private void HandleInput()
		{
			// Update the keyboard and mouse state first (this ensures that WentDown returns 
			// false for all keys and buttons, etc.)
			InputDevice.Keyboard.Update();
			InputDevice.Mouse.Update();

			// Process all pending operating system events
			Window.HandleEvents();

			// Update the logical inputs based on the new state of the input system
			InputDevice.Update();
		}

		/// <summary>
		///   Exists the application.
		/// </summary>
		private void Exit()
		{
			_running = false;
			Log.Info("Exiting {0}...", Name);
		}
	}
}