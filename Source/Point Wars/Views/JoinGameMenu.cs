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
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Sockets;
	using Assets;
	using Network;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///   Lets the user select a server to connect to.
	/// </summary>
	internal sealed class JoinGameMenu : View
	{
		/// <summary>
		///   The buffer that is used to receive the multi cast data.
		/// </summary>
		private readonly byte[] _buffer = new byte[NetworkProtocol.MaxPacketSize];

		/// <summary>
		///   The list of known servers that have been discovered.
		/// </summary>
		private readonly List<ServerInfo> _discoveredServers = new List<ServerInfo>();

		/// <summary>
		///   Indicates whether the initialization of the socket failed.
		/// </summary>
		private bool _isFaulted;

		/// <summary>
		///   The socket that is used to receive discovery messages.
		/// </summary>
		private Socket _socket;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				CapturesInput = true,
				IsFocusable = true,
				Font = Assets.Roboto14,
				AutoFocus = true,
				InputBindings =
				{
					new KeyBinding(() =>
					{
						IsActive = false;
						Views.MainMenu.IsActive = true;
					}, Key.Escape)
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
							Font = Assets.Moonhouse80,
							Margin = new Thickness(0, 0, 0, 30),
						},
						new Label
						{
							Text = "Click on one of the following servers to join a game:"
						},
						new Button
						{
							Content = "Return",
							HorizontalAlignment = HorizontalAlignment.Center,
							Margin = new Thickness(0, 10, 0, 0),
							Click = () =>
							{
								IsActive = false;
								Views.MainMenu.IsActive = true;
							}
						}
					}
				}
			};
		}

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected override void Activate()
		{
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
					using (var reader = new BufferReader(_buffer, 0, size, Endianess.Big))
						HandleDiscoveryMessage(reader, endPoint);
				}
			}
			catch (SocketException e)
			{
				_isFaulted = true;
				Log.Error("Server discovery service failure: {0}", e.GetMessage());
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

				Log.Info("Discovered server {0}.", ipEndPoint);
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