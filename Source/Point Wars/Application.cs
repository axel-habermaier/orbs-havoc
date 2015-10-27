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

namespace PointWars
{
	using System.Numerics;
	using System.Threading;
	using Platform;
	using Platform.Graphics;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Scripting;
	using UserInterface;
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
		public const string Name = "Point Wars";

		private bool _running;
		private SpriteBatch _spriteBatch;
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

			Window.Closing += Exit;
			Window.Resized += Resize;
			Commands.OnExit += Exit;

			Commands.Bind(Key.F1, "start_server TestServer");
			Commands.Bind(Key.F2, "stop_server");
			Commands.Bind(Key.F3, "connect ::1");
			Commands.Bind(Key.F4, "disconnect");
			Commands.Bind(Key.F5, "reload_assets");

			Commands.Bind(new ConfigurableInput(Key.Escape, KeyModifiers.LeftShift), "exit");
			Commands.Bind(Key.F10, "toggle show_debug_overlay");

			InputDevice.ActivateLayer(InputLayer.Game);
			Resize(Window.Size);
		}

		/// <summary>
		///   Runs the application.
		/// </summary>
		/// <param name="console">The console that should be used by the application.</param>
		public unsafe void Run(Console console)
		{
			_running = true;

			using (GraphicsDevice = new GraphicsDevice())
			using (Window = new Window(GraphicsDevice, Name, Cvars.WindowPosition, Cvars.WindowSize, Cvars.WindowMode))
			using (InputDevice = new LogicalInputDevice(Window))
			using (var bindings = new BindingCollection(InputDevice))
			using (new Assets())
			using (_spriteBatch = new SpriteBatch())
			using (_views = new ViewCollection(this, console))
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
						Window.BackBuffer.Clear(Colors.Black);

						_views.Draw(_spriteBatch);
						_spriteBatch.DrawBatch(Window.BackBuffer);

						GraphicsDevice.EndFrame();
					}

					// Present the contents of the window's backbuffer
					Window.Present();

					// Save CPU when there are no focused windows
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
		public void Exit()
		{
			_running = false;
			Log.Info("Exiting {0}...", Name);
		}

		/// <summary>
		///   Resizes the application's views.
		/// </summary>
		private void Resize(Size size)
		{
			_spriteBatch.ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
			_views.Resize(size);
		}
	}
}