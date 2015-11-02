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

namespace PointWars.Views
{
	using System.Net.Sockets;
	using System.Numerics;
	using Assets;
	using Gameplay.Server;
	using Network;
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

			_views = new View[]
			{
				Console,
				DebugOverlay,
				MessageBoxes,
				MainMenu,
				JoinGameMenu,
				GameSession
			};

			Commands.OnStartServer += StartServer;
			Commands.OnStopServer += Server.Stop;
		}

		/// <summary>
		///   Gets the root UI element of all views within the collection.
		/// </summary>
		public RootUIElement RootElement { get; }

		/// <summary>
		///   Gets the server that hosts game sessions.
		/// </summary>
		public ServerGameSession Server { get; } = new ServerGameSession();

		/// <summary>
		///   Gets the menu that lets the user join a game.
		/// </summary>
		public JoinGameMenu JoinGameMenu { get; } = new JoinGameMenu();

		/// <summary>
		///   Gets the application the view collection belongs to.
		/// </summary>
		public Application Application { get; }

		/// <summary>
		///   Gets the main menu view.
		/// </summary>
		public MainMenu MainMenu { get; } = new MainMenu();

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
		public GameSession GameSession { get; } = new GameSession();

		/// <summary>
		///   Changes the size available to the views.
		/// </summary>
		/// <param name="size">The new size available to the views.</param>
		public void Resize(Size size)
		{
			foreach (var view in _views)
				view.Resize(size);
		}

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
				if (view.IsActive)
					view.Update();
			}

			RootElement.Update(Application.Window.Size);
			Server.CheckForErrors();
		}

		/// <summary>
		///   Draws the views' contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the views.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			spriteBatch.Layer = 0;
			spriteBatch.PositionOffset = Vector2.Zero;
			GameSession.Draw(spriteBatch);

			RootElement.Draw(spriteBatch);
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
			Commands.OnStartServer -= StartServer;
			Commands.OnStopServer -= Server.Stop;

			Server.SafeDispose();
			Application.Window.Closing -= Exit;
			_views.SafeDisposeAll();
		}

		/// <summary>
		///   Handles a request to exit the application.
		/// </summary>
		public void Exit()
		{
			if (_exitMessageBoxOpen)
				return;

			_exitMessageBoxOpen = true;
			MessageBoxes.ShowYesNo("Are you sure?", $"Do you really want to quit {Application.Name}?",
				Commands.Exit, () => _exitMessageBoxOpen = false);
		}

		/// <summary>
		///   Shows or hides the console.
		/// </summary>
		private void ToggleConsole()
		{
			Console.IsActive = !Console.IsActive;
		}

		/// <summary>
		///   Starts a new server game session.
		/// </summary>
		private void StartServer(string serverName, ushort serverPort)
		{
			try
			{
				Server.Start(serverName, serverPort);
			}
			catch (SocketException e)
			{
				var message = $"Unable to start the server: {e.GetMessage()}";
				Log.Error("{0}", message);
				MessageBoxes.ShowError("Server Failure", message);
			}
		}
	}
}