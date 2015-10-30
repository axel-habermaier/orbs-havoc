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
	using System;
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
	using Utilities;

	/// <summary>
	///   Represents a collection of application views.
	/// </summary>
	internal class ViewCollection : DisposableObject
	{
		private const int LayersPerView = 1000000;
		private readonly View[] _views;
		private bool _exitMessageBoxOpen;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="app">The application the view collection belongs to.</param>
		public ViewCollection(Application app)
		{
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
		///   Gets the server that hosts game sessions.
		/// </summary>
		public ServerGameSession Server { get; } = new ServerGameSession();

		/// <summary>
		/// Gets the menu that lets the user join a game.
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
			foreach (var view in _views)
			{
				view.Views = this;
				view.RootElement.Initialize(view.InputDevice, view.InputLayer);
				view.Initialize();
			}
		}

		/// <summary>
		///   Updates the views' states.
		/// </summary>
		public void Update()
		{
			// We always update all views, regardless of whether they're currently active; this way,
			// the view can decide to update its own active state, for instance
			foreach (var view in _views)
				view.Update();

			Server.CheckForErrors();
		}

		/// <summary>
		///   Draws the views' contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the views.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			var i = Byte.MaxValue;
			foreach (var view in _views)
			{
				if (!view.IsActive)
					continue;

				spriteBatch.Layer = i * LayersPerView;
				spriteBatch.PositionOffset = Vector2.Zero;
				spriteBatch.ScissorArea = null;

				view.Draw(spriteBatch);
				Assert.That(spriteBatch.Layer < i * LayersPerView + LayersPerView, "The view used too many layers.");

				--i;
			}

			DrawCursor();
		}

		/// <summary>
		///   Draws the mouse cursor.
		/// </summary>
		private void DrawCursor()
		{
			foreach (var view in _views)
			{
				if (!view.IsActive || view.InputLayer == InputLayer.None)
					continue;

				// Check if the hovered element or any of its parents override the default cursor
				Cursor cursor = null;
				var element = view.RootElement.HitTest(Application.InputDevice.Mouse.Position, boundsTestOnly: true);

				while (element != null && cursor == null)
				{
					cursor = element.Cursor;
					element = element.Parent;
				}

				cursor = cursor ?? Assets.PointerCursor;
				cursor.Draw();

				break;
			}
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
			MessageBoxes.Show(MessageBox.CreateYesNo("Are you sure?", $"Do you really want to quit {Application.Name}?",
				Commands.Exit, () => _exitMessageBoxOpen = false));
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
				MessageBoxes.Show(MessageBox.CreateError("Server Failure", message));
			}
		}
	}
}