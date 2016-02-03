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

namespace OrbsHavoc.Views
{
	using System.Net.Sockets;
	using Assets;
	using Gameplay.Server;
	using Network;
	using Platform.Graphics;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///   Represents a collection of application views.
	/// </summary>
	internal class ViewCollection : DisposableObject
	{
		private readonly View[] _views;
		private RenderTarget _bloomRenderTarget;
		private bool _exitMessageBoxOpen;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="app">The application the view collection belongs to.</param>
		public ViewCollection(Application app)
		{
			Assert.ArgumentNotNull(app, nameof(app));

			RootElement = new RootUIElement(app.InputDevice);
			RootElement.InputBindings.Add(new ScanCodeBinding(ToggleConsole, ScanCode.Grave) { Preview = true });
			RootElement.InputBindings.Add(new KeyBinding(app.Window.ToggleFullscreen, Key.Enter, KeyModifiers.Alt) { Preview = true });
			RootElement.InputBindings.Add(new KeyBinding(app.Window.ToggleFullscreen, Key.NumpadEnter, KeyModifiers.Alt) { Preview = true });

			Application = app;
			Application.Window.Closing += Exit;
			Application.Window.Resized += OnResized;

			OnResized(Application.Window.Size);

			_views = new View[]
			{
				Console,
				DebugOverlay,
				MessageBoxes,
				MainMenu,
				InGameMenu,
				JoinGameMenu,
				StartGameMenu,
				LoadingOverlay,
				EventMessages,
				Scoreboard,
				Chat,
				WaitingOverlay,
				RespawnOverlay,
				Hud,
				Game
			};

			Commands.OnStartServer += StartHost;
			Commands.OnStopServer += Host.Stop;
		}

		/// <summary>
		///   Gets the event messages.
		/// </summary>
		public EventMessages EventMessages { get; } = new EventMessages();

		/// <summary>
		///   Gets the root UI element of all views within the collection.
		/// </summary>
		public RootUIElement RootElement { get; }

		/// <summary>
		///   Gets the HUD overlay.
		/// </summary>
		public HudOverlay Hud { get; } = new HudOverlay();

		/// <summary>
		///   The in-game chat view.
		/// </summary>
		public Chat Chat { get; } = new Chat();

		/// <summary>
		///   The in-game scoreboard.
		/// </summary>
		public Scoreboard Scoreboard { get; } = new Scoreboard();

		/// <summary>
		///   Gets the local game session host.
		/// </summary>
		public GameSessionHost Host { get; } = new GameSessionHost();

		/// <summary>
		///   Gets the menu that lets the user join a game.
		/// </summary>
		public JoinGameMenu JoinGameMenu { get; } = new JoinGameMenu();

		/// <summary>
		///   Gets the menu that lets the user start a new game.
		/// </summary>
		public StartGameMenu StartGameMenu { get; } = new StartGameMenu();

		/// <summary>
		///   Gets the application the view collection belongs to.
		/// </summary>
		public Application Application { get; }

		/// <summary>
		///   Gets the waiting-for-server overlay.
		/// </summary>
		public WaitingOverlay WaitingOverlay { get; } = new WaitingOverlay();

		/// <summary>
		///   Gets the loading view.
		/// </summary>
		public LoadingOverlay LoadingOverlay { get; } = new LoadingOverlay();

		/// <summary>
		///   Gets the main menu view.
		/// </summary>
		public MainMenu MainMenu { get; } = new MainMenu();

		/// <summary>
		///   Gets the in-game menu view.
		/// </summary>
		public InGameMenu InGameMenu { get; } = new InGameMenu();

		/// <summary>
		///   Gets the view containing the messages boxes.
		/// </summary>
		public MessageBoxes MessageBoxes { get; } = new MessageBoxes();

		/// <summary>
		///   Gets the console view.
		/// </summary>
		public Console Console { get; } = new Console();

		/// <summary>
		///   Gets the debug overlay view.
		/// </summary>
		public DebugOverlay DebugOverlay { get; } = new DebugOverlay();

		/// <summary>
		///   Gets the game session view.
		/// </summary>
		public GameView Game { get; } = new GameView();

		/// <summary>
		///   Gets the respawn overlay.
		/// </summary>
		public RespawnOverlay RespawnOverlay { get; } = new RespawnOverlay();

		/// <summary>
		///   Initializes the views.
		/// </summary>
		public void Initialize()
		{
			for (var i = _views.Length - 1; i >= 0; --i)
			{
				_views[i].Views = this;
				_views[i].Initialize();
			}
		}

		/// <summary>
		///   Updates the views' states.
		/// </summary>
		public void Update()
		{
			foreach (var view in _views)
			{
				if (view.IsShown)
					view.Update();
			}

			RootElement.Update(Application.Window.Size);
			Host.CheckForErrors();
		}

		/// <summary>
		///   Draws the views' contents.
		/// </summary>
		/// <param name="renderer">The renderer that should be used to draw the views.</param>
		public void Draw(Renderer renderer)
		{
			Assert.ArgumentNotNull(renderer, nameof(renderer));

			if (Game.IsRunning)
			{
				if (Cvars.BloomEnabled)
				{
					renderer.ClearRenderTarget(_bloomRenderTarget, Colors.Black);
					Game.Draw(renderer.CreateSpriteBatch(_bloomRenderTarget));
					renderer.Bloom(_bloomRenderTarget, Application.Window.BackBuffer);
				}
				else
					Game.Draw(renderer.CreateSpriteBatch(Application.Window.BackBuffer));
			}

			RootElement.Draw(renderer.CreateSpriteBatch(Application.Window.BackBuffer));
			DrawCursor();
		}

		/// <summary>
		///   Draws the mouse cursor.
		/// </summary>
		private void DrawCursor()
		{
			// Check if the hovered element or any of its parents override the default cursor
			Cursor cursor = null;
			var element = RootElement.HitTest(Application.InputDevice.Mouse.Position, boundsTestOnly: true);

			while (element != null && cursor == null)
			{
				cursor = element.Cursor;
				element = element.Parent;
			}

			cursor = cursor ?? AssetBundle.PointerCursor;
			cursor.Draw();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnStartServer -= StartHost;
			Commands.OnStopServer -= Host.Stop;

			// Remove all views from the root element so that they can execute cleanup logic
			RootElement.Clear();

			Application.Window.Closing -= Exit;
			Application.Window.Resized -= OnResized;
			Host.SafeDispose();

			_bloomRenderTarget.SafeDispose();
			_views.SafeDisposeAll();
		}

		/// <summary>
		///   Resizes the render targets.
		/// </summary>
		private void OnResized(Size size)
		{
			_bloomRenderTarget.SafeDispose();
			_bloomRenderTarget = new RenderTarget(size);
		}

		/// <summary>
		///   Handles a request to exit the application.
		/// </summary>
		public void Exit()
		{
			if (_exitMessageBoxOpen)
				return;

			_exitMessageBoxOpen = true;
			MessageBoxes.ShowYesNo($"Exit {Application.Name}", $"Do you really want to exit {Application.Name}?",
				Commands.Exit, () => _exitMessageBoxOpen = false);
		}

		/// <summary>
		///   Shows or hides the console.
		/// </summary>
		private void ToggleConsole()
		{
			Console.IsShown = !Console.IsShown;
		}

		/// <summary>
		///   Starts a new locally-hosted game session.
		/// </summary>
		public bool TryStartHost(string serverName, ushort serverPort)
		{
			try
			{
				Host.Start(serverName, serverPort);
				return true;
			}
			catch (SocketException e)
			{
				var message = $"Unable to start the server: {e.GetMessage()}";
				Log.Error("{0}", message);
				MessageBoxes.ShowError("Server Failure", message);

				return false;
			}
		}

		/// <summary>
		///   Starts a new locally-hosted game session.
		/// </summary>
		private void StartHost(string serverName, ushort serverPort)
		{
			TryStartHost(serverName, serverPort);
		}
	}
}