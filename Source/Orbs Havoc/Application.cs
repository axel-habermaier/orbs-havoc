namespace OrbsHavoc
{
	using Assets;
	using Platform;
	using Platform.Graphics;
	using UserInterface.Input;
	using Platform.Logging;
	using Rendering;
	using Scripting;
	using Utilities;
	using Views;

	internal static class Application
	{
		public const string Name = "Orbs Havoc";

		private static void InitializeCommands()
		{
			Commands.Bind(Key.F1, "start_server");
			Commands.Bind(Key.F2, "stop_server");
			Commands.Bind(Key.F3, "connect ::1");
			Commands.Bind(Key.F4, "disconnect");
			Commands.Bind(Key.F5, "reload_assets");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control), "add_bot");
			Commands.Bind(new InputTrigger(Key.B, KeyModifiers.Control | KeyModifiers.Shift), "remove_bot");
			Commands.Bind(new InputTrigger(Key.Escape, KeyModifiers.Shift), "exit");
			Commands.Bind(Key.F10, "toggle show_debug_overlay");

			Commands.Help();
		}

		public static unsafe void Run()
		{
			using (var graphicsDevice = new GraphicsDevice())
			using (var window = new Window(graphicsDevice, Name, Cvars.WindowPosition, Cvars.WindowSize, Cvars.WindowMode))
			using (new AssetBundle())
			using (var renderer = new Renderer(graphicsDevice, window))
			using (var views = new ViewCollection(window))
			using (var bindings = new BindingCollection(views.Keyboard, views.Mouse))
			{
				views.Initialize();
				InitializeCommands();

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
						var updateTime = Update(views, bindings);
						graphicsDevice.SyncWithCpu();
						var drawTime = Draw(graphicsDevice, renderer, window, views);

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

		private static unsafe double Update(ViewCollection views, BindingCollection bindings)
		{
			double updateTime;

			using (TimeMeasurement.Measure(&updateTime))
			{
				views.HandleInput();

				bindings.Update();
				views.Update();
			}

			return updateTime;
		}

		private static unsafe double Draw(GraphicsDevice graphicsDevice, Renderer renderer, Window window, ViewCollection views)
		{
			double drawTime;

			using (TimeMeasurement.Measure(&drawTime))
			{
				graphicsDevice.BeginFrame();

				renderer.ClearRenderTarget(window.BackBuffer, Colors.Black);
				views.Draw(renderer);
				renderer.Render();

				graphicsDevice.EndFrame();
			}

			window.Present();

			return drawTime;
		}
	}
}