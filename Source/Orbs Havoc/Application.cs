// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	internal static class Application
	{
		/// <summary>
		///   The name of the application.
		/// </summary>
		public const string Name = "Orbs Havoc";

		/// <summary>
		///   Initializes the command subsystem.
		/// </summary>
		private static void InitializeCommands()
		{
			Commands.Bind(Key.F1, "start_server");
			Commands.Bind(Key.F2, "stop_server");
			Commands.Bind(Key.F3, "connect ::1");
			Commands.Bind(Key.F4, "disconnect");
			Commands.Bind(Key.F5, "reload_assets");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control), "add_bot");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control | KeyModifiers.Shift), "remove_bot");
			Commands.Bind(new InputTrigger(Key.Escape, KeyModifiers.LeftShift), "exit");
			Commands.Bind(Key.F10, "toggle show_debug_overlay");
			Commands.Help();
		}

		/// <summary>
		///   Runs the application.
		/// </summary>
		public static unsafe void Run()
		{
			using (var graphicsDevice = new GraphicsDevice())
			using (var window = new Window(graphicsDevice, Name, Cvars.WindowPosition, Cvars.WindowSize, Cvars.WindowMode))
			using (var inputDevice = new LogicalInputDevice(window))
			using (var bindings = new BindingCollection(inputDevice))
			using (new AssetBundle())
			using (var renderer = new Renderer(graphicsDevice, window))
			using (var views = new ViewCollection(window, inputDevice))
			{
				// Initialize the views and the command subsystem
				views.Initialize();
				InitializeCommands();

				// Abort when the exit command is invoked
				var running = true;
				Commands.OnExit += () =>
				{
					running = false;
					Log.Info($"Exiting {Name}...");
				};

				while (running)
				{
					double frameTime;

					using (TimeMeasurement.Measure(&frameTime))
					{
						double updateTime, drawTime;

						// Perform the necessary updates for the frame
						using (TimeMeasurement.Measure(&updateTime))
						{
							// Process all pending operating system events
							window.HandleEvents();

							// Update the logical inputs based on the new state of the input system as well as the bindings
							inputDevice.Update();
							bindings.Update();

							// Update the views
							views.Update();
						}

						// Ensure that CPU and GPU are synchronized after this point, i.e., the GPU lags behind by
						// at most GraphicsDevice.FrameLag frames
						graphicsDevice.SyncWithCpu();

						// Draw the current frame
						using (TimeMeasurement.Measure(&drawTime))
						{
							graphicsDevice.BeginFrame();

							renderer.ClearRenderTarget(window.BackBuffer, Colors.Black);
							views.Draw(renderer);
							renderer.Render();

							graphicsDevice.EndFrame();
						}

						// Present the contents of the window's backbuffer
						window.Present();

						// Update the debug overlay
						views.DebugOverlay.GpuFrameTime = graphicsDevice.FrameTime;
						views.DebugOverlay.CpuUpdateTime = updateTime;
						views.DebugOverlay.CpuRenderTime = drawTime;
						views.DebugOverlay.VertexCount = renderer.VertexCount;
						views.DebugOverlay.DrawCalls = renderer.DrawCalls;
					}

					views.DebugOverlay.CpuFrameTime = frameTime;
				}
			}
		}
	}
}