﻿// The MIT License (MIT)
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
	using System.Threading;
	using Math;
	using Platform;
	using Platform.Graphics;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Scripting;
	using UserInterface;
	using Utilities;

	/// <summary>
	///   Represents the application.
	/// </summary>
	internal class Application
	{
		/// <summary>
		///   The name of the application.
		/// </summary>
		public const string Name = "Point Wars";

		/// <summary>
		///   Indicates whether the application is currently running.
		/// </summary>
		private bool _running;

		/// <summary>
		///   Initializes the application.
		/// </summary>
		public Application(Console console)
		{
			Assert.ArgumentNotNull(console, nameof(console));
			Assert.That(Current == null, "The application has already been initialized.");

			Current = this;
			Console = console;
		}

		/// <summary>
		///   The console used by the application.
		/// </summary>
		public Console Console { get; }

		/// <summary>
		///   Gets the current application instance.
		/// </summary>
		public static Application Current { get; private set; }

		/// <summary>
		///   Gets the application's window.
		/// </summary>
		public Window Window { get; private set; }

		/// <summary>
		///   Gets the application's sprite batch.
		/// </summary>
		public SpriteBatch SpriteBatch { get; private set; }

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
			Window.Closing += Exit;
			Window.Resized += OnWindowResized;
			Commands.OnExit += Exit;

			Commands.Bind(Key.F1.WentDown(), "start_server TestServer");
			Commands.Bind(Key.F2.WentDown(), "stop_server");
			Commands.Bind(Key.F3.WentDown(), "connect ::1");
			Commands.Bind(Key.F4.WentDown(), "disconnect");
			Commands.Bind(Key.F5.WentDown(), "reload_assets");

			Commands.Bind(Key.Escape.WentDown() & Key.LeftShift.IsPressed(), "exit");
			Commands.Bind(Key.F10.WentDown(), "toggle show_debug_overlay");

			OnWindowResized(Window.Size);
			InputDevice.ActivateLayer(InputLayer.Game);
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
			Window.BackBuffer.Clear(Colors.Black);
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
			using (new Assets())
			using (SpriteBatch = new SpriteBatch())
			using (var bindings = new BindingCollection(InputDevice))
			using (var debugOverlay = new DebugOverlay())
			{
				Console.Initialize(Window.Size, InputDevice, Assets.DefaultFont);
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
						debugOverlay.Update(Window.Size);
						Console.Update();
						Update();
					}

					// Ensure that CPU and GPU are synchronized after this point, i.e., the GPU lags behind by
					// at most GraphicsDevice.FrameLag frames
					GraphicsDevice.SyncWithCpu();

					// Draw the current frame
					using (TimeMeasurement.Measure(&drawTime))
					{
						GraphicsDevice.BeginFrame();

						Draw();

						debugOverlay.Draw(SpriteBatch);
						Console.Draw(SpriteBatch);
						SpriteBatch.DrawBatch(Window.BackBuffer);

						GraphicsDevice.EndFrame();
					}

					// Present the contents of the window's backbuffer
					Window.Present();

					// Save CPU when there are no focused windows
					if (!Window.HasFocus)
						Thread.Sleep(10);

					// Update the debug overlay
					debugOverlay.GpuFrameTime = GraphicsDevice.FrameTime;
					debugOverlay.CpuUpdateTime = updateTime;
					debugOverlay.CpuRenderTime = drawTime;
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
		///   Handles window resizing events.
		/// </summary>
		private void OnWindowResized(Size size)
		{
			SpriteBatch.ProjectionMatrix = Matrix.CreateOrthographic(0, size.Width, size.Height, 0, 0, 1);
			Console.ChangeSize(size);
		}
	}
}