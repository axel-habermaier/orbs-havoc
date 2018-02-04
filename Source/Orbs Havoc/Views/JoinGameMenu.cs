namespace OrbsHavoc.Views
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using Network;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal sealed class JoinGameMenu : View<JoinGameMenuUI>
	{
		private readonly byte[] _buffer = new byte[NetworkProtocol.MaxPacketSize];
		private readonly List<ServerInfo> _discoveredServers = new List<ServerInfo>();
		private bool _connecting;
		private bool _isFaulted;
		private bool _serversDirty = true;
		private Socket _socket;

		private ushort? ServerPort => UInt16.TryParse(UI.Port.Text, out ushort port) ? port : (ushort?)null;

		public override void InitializeUI()
		{
			UI.InputBindings.AddRange(
				new KeyBinding(Close, Key.Escape),
				new KeyBinding(Connect, Key.Enter),
				new KeyBinding(Connect, Key.NumpadEnter)
			);

			UI.Address.TextChanged = OnServerAddressChanged;
			UI.Port.TextChanged = OnPortChanged;
			UI.Connect.Click = Connect;
			UI.Return.Click = () =>
			{
				Hide();
				Views.MainMenu.Show();
			};
		}

		private void OnPortChanged(string port)
		{
			UI.InvalidPort.Visibility = ServerPort == null ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OnServerAddressChanged(string port)
		{
			UI.InvalidAddress.Visibility = TextString.IsNullOrWhiteSpace(UI.Address.Text) ? Visibility.Visible : Visibility.Collapsed;
		}

		protected override void Activate()
		{
			_connecting = false;
			_serversDirty = true;

			UI.Port.Text = NetworkProtocol.DefaultServerPort.ToString();
			UI.InvalidAddress.Visibility = Visibility.Collapsed;
			UI.InvalidPort.Visibility = Visibility.Collapsed;

			try
			{
				_socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
				_socket.InitializeMulticasting(NetworkProtocol.MulticastGroup.Address, (ushort)NetworkProtocol.MulticastGroup.Port);
				_socket.Blocking = false;

				Log.Info("Server discovery started.");
			}
			catch (SocketException e)
			{
				_isFaulted = true;
				Log.Error($"Failed to initialize server discovery: {e.GetMessage()}");
			}
		}

		protected override void Deactivate()
		{
			_socket.SafeDispose();
			_socket = null;
			_discoveredServers.Clear();

			Log.Info("Server discovery stopped.");
		}

		public override void Update()
		{
			RemoveTimedOutServers();

			if (!_isFaulted)
				CheckForDiscoveryMessages();

			if (!_serversDirty)
				return;

			UI.SetServers(_discoveredServers.Select(s => (Name:s.Name, EndPoint: s.EndPoint)));
			_serversDirty = false;
		}

		private void Connect()
		{
			if (_connecting)
				return;

			if (!TextString.IsNullOrWhiteSpace(UI.Address.Text) && ServerPort is ushort port)
			{
				_connecting = true;
				Close();
				Commands.Connect(UI.Address.Text, port);
			}
			else
				UI.InvalidAddress.Visibility = TextString.IsNullOrWhiteSpace(UI.Address.Text) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void Close()
		{
			Hide();
			Views.MainMenu.Show();
		}

		private void HandleDiscoveryMessage(BufferReader reader, EndPoint endPoint)
		{
			if (!TryParseDiscoveryMessage(reader, endPoint, out var name, out var port))
				return;

			var ipEndPoint = new IPEndPoint(((IPEndPoint)endPoint).Address, port);

			// Check if we already know this server; if not add it, otherwise update the server's discovery time
			var server = _discoveredServers.SingleOrDefault(s => s.EndPoint.Equals(ipEndPoint) && s.Name == name);
			if (server == null)
			{
				server = new ServerInfo { EndPoint = ipEndPoint, Name = name, DiscoveryTime = Clock.GetTime() };
				_discoveredServers.Add(server);
				_serversDirty = true;

				Log.Info($"Discovered server '{name}' at {ipEndPoint}.");
			}
			else
				server.DiscoveryTime = Clock.GetTime();
		}

		private static bool TryParseDiscoveryMessage(BufferReader reader, EndPoint endPoint, out string name, out ushort port)
		{
			port = 0;
			name = String.Empty;

			if (!reader.CanRead(sizeof(uint) + sizeof(byte) + sizeof(ushort)))
			{
				Log.Debug($"Ignored invalid discovery message from {endPoint}.");
				return false;
			}

			var applicationIdentifier = reader.ReadUInt32();
			var revision = reader.ReadByte();

			port = reader.ReadUInt16();
			name = reader.ReadString(NetworkProtocol.ServerNameLength);

			if (applicationIdentifier == NetworkProtocol.AppIdentifier && revision == NetworkProtocol.Revision)
				return true;

			Log.Debug($"Ignored invalid discovery message from {endPoint}.");
			return false;
		}

		private void RemoveTimedOutServers()
		{
			for (var i = 0; i < _discoveredServers.Count; ++i)
			{
				if (!_discoveredServers[i].HasTimedOut)
					continue;

				Log.Info($"Server {_discoveredServers[i].EndPoint} is no longer running.");

				_serversDirty = true;
				_discoveredServers.RemoveAt(i);
				--i;
			}
		}

		private void CheckForDiscoveryMessages()
		{
			try
			{
				EndPoint endPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
				while (_socket.Available != 0)
				{
					var size = _socket.ReceiveFrom(_buffer, ref endPoint);
					var reader = new BufferReader(_buffer, 0, size);

					HandleDiscoveryMessage(reader, endPoint);
				}
			}
			catch (SocketException e)
			{
				_isFaulted = true;
				Log.Error($"Server discovery service failure: {e.GetMessage()}");
			}
		}

		protected override void OnDisposing()
		{
			_socket.SafeDispose();
		}

		private class ServerInfo
		{
			public double DiscoveryTime { get; set; }
			public IPEndPoint EndPoint { get; set; }
			public string Name { get; set; }
			public bool HasTimedOut => (Clock.GetTime() - DiscoveryTime) > NetworkProtocol.DiscoveryTimeout;
		}
	}
}