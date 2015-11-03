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
	using Gameplay;
	using Network;
	using Network.Messages;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents the application view of playing a game session.
	/// </summary>
	internal sealed class GameSessionView : View
	{
		private readonly PoolAllocator _allocator = new PoolAllocator();

		private ClientLogic _clientLogic;
		private Clock _clock = new Clock();
		private Connection _connection;
		private GameSession _gameSession;

		/// <summary>
		///   Gets the remote end point of the server.
		/// </summary>
		public IPEndPoint ServerEndPoint { get; private set; }

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Commands.OnConnect += Connect;
			Commands.OnDisconnect += Disconnect;
			Commands.OnSay += OnSay;
			Cvars.PlayerNameChanged += OnPlayerNameChanged;
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
					_connection.SendQueuedMessages();
					_connection.DispatchReceivedMessages(_clientLogic);

					if (!_clientLogic.IsSynced)
						return;

					Log.Info("Loading completed and game state synced. Now connected to game session hosted by {0}.", ServerEndPoint);
					Views.LoadingView.IsActive = false;

					// Resend player name, as it might have been changed during the connection attempt
					OnPlayerNameChanged();

					_clock.Reset();
				}
				else
				{
					_connection.DispatchReceivedMessages(_clientLogic);

					var elapsedSeconds = (float)_clock.Seconds;
					_clock.Reset();

					_gameSession.Update(elapsedSeconds);
					_connection.SendQueuedMessages();
				}
			}
			catch (ConnectionDroppedException)
			{
				if (!_clientLogic.IsSynced)
					Views.MessageBoxes.ShowError("Connection Failed", $"Unable to connect to {ServerEndPoint}. The connection attempt timed out.");
				else
					Views.MessageBoxes.ShowError("Connection Lost", "The connection to the server has been lost.");

				Commands.Disconnect();
			}
			catch (ServerFullException)
			{
				Views.MessageBoxes.ShowError("Connection Rejected", "The server is full and does not accept any additional players.");
				Commands.Disconnect();
			}
			catch (ProtocolMismatchException)
			{
				Views.MessageBoxes.ShowError("Connection Rejected", "The server uses an incompatible version of the network protocol.");
				Commands.Disconnect();
			}
			catch (ServerQuitException)
			{
				Views.MessageBoxes.ShowError("Server Shutdown", "The server has ended the game session.");
				Commands.Disconnect();
			}
			catch (NetworkException e)
			{
				Views.MessageBoxes.ShowError("Connection Error", $"The game session has been aborted due to a network error: {e.Message}");
				Commands.Disconnect();
			}
		}

		/// <summary>
		///   Draws the game session.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
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
		}

		/// <summary>
		///   Invoked when the client should connect to a game session.
		/// </summary>
		private void Connect(IPAddress serverAddress, ushort serverPort)
		{
			Disconnect();

			ServerEndPoint = new IPEndPoint(serverAddress, serverPort);

			_gameSession = new GameSession(_allocator);
			_clientLogic = new ClientLogic(_allocator, _gameSession);
			_connection = Connection.Create(_allocator, ServerEndPoint);

			_gameSession.InitializeClient();
			_connection.EnqueueMessage(ClientConnectMessage.Create(_allocator, Cvars.PlayerName));

			IsActive = true;
			Views.LoadingView.Load(ServerEndPoint);
		}

		/// <summary>
		///   Invoked when the local player changed his or her name.
		/// </summary>
		private void OnPlayerNameChanged()
		{
			_connection?.EnqueueMessage(PlayerNameMessage.Create(_allocator, _gameSession.Players.LocalPlayer.Identity, Cvars.PlayerName));
		}

		/// <summary>
		///   Invoked when the local player entered a chat message.
		/// </summary>
		/// <param name="message">The message that the local player wants to send.</param>
		private void OnSay(string message)
		{
			if (_gameSession?.Players.LocalPlayer == null)
				return;

			_connection?.EnqueueMessage(PlayerChatMessage.Create(_allocator, _gameSession.Players.LocalPlayer.Identity, message));
		}

		/// <summary>
		///   Invoked when the client should disconnect from a game session.
		/// </summary>
		private void Disconnect()
		{
			Views.LoadingView.IsActive = false;
			Views.MainMenu.IsActive = true;
			IsActive = false;

			_gameSession.SafeDispose();
			_connection.SafeDispose();

			if (_clientLogic != null && _clientLogic.IsSynced)
				Log.Info("The game session has ended.");
			else if (_gameSession != null)
				Log.Info("Connection attempt failed or was aborted.");

			_gameSession = null;
			_clientLogic = null;
			_connection = null;
		}
	}
}