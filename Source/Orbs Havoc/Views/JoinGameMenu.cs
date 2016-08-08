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
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using Assets;
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
	///   Lets the user select a server to connect to.
	/// </summary>
	internal sealed class JoinGameMenu : View
	{
		private readonly byte[] _buffer = new byte[NetworkProtocol.MaxPacketSize];
		private readonly List<ServerInfo> _discoveredServers = new List<ServerInfo>();
		private readonly Panel _serversPanel = new StackPanel();
		private TextBox _address;
		private bool _connecting;
		private UIElement _invalidAddress;
		private UIElement _invalidPort;
		private bool _isFaulted;
		private bool _panelDirty;
		private TextBox _port;
		private Socket _socket;

		/// <summary>
		///   Gets the server port entered by the user or null if the port is invalid.
		/// </summary>
		private ushort? ServerPort
		{
			get
			{
				ushort port;
				return UInt16.TryParse(_port.Text, out port) ? port : (ushort?)null;
			}
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				CapturesInput = true,
				IsFocusable = true,
				Font = AssetBundle.Roboto14,
				AutoFocus = true,
				InputBindings =
				{
					new KeyBinding(Close, Key.Escape)
				},
				Child = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Children =
					{
						new Label
						{
							Text = "Join Game",
							Font = AssetBundle.Moonhouse80,
							Margin = new Thickness(0, 0, 0, 30),
						},
						new Grid(columns: 2, rows: 5)
						{
							HorizontalAlignment = HorizontalAlignment.Center,
							Children =
							{
								new Label
								{
									Width = 120,
									Row = 0,
									Column = 0,
									VerticalAlignment = VerticalAlignment.Center,
									Text = "Server Address:"
								},
								(_address = new TextBox
								{
									Row = 0,
									Column = 1,
									Margin = new Thickness(5, 0, 0, 5),
									Width = 200,
									MaxLength = NetworkProtocol.ServerNameLength,
									TextChanged = OnServerAddressChanged
								}),
								(_invalidAddress = new Label
								{
									Row = 1,
									Column = 1,
									Text = "Expected a valid server name.",
									Margin = new Thickness(5, 0, 0, 5),
									Foreground = Colors.Red,
									VerticalAlignment = VerticalAlignment.Center,
									Visibility = Visibility.Collapsed,
									Width = 200,
									TextWrapping = TextWrapping.Wrap
								}),
								new Label
								{
									Row = 2,
									Column = 0,
									Width = 120,
									VerticalAlignment = VerticalAlignment.Center,
									Text = "Server Port:"
								},
								(_port = new TextBox
								{
									Row = 2,
									Column = 1,
									Margin = new Thickness(5, 0, 0, 5),
									Width = 200,
									MaxLength = 5,
									TextChanged = OnPortChanged
								}),
								(_invalidPort = new Label
								{
									Width = 200,
									Row = 3,
									Column = 1,
									Text = "Expected a valid server port.",
									Margin = new Thickness(5, 0, 0, 5),
									Foreground = Colors.Red,
									TextWrapping = TextWrapping.Wrap,
									VerticalAlignment = VerticalAlignment.Center,
									Visibility = Visibility.Collapsed
								}),
								new Button
								{
									Row = 4,
									Column = 1,
									Content = "Connect",
									HorizontalAlignment = HorizontalAlignment.Left,
									Margin = new Thickness(5, 10, 0, 50),
									Click = Connect
								}
							}
						},
						new StackPanel
						{
							Width = 330,
							Children =
							{
								new Label
								{
									TextWrapping = TextWrapping.Wrap,
									Text = "Click on one of the following servers to join a game:"
								},
								_serversPanel
							}
						},
						new Button
						{
							Content = "Return",
							HorizontalAlignment = HorizontalAlignment.Center,
							Margin = new Thickness(0, 10, 0, 0),
							Click = () =>
							{
								Hide();
								Views.MainMenu.Show();
							}
						}
					}
				}
			};
		}

		/// <summary>
		///   Invoked when the user entered another port.
		/// </summary>
		private void OnPortChanged(string port)
		{
			_invalidPort.Visibility = ServerPort == null ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Invoked when the user entered another server address.
		/// </summary>
		private void OnServerAddressChanged(string port)
		{
			_invalidAddress.Visibility = String.IsNullOrWhiteSpace(_address.Text) ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected override void Activate()
		{
			_connecting = false;
			_port.Text = NetworkProtocol.DefaultServerPort.ToString();
			_invalidAddress.Visibility = Visibility.Collapsed;
			_invalidPort.Visibility = Visibility.Collapsed;

			_panelDirty = true;
			UpdatePanel();

			try
			{
				_socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
				_socket.InitializeMulticasting();
				_socket.Blocking = false;

				Log.Info("Server discovery started.");
			}
			catch (SocketException e)
			{
				_isFaulted = true;
				Log.Error("Failed to initialize server discovery: {0}", e.GetMessage());
			}
		}

		/// <summary>
		///   Invoked when the view should be deactivated.
		/// </summary>
		protected override void Deactivate()
		{
			_socket.SafeDispose();
			_socket = null;
			_discoveredServers.Clear();

			Log.Info("Server discovery stopped.");
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			// Remove all servers that have timed out
			for (var i = 0; i < _discoveredServers.Count; ++i)
			{
				if (!_discoveredServers[i].HasTimedOut)
					continue;

				Log.Info("Server {0} is no longer running.", _discoveredServers[i].EndPoint);

				_panelDirty = true;
				_discoveredServers.RemoveAt(i);
				--i;
			}

			if (_isFaulted)
				return;

			try
			{
				// Check for incoming discovery messages
				EndPoint endPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
				while (_socket.Available != 0)
				{
					var size = _socket.ReceiveFrom(_buffer, ref endPoint);
					var reader = new BufferReader(_buffer, 0, size, Endianess.Big);

					HandleDiscoveryMessage(reader, endPoint);
				}
			}
			catch (SocketException e)
			{
				_isFaulted = true;
				Log.Error("Server discovery service failure: {0}", e.GetMessage());
			}

			UpdatePanel();
		}

		/// <summary>
		///   Connects to the server with the endpoint entered by the user.
		/// </summary>
		private void Connect()
		{
			if (_connecting)
				return;

			if (!String.IsNullOrWhiteSpace(_address.Text) && ServerPort != null)
			{
				_connecting = true;
				Close();
				Commands.Connect(_address.Text, ServerPort.Value);
			}
			else
				_invalidAddress.Visibility = String.IsNullOrWhiteSpace(_address.Text) ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Closes the view.
		/// </summary>
		private void Close()
		{
			Hide();
			Views.MainMenu.Show();
		}

		/// <summary>
		///   Updates the server panel.
		/// </summary>
		private void UpdatePanel()
		{
			if (!_panelDirty)
				return;

			_panelDirty = false;
			_serversPanel.Clear();

			foreach (var server in _discoveredServers.OrderBy(s => s.Name))
			{
				_serversPanel.Add(new Button
				{
					Content = new Label { Text = $"{server.Name} @ {server.EndPoint}", TextWrapping = TextWrapping.Wrap },
					Margin = new Thickness(0, 2, 0, 2),
					Click = () => Commands.Connect(server.EndPoint.Address.ToString(), (ushort)server.EndPoint.Port)
				});
			}
		}

		/// <summary>
		///   Handles the discovery message that has just been received.
		/// </summary>
		/// <param name="reader">The reader that should be used to read the contents of the discovery message.</param>
		/// <param name="endPoint">The endpoint of the sender of the discovery message.</param>
		private void HandleDiscoveryMessage(BufferReader reader, EndPoint endPoint)
		{
			if (!reader.CanRead(sizeof(uint) + sizeof(byte) + sizeof(ushort)))
			{
				Log.Debug("Ignored invalid discovery message from {0}.", endPoint);
				return;
			}

			var applicationIdentifier = reader.ReadUInt32();
			var revision = reader.ReadByte();

			var port = reader.ReadUInt16();
			var name = reader.ReadString(NetworkProtocol.ServerNameLength);

			if (applicationIdentifier != NetworkProtocol.AppIdentifier || revision != NetworkProtocol.Revision)
			{
				Log.Debug("Ignored invalid discovery message from {0}.", endPoint);
				return;
			}

			var ipEndPoint = new IPEndPoint(((IPEndPoint)endPoint).Address, port);

			// Check if we already know this server; if not add it, otherwise update the server's discovery time
			var server = _discoveredServers.SingleOrDefault(s => s.EndPoint.Equals(ipEndPoint) && s.Name == name);
			if (server == null)
			{
				server = new ServerInfo { EndPoint = ipEndPoint, Name = name, DiscoveryTime = Clock.GetTime() };
				_discoveredServers.Add(server);
				_panelDirty = true;

				Log.Info("Discovered server '{1}' at {0}.", ipEndPoint, name);
			}
			else
				server.DiscoveryTime = Clock.GetTime();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_socket.SafeDispose();
		}

		/// <summary>
		///   Stores information about a discovered server.
		/// </summary>
		private class ServerInfo
		{
			/// <summary>
			///   Gets or sets the last time a discovery message has been received from the server.
			/// </summary>
			public double DiscoveryTime { get; set; }

			/// <summary>
			///   Gets or sets the end point of the server.
			/// </summary>
			public IPEndPoint EndPoint { get; set; }

			/// <summary>
			///   Gets or sets the name of the server.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			///   Gets a value indicating whether the server has timed out and is presumably no longer running.
			/// </summary>
			public bool HasTimedOut => (Clock.GetTime() - DiscoveryTime) > NetworkProtocol.DiscoveryTimeout;
		}
	}
}