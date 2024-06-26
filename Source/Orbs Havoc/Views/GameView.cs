namespace OrbsHavoc.Views
{
	using System.Net;
	using System.Net.Sockets;
	using System.Numerics;
	using System.Threading;
	using Assets;
	using Gameplay;
	using Gameplay.Client;
	using Gameplay.SceneNodes;
	using Network;
	using Network.Messages;
	using Platform.Graphics;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///     Represents the application view of playing a game session.
	/// </summary>
	internal sealed class GameView : View<Border>
	{
		private readonly PoolAllocator _allocator = new PoolAllocator();

		private ClientLogic _clientLogic;
		private Clock _clock = new Clock();
		private InputManager _inputManager;

		/// <summary>
		///     The camera that is used to draw the game session.
		/// </summary>
		public Camera Camera { get; } = new Camera();

		/// <summary>
		///     Gets the currently active server connection.
		/// </summary>
		public Connection Connection { get; private set; }

		/// <summary>
		///     Gets the currently active game session.
		/// </summary>
		public GameSession GameSession { get; private set; }

		/// <summary>
		///     Gets a value indicating whether a game session is currently running.
		/// </summary>
		public bool IsRunning => GameSession != null;

		/// <summary>
		///     Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Commands.OnConnect += Connect;
			Commands.OnDisconnect += Disconnect;
			Commands.OnSay += OnSay;
			Cvars.PlayerNameChanged += OnPlayerNameChanged;
		}

		public override void InitializeUI()
		{
			UI.CapturesInput = true;
			UI.AutoFocus = true;
			UI.Cursor = AssetBundle.Crosshair;

			UI.InputBindings.AddRange(
				new ConfigurableBinding(Views.Chat.Show, Cvars.InputChatCvar),
				new KeyBinding(Views.InGameMenu.Show, Key.Escape)
			);
		}

		/// <summary>
		///     Updates the view's state.
		/// </summary>
		public override void Update()
		{
			try
			{
				if (!_clientLogic.IsSynced)
				{
					Connection.SendQueuedMessages();
					Connection.DispatchReceivedMessages(_clientLogic);

					if (!_clientLogic.IsSynced)
						return;

					Log.Info($"Loading completed and game state synced. Now connected to game session hosted by {Connection.RemoteEndPoint}.");
					Views.LoadingOverlay.Hide();

					// Resend player name, as it might have been changed during the connection attempt
					OnPlayerNameChanged();

					_clock.Reset();
					_inputManager = new InputManager(GameSession.Players.LocalPlayer, Window, Views.Keyboard, Views.Mouse);

					// Remove all event messages that have been added, e.g., for player joins
					Views.EventMessages.Clear();
				}
				else
				{
					Connection.DispatchReceivedMessages(_clientLogic);

					var elapsedSeconds = (float)_clock.Seconds;
					_clock.Reset();

					GameSession.UpdateClient(elapsedSeconds);
					Connection.SendQueuedMessages();

					if (Connection.IsLagging)
						Views.WaitingOverlay.Show();

					// Always send the input state, but update it only when the game session is focused 
					if (UI.IsFocused)
					{
						_inputManager.Update();
						_inputManager.SendActiveInput(GameSession, Connection);
					}
					else
						_inputManager.SendInactiveInput(GameSession, Connection);

					Views.Hud.IsShown = !GameSession.IsLocalPlayerDead;
					Views.RespawnOverlay.IsShown = GameSession.IsLocalPlayerDead;

					Views.Scoreboard.IsShown = UI.IsFocused && (_inputManager.ShowScoreboard || GameSession.IsLocalPlayerDead);
				}
			}
			catch (ConnectionDroppedException)
			{
				var wasSynced = _clientLogic.IsSynced;
				Commands.Disconnect();

				if (!wasSynced)
					Views.MessageBoxes.ShowError("Connection Failed", "Unable to connect to the server. The connection attempt timed out.");
				else
					Views.MessageBoxes.ShowError("Connection Lost", "The connection to the server has been lost.");
			}
			catch (ServerFullException)
			{
				Commands.Disconnect();
				Views.MessageBoxes.ShowError("Connection Rejected", "The server is full and does not accept any additional players.");
			}
			catch (ProtocolMismatchException)
			{
				Commands.Disconnect();
				Views.MessageBoxes.ShowError("Connection Rejected", "The server uses an incompatible version of the network protocol.");
			}
			catch (ServerQuitException)
			{
				Commands.Disconnect();
				Views.MessageBoxes.ShowError("Server Shutdown", "The server has ended the game session.");
			}
			catch (NetworkException e)
			{
				var wasSynced = _clientLogic.IsSynced;
				Commands.Disconnect();

				if (!wasSynced)
					Views.MessageBoxes.ShowError("Connection Error", $"The connection attempt has been aborted due to a network error: {e.Message}");
				else
					Views.MessageBoxes.ShowError("Connection Error", $"The game session has been aborted due to a network error: {e.Message}");
			}
		}

		/// <summary>
		///     Draws the game session.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			if (_clientLogic == null || !_clientLogic.IsSynced)
				return;

			// The orb, if alive, should always be at the center of the screen
			var orb = GameSession.Players?.LocalPlayer?.Orb;
			if (orb != null)
			{
				var windowCenter = new Vector2(MathUtils.Round(Window.Size.Width / 2), MathUtils.Round(Window.Size.Height / 2));
				Camera.Position = windowCenter - orb.WorldPosition;
			}

			// Draw the level first; everything else is drawn above
			spriteBatch.RenderState.Camera = Camera;
			GameSession.LevelRenderer.Draw(spriteBatch);

			// Draw the entity sprites next, using layers to control draw order
			GameSession.EntityRenderer.Draw(spriteBatch);

			// Draw the particles last, on top of everything, using additive blending
			spriteBatch.RenderState.BlendOperation = BlendOperation.Additive;
			spriteBatch.RenderState.Layer = 10000;

			foreach (var particleNode in GameSession.SceneGraph.EnumeratePostOrder<ParticleEffectNode>())
			{
				if (UI.IsFocused || particleNode != GameSession.MouseEffect)
					particleNode.Draw(spriteBatch);
			}

			spriteBatch.RenderState.BlendOperation = BlendOperation.Premultiplied;
		}

		/// <summary>
		///     Invoked when the client should connect to a game session.
		/// </summary>
		private void Connect(string serverAddress, ushort serverPort)
		{
			if (_clientLogic != null)
			{
				// We're already connected, so disconnect first, then wait a bit for the server to actually remove
				// the player; this prevents "server is full" situations when the player has not yet been removed
				// before he connects again to the same server
				Disconnect();
				Thread.Sleep(75);
			}

			// We always use the first address, which might be problematic for internet play
			// but should not be a problem in a local area network
			IPEndPoint endpoint;
			try
			{
				var ipAddresses = Dns.GetHostAddresses(serverAddress);
				if (ipAddresses == null || ipAddresses.Length == 0)
				{
					Views.MessageBoxes.ShowError("Connection Error", "The specified server address could not be resolved.");
					return;
				}

				endpoint = new IPEndPoint(ipAddresses[0], serverPort);
			}
			catch (SocketException e)
			{
				Views.MessageBoxes.ShowError("Connection Error", $"The connection attempt has been aborted due to a network error: {e.GetMessage()}");
				return;
			}

			GameSession = new GameSession(_allocator);
			Connection = Connection.Create(_allocator, endpoint);

			_clientLogic = new ClientLogic(_allocator, GameSession, Views);

			GameSession.InitializeClient();
			Connection.EnqueueMessage(ClientConnectMessage.Create(_allocator, Cvars.PlayerName));

			Show();
			Views.EventMessages.Show();

			Views.LoadingOverlay.Load(Connection.RemoteEndPoint);
		}

		/// <summary>
		///     Invoked when the client should disconnect from a game session.
		/// </summary>
		private void Disconnect()
		{
			Views.HideAllViews(closeMessageBoxes: true);
			Views.MainMenu.Show();
			Views.EventMessages.Clear();

			GameSession.SafeDispose();
			Connection.SafeDispose();

			if (_clientLogic != null && _clientLogic.IsSynced)
				Log.Info("The game session has ended.");

			GameSession = null;
			Connection = null;

			_inputManager = null;
			_clientLogic = null;
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnConnect -= Connect;
			Commands.OnDisconnect -= Disconnect;
			Commands.OnSay -= OnSay;
			Cvars.PlayerNameChanged -= OnPlayerNameChanged;

			Disconnect();
			_allocator.SafeDispose();
			Camera.SafeDispose();
		}

		/// <summary>
		///     Invoked when the local player changed his or her name.
		/// </summary>
		private void OnPlayerNameChanged()
		{
			Connection?.EnqueueMessage(PlayerNameMessage.Create(_allocator, GameSession.Players.LocalPlayer.Identity, Cvars.PlayerName));
		}

		/// <summary>
		///     Invoked when the local player entered a chat message.
		/// </summary>
		/// <param name="message">The message that the local player wants to send.</param>
		private void OnSay(string message)
		{
			if (GameSession?.Players.LocalPlayer == null)
				return;

			Connection?.EnqueueMessage(PlayerChatMessage.Create(_allocator, GameSession.Players.LocalPlayer.Identity, message));
		}
	}
}