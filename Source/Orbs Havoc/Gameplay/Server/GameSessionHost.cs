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

namespace OrbsHavoc.Gameplay.Server
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Network;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using Utilities;
	using Timer = Utilities.Timer;

	/// <summary>
	///   Represents a server hosting a game session.
	/// </summary>
	public sealed class GameSessionHost : DisposableObject
	{
		/// <summary>
		///   The allocator that is used to allocate server objects.
		/// </summary>
		private readonly PoolAllocator _allocator;

		/// <summary>
		///   The bot commands generated on the main thread.
		/// </summary>
		private readonly ConcurrentQueue<BotCommand> _botCommands = new ConcurrentQueue<BotCommand>();

		/// <summary>
		///   The avaiable and currently unused bot names.
		/// </summary>
		private readonly List<string> _botNames = new List<string>
		{
			"\\greyThe Dark One",
			"\\redDevil 666",
			"\\magentaSuperGirl",
			"Lord Pain",
			"Broken Vector",
			"Annihilator",
			"Dr. Unstoppable",
			"Bad Boy"
		};

		/// <summary>
		///   The currently active bots in the game session.
		/// </summary>
		private readonly List<Player> _bots = new List<Player>();

		/// <summary>
		///   The step timer that is used to update the server at a fixed rate.
		/// </summary>
		private readonly StepTimer _timer = new StepTimer { UseFixedTimeStep = true };

		/// <summary>
		///   Allows the cancellation of the server task.
		/// </summary>
		private CancellationTokenSource _cancellation;

		/// <summary>
		///   The clients connected to the server.
		/// </summary>
		private ClientCollection _clients;

		/// <summary>
		///   The game session that manages the state of the entities.
		/// </summary>
		private GameSession _gameSession;

		/// <summary>
		///   Listens for incoming UDP connections.
		/// </summary>
		private Socket _listener;

		/// <summary>
		///   The timer that is used to schedule player stats updates.
		/// </summary>
		private Timer _playerStatsTimer = new Timer(1000.0f / NetworkProtocol.PlayerStatsUpdateFrequency);

		/// <summary>
		///   Periodically sends server discovery messages.
		/// </summary>
		private ServerDiscovery _serverDiscovery;

		/// <summary>
		///   The server logic that handles the communication between the server and the clients.
		/// </summary>
		private ServerLogic _serverLogic;

		/// <summary>
		///   The task that executes the server.
		/// </summary>
		private Task _task;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public GameSessionHost()
		{
			_timer.TargetElapsedSeconds = 1.0f / NetworkProtocol.ServerUpdateFrequency;
			_timer.UpdateRequired += () => Update(_timer.ElapsedSeconds);
			_allocator = new PoolAllocator();

			Commands.OnAddBot += AddBotDeferred;
			Commands.OnRemoveBot += RemoveBotDeferred;
		}

		/// <summary>
		///   Gets a value indicating whether the server is currently running.
		/// </summary>
		public bool IsRunning => _task != null;

		/// <summary>
		///   Gets a default server name.
		/// </summary>
		public static string DefaultServerName => $"{Environment.UserName}'s Server";

		/// <summary>
		///   Tries to start a server.
		/// </summary>
		/// <param name="serverName">The name of the server that is displayed in the Join screen.</param>
		/// <param name="port">The port that the server should be used to listen for connecting clients.</param>
		public void Start(string serverName, ushort port)
		{
			Stop();

			_serverDiscovery = new ServerDiscovery(serverName, port);

			_gameSession = new GameSession(_allocator);
			_serverLogic = new ServerLogic(_allocator, _gameSession);
			_playerStatsTimer.Timeout += _serverLogic.BroadcastPlayerStats;

			_listener = new Socket(SocketType.Dgram, ProtocolType.Udp);
			_listener.Bind(new IPEndPoint(IPAddress.IPv6Any, port));

			_clients = new ClientCollection(_allocator, _serverLogic, _listener);
			_gameSession.Broadcast = _clients.Broadcast;
			_gameSession.InitializeServer(_serverLogic);

			_cancellation = new CancellationTokenSource();
			var token = _cancellation.Token;

			_task = Task.Run(() =>
			{
				while (!token.IsCancellationRequested)
				{
					_timer.Update();
					Thread.Sleep(1);
				}
			}, token);

			Log.Info("Server '{1}' started on port {0}.", port, serverName);
		}

		/// <summary>
		///   Stops the server, if it is currently running.
		/// </summary>
		public void Stop()
		{
			try
			{
				if (!IsRunning)
					return;

				_cancellation.Cancel();
				_task.Wait();
			}
			catch (AggregateException e)
			{
				try
				{
					e.Handle(inner => inner is OperationCanceledException);
				}
				catch (AggregateException aggregateException)
				{
					Log.Error("One ore more exceptions occurred on the server thread.");

					var exceptions = aggregateException.InnerExceptions.Select(ex =>
					{
						while (ex is TargetInvocationException || ex is TypeInitializationException)
							ex = ex.InnerException;

						return ex;
					}).ToArray();

					foreach (var exception in exceptions)
					{
						Log.Error("Exception type: {0}", exception.GetType().FullName);
						Log.Error("Exception message: {0}", exception.Message);
						Log.Error("Stack trace: {0}", exception.StackTrace);
					}

					var messages = exceptions.Select(ex => ex.Message);
					throw new InvalidOperationException(String.Join("\n\n", messages));
				}
			}
			finally
			{
				if (IsRunning)
					Log.Info("Server stopped.");

				if (_serverLogic != null)
					_playerStatsTimer.Timeout -= _serverLogic.BroadcastPlayerStats;

				_cancellation.SafeDispose();
				_clients.SafeDispose();
				_gameSession.SafeDispose();
				_listener.SafeDispose();
				_serverDiscovery.SafeDispose();

				_task = null;
				_clients = null;
				_listener = null;
				_gameSession = null;
				_serverLogic = null;
				_serverDiscovery = null;

				foreach (var bot in _bots)
					_botNames.Add(bot.Name);

				_bots.Clear();
			}
		}

		/// <summary>
		///   Adds a bot to the currently active game session.
		/// </summary>
		private void AddBotDeferred()
		{
			if (_gameSession != null && _gameSession.Players.Count < NetworkProtocol.MaxPlayers)
				_botCommands.Enqueue(BotCommand.Add);
		}

		/// <summary>
		///   Removes a bot from the currently active game session.
		/// </summary>
		private void RemoveBotDeferred()
		{
			if (_gameSession != null && _bots.Count != 0)
				_botCommands.Enqueue(BotCommand.Remove);
		}

		/// <summary>
		///   Adds a bot to the currently active game session.
		/// </summary>
		private void AddBot()
		{
			if (_gameSession == null || _gameSession.Players.Count >= NetworkProtocol.MaxPlayers)
				return;

			var nameIndex = RandomNumbers.NextIndex(_botNames);
			var bot = _serverLogic.CreatePlayer(_botNames[nameIndex], PlayerKind.Bot);

			_botNames.RemoveAt(nameIndex);
			_bots.Add(bot);

			Log.Info("Bot '{0}\\default' was added to the game session.", bot.Name);
		}

		/// <summary>
		///   Removes a bot from the currently active game session.
		/// </summary>
		private void RemoveBot()
		{
			if (_gameSession == null || _bots.Count == 0)
				return;

			var index = RandomNumbers.NextIndex(_bots);
			var name = _bots[index].Name;

			_bots[index].LeaveReason = LeaveReason.Disconnect;
			_botNames.Add(_bots[index].Name);
			_serverLogic.RemovePlayer(_bots[index]);
			_bots.RemoveAt(index);

			Log.Info("Bot '{0}\\default' was removed from the game session.", name);
		}

		/// <summary>
		///   Updates the server.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have elapsed since the last update.</param>
		private void Update(double elapsedSeconds)
		{
			_serverDiscovery.SendDiscoveryMessage(elapsedSeconds);

			_playerStatsTimer.Update();
			_clients.Update((float)elapsedSeconds);
			_clients.DispatchClientMessages();
			_gameSession.UpdateServer((float)elapsedSeconds);
			_serverLogic.BroadcastEntityUpdates();
			_clients.SendQueuedMessages();

			// Handle all bot commands, which come from the main thread
			BotCommand command;
			while (_botCommands.TryDequeue(out command))
			{
				if (command == BotCommand.Add)
					AddBot();
				else
					RemoveBot();
			}

			// Make sure we respawn the bot when it is dead
			foreach (var bot in _bots)
			{
				if (bot.Orb != null)
					continue;

				bot.RemainingRespawnDelay -= (float)elapsedSeconds;
				_serverLogic.RespawnPlayer(bot);
			}
		}

		/// <summary>
		///   Checks whether any server errors occurred. If so, stops the server and raises an exception.
		/// </summary>
		public void CheckForErrors()
		{
			if (_task != null && _task.IsFaulted)
				Stop();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Stop();
			_allocator.SafeDispose();

			Commands.OnAddBot -= AddBotDeferred;
			Commands.OnRemoveBot -= RemoveBotDeferred;
		}

		private enum BotCommand
		{
			Add,
			Remove
		}
	}
}