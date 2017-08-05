namespace OrbsHavoc.Views
{
	using System.Net.Sockets;
	using Assets;
	using Gameplay.Server;
	using Network;
	using Platform;
	using Platform.Graphics;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///     Represents a collection of application views.
	/// </summary>
	internal class ViewCollection : DisposableObject
	{
		private readonly RootUIElement _rootElement;
		private readonly IView[] _views;
		private bool _exitMessageBoxOpen;
		private RenderTarget _gameRenderTarget;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="window">The application window the view is shown in.</param>
		public ViewCollection(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			Window = window;

			_rootElement = new RootUIElement(Window);
			_rootElement.InputBindings.Add(new ScanCodeBinding(ToggleConsole, ScanCode.Grave) { Preview = true });
			_rootElement.InputBindings.Add(new KeyBinding(Window.ToggleFullscreen, Key.Enter, KeyModifiers.Alt) { Preview = true });
			_rootElement.InputBindings.Add(new KeyBinding(Window.ToggleFullscreen, Key.NumpadEnter, KeyModifiers.Alt) { Preview = true });

			Window.Closing += Exit;
			Window.Resized += OnResized;

			OnResized(Window.Size);

			_views = new IView[]
			{
				Console,
				DebugOverlay,
				MessageBoxes,
				MainMenu,
				InGameMenu,
				JoinGameMenu,
				StartGameMenu,
				OptionsMenu,
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
			Cvars.ResolutionChanged += OnResolutionChanged;
		}

		public Window Window { get; }
		public Keyboard Keyboard => _rootElement.Keyboard;
		public Mouse Mouse => _rootElement.Mouse;

		/// <summary>
		///     Gets the event messages.
		/// </summary>
		public EventMessages EventMessages { get; } = new EventMessages();

		/// <summary>
		///     Gets the HUD overlay.
		/// </summary>
		public HudOverlay Hud { get; } = new HudOverlay();

		/// <summary>
		///     The in-game chat view.
		/// </summary>
		public Chat Chat { get; } = new Chat();

		/// <summary>
		///     The in-game scoreboard.
		/// </summary>
		public Scoreboard Scoreboard { get; } = new Scoreboard();

		/// <summary>
		///     Gets the local game session host.
		/// </summary>
		public GameSessionHost Host { get; } = new GameSessionHost();

		/// <summary>
		///     Gets the menu that lets the user join a game.
		/// </summary>
		public JoinGameMenu JoinGameMenu { get; } = new JoinGameMenu();

		/// <summary>
		///     Gets the menu that lets the user start a new game.
		/// </summary>
		public StartGameMenu StartGameMenu { get; } = new StartGameMenu();

		/// <summary>
		///     Gets the waiting-for-server overlay.
		/// </summary>
		public WaitingOverlay WaitingOverlay { get; } = new WaitingOverlay();

		/// <summary>
		///     Gets the loading view.
		/// </summary>
		public LoadingOverlay LoadingOverlay { get; } = new LoadingOverlay();

		/// <summary>
		///     Gets the main menu view.
		/// </summary>
		public MainMenu MainMenu { get; } = new MainMenu();

		/// <summary>
		///     Gets the in-game menu view.
		/// </summary>
		public InGameMenu InGameMenu { get; } = new InGameMenu();

		/// <summary>
		///     Gets the options menu view.
		/// </summary>
		public OptionsMenu OptionsMenu { get; } = new OptionsMenu();

		/// <summary>
		///     Gets the view containing the messages boxes.
		/// </summary>
		public MessageBoxes MessageBoxes { get; } = new MessageBoxes();

		/// <summary>
		///     Gets the console view.
		/// </summary>
		public Console Console { get; } = new Console();

		/// <summary>
		///     Gets the debug overlay view.
		/// </summary>
		public DebugOverlay DebugOverlay { get; } = new DebugOverlay();

		/// <summary>
		///     Gets the game session view.
		/// </summary>
		public GameView Game { get; } = new GameView();

		/// <summary>
		///     Gets the respawn overlay.
		/// </summary>
		public RespawnOverlay RespawnOverlay { get; } = new RespawnOverlay();

		/// <summary>
		///     Hides all views except for the console and the debug overlay.
		/// </summary>
		/// <param name="closeMessageBoxes">Indicates whether all message boxes should be closed.</param>
		public void HideAllViews(bool closeMessageBoxes)
		{
			foreach (var view in _views)
			{
				if (view != Console && view != DebugOverlay && view != MessageBoxes)
					view.Hide();
			}

			if (closeMessageBoxes)
				MessageBoxes.CloseAll();
		}

		public void Initialize()
		{
			for (var i = _views.Length - 1; i >= 0; --i)
			{
				_views[i].Initialize(this);
				_rootElement.Add(_views[i].UI);
			}
		}

		public void HandleInput()
		{
			Keyboard.Update();
			Mouse.Update();
			Window.HandleEvents();
		}

		public void Update()
		{
			foreach (var view in _views)
			{
				view.HandleActivationChange();

				if (view.IsShown)
					view.Update();
			}

			_rootElement.Update(Window.Size);
			Host.CheckForErrors();
		}

		/// <summary>
		///     Draws the views' contents.
		/// </summary>
		/// <param name="renderer">The renderer that should be used to draw the views.</param>
		public void Draw(Renderer renderer)
		{
			Assert.ArgumentNotNull(renderer, nameof(renderer));

			if (Game.IsRunning)
			{
				renderer.ClearRenderTarget(_gameRenderTarget, Colors.Black);
				Game.Draw(renderer.CreateSpriteBatch(_gameRenderTarget));

				if (Cvars.BloomEnabled)
					renderer.Bloom(_gameRenderTarget, Window.BackBuffer);
				else
					renderer.Copy(_gameRenderTarget, Window.BackBuffer);
			}

			_rootElement.Draw(renderer.CreateSpriteBatch(Window.BackBuffer));
			DrawCursor();
		}

		/// <summary>
		///     Draws the mouse cursor.
		/// </summary>
		private void DrawCursor()
		{
			// Check if the hovered element or any of its parents override the default cursor
			Cursor cursor = null;
			var element = _rootElement.HitTest(Mouse.Position, boundsTestOnly: true);

			while (element != null && cursor == null)
			{
				cursor = element.Cursor;
				element = element.Parent;
			}

			cursor = cursor ?? AssetBundle.PointerCursor;
			cursor.Draw();
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnStartServer -= StartHost;
			Commands.OnStopServer -= Host.Stop;
			Cvars.ResolutionChanged -= OnResolutionChanged;

			// Remove all views from the root element so that they can execute cleanup logic
			_rootElement.Clear();

			Window.Closing -= Exit;
			Window.Resized -= OnResized;
			Host.SafeDispose();

			_gameRenderTarget.SafeDispose();
			_views.SafeDisposeAll();
		}

		/// <summary>
		///     Changes the resolution of the game.
		/// </summary>
		private void OnResolutionChanged()
		{
			OnResized(Cvars.Resolution);
		}

		/// <summary>
		///     Resizes the render targets.
		/// </summary>
		private void OnResized(Size size)
		{
			_gameRenderTarget.SafeDispose();

			if (Window.Mode == WindowMode.Fullscreen)
			{
				if (Cvars.ResolutionCvar.HasExplicitValue)
					size = Cvars.Resolution;
				else
				{
					size = Window.Size;
					Cvars.Resolution = size;
				}
			}

			_gameRenderTarget = new RenderTarget(size);
		}

		/// <summary>
		///     Handles a request to exit the application.
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
		///     Shows or hides the console.
		/// </summary>
		private void ToggleConsole()
		{
			Console.IsShown = !Console.IsShown;
		}

		/// <summary>
		///     Starts a new locally-hosted game session.
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
				Log.Error(message);
				MessageBoxes.ShowError("Server Failure", message);

				return false;
			}
		}

		/// <summary>
		///     Starts a new locally-hosted game session.
		/// </summary>
		private void StartHost(string serverName, ushort serverPort)
		{
			TryStartHost(TextString.IsNullOrWhiteSpace(serverName) ? GameSessionHost.DefaultServerName : serverName, serverPort);
		}
	}
}