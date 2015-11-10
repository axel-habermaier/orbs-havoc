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
	using System.Net;
	using System.Numerics;
	using Gameplay;
	using Gameplay.Client;
	using Gameplay.SceneNodes;
	using Network;
	using Network.Messages;
	using Platform.Graphics;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///   Represents the application view of playing a game session.
	/// </summary>
	internal sealed class GameView : View
	{
		private readonly PoolAllocator _allocator = new PoolAllocator();
		private Camera _camera = new Camera();

		private ClientLogic _clientLogic;
		private Clock _clock = new Clock();
		private InputManager _inputManager;
		private LogicalInput _showScoreboard;

		/// <summary>
		///   Gets the currently active server connection.
		/// </summary>
		public Connection Connection { get; private set; }

		/// <summary>
		///   Gets the currently active game session.
		/// </summary>
		public GameSession GameSession { get; private set; }

		/// <summary>
		///   Gets the remote end point of the server.
		/// </summary>
		public IPEndPoint ServerEndPoint { get; private set; }

		/// <summary>
		///   Gets a value indicating whether a game session is currently running.
		/// </summary>
		public bool IsRunning => GameSession != null;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Commands.OnConnect += Connect;
			Commands.OnDisconnect += Disconnect;
			Commands.OnSay += OnSay;
			Cvars.PlayerNameChanged += OnPlayerNameChanged;

			_inputManager = new InputManager(InputDevice);
			_showScoreboard = new LogicalInput(Cvars.InputShowScoreboardCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);

			InputDevice.Add(_showScoreboard);

			RootElement = new Border
			{
				CapturesInput = true,
				AutoFocus = true,
				InputBindings =
				{
					new ConfigurableBinding(Views.Chat.Show, Cvars.InputChatCvar),
					new KeyBinding(Views.InGameMenu.Show, Key.Escape)
				}
			};
		}

		/// <summary>
		///   Updates the view's state.
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

					Log.Info("Loading completed and game state synced. Now connected to game session hosted by {0}.", ServerEndPoint);
					Views.LoadingOverlay.Hide();

					// Resend player name, as it might have been changed during the connection attempt
					OnPlayerNameChanged();

					_clock.Reset();

					// Remove all event messages that have been added, e.g., for player joins
					Views.EventMessages.Clear();
				}
				else
				{
					Connection.DispatchReceivedMessages(_clientLogic);

					var elapsedSeconds = (float)_clock.Seconds;
					_clock.Reset();

					GameSession.Update(elapsedSeconds);
					Connection.SendQueuedMessages();

					if (Connection.IsLagging)
						Views.WaitingOverlay.Show();

					// Always send the input state, but update it only when the game session is focused 
					if (RootElement.IsFocused)
					{
						_inputManager.Update();
						_inputManager.SendInput(GameSession, Connection);
					}
					else
						_inputManager.SendInactiveInput(GameSession, Connection);

					Views.Hud.IsShown = !GameSession.IsLocalPlayerDead;
					Views.RespawnOverlay.IsShown = GameSession.IsLocalPlayerDead;
					Views.Scoreboard.IsShown = RootElement.IsFocused && (_showScoreboard.IsTriggered || GameSession.IsLocalPlayerDead);
				}
			}
			catch (ConnectionDroppedException)
			{
				var wasSynced = _clientLogic.IsSynced;
				Commands.Disconnect();

				if (!wasSynced)
					Views.MessageBoxes.ShowError("Connection Failed", $"Unable to connect to {ServerEndPoint}. The connection attempt timed out.");
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
		///   Draws the game session.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			if (_clientLogic == null || !_clientLogic.IsSynced)
				return;

			// The avatar, if alive, should always be at the center of the screen
			var avatar = GameSession.Players?.LocalPlayer?.Avatar;
			if (avatar != null)
			{
				var windowCenter = new Vector2(MathUtils.Round(Window.Size.Width / 2), MathUtils.Round(Window.Size.Height / 2));
				_camera.Position = windowCenter - avatar.WorldPosition;
			}

			// Draw the level first; everything else is drawn above
			GameSession.LevelRenderer.Draw(spriteBatch);

			// Draw the entity sprites next, using layers to control draw order
			foreach (var spriteNode in GameSession.SceneGraph.EnumeratePostOrder<SpriteNode>())
				spriteNode.Draw(spriteBatch);

			// Draw the particles last, on top of everything, using additive blending
			spriteBatch.RenderState.BlendOperation = BlendOperation.Additive;
			spriteBatch.RenderState.Layer = 10000;
			foreach (var particleNode in GameSession.SceneGraph.EnumeratePostOrder<ParticleEffectNode>())
				particleNode.Draw(spriteBatch);

			spriteBatch.RenderState.BlendOperation = BlendOperation.Premultiplied;
		}

		/// <summary>
		///   Invoked when the client should connect to a game session.
		/// </summary>
		private void Connect(IPAddress serverAddress, ushort serverPort)
		{
			Disconnect();

			ServerEndPoint = new IPEndPoint(serverAddress, serverPort);

			GameSession = new GameSession(_allocator);
			Connection = Connection.Create(_allocator, ServerEndPoint);

			_clientLogic = new ClientLogic(_allocator, GameSession, Views);

			GameSession.InitializeClient();
			Connection.EnqueueMessage(ClientConnectMessage.Create(_allocator, Cvars.PlayerName));

			Show();
			Views.LoadingOverlay.Load(ServerEndPoint);
		}

		/// <summary>
		///   Invoked when the client should disconnect from a game session.
		/// </summary>
		private void Disconnect()
		{
			Views.LoadingOverlay.Hide();
			Views.MainMenu.Show();
			Views.Chat.Hide();
			Views.EventMessages.Clear();
			Views.InGameMenu.Hide();
			Views.MessageBoxes.CloseAll();
			Views.Scoreboard.Hide();
			Views.WaitingOverlay.Hide();
			Views.Hud.Hide();
			Views.RespawnOverlay.Hide();
			Hide();

			GameSession.SafeDispose();
			Connection.SafeDispose();

			if (_clientLogic != null && _clientLogic.IsSynced)
				Log.Info("The game session has ended.");

			GameSession = null;
			Connection = null;

			_clientLogic = null;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnConnect -= Connect;
			Commands.OnDisconnect -= Disconnect;
			Commands.OnSay -= OnSay;
			Cvars.PlayerNameChanged -= OnPlayerNameChanged;

			Disconnect();
			_allocator.SafeDispose();
			_inputManager.SafeDispose();
			_camera.SafeDispose();

			InputDevice.Remove(_showScoreboard);
		}

		/// <summary>
		///   Invoked when the local player changed his or her name.
		/// </summary>
		private void OnPlayerNameChanged()
		{
			Connection?.EnqueueMessage(PlayerNameMessage.Create(_allocator, GameSession.Players.LocalPlayer.Identity, Cvars.PlayerName));
		}

		/// <summary>
		///   Invoked when the local player entered a chat message.
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