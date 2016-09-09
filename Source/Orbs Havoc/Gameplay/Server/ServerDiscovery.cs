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
	using System.Net.Sockets;
	using Network;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Sends server discovery messages.
	/// </summary>
	internal class ServerDiscovery : DisposableObject
	{
		/// <summary>
		///   The number of times the socket should be recreated after a socket fault before giving up.
		/// </summary>
		private const int RetryCount = 10;

		/// <summary>
		///   A cached buffer that is used to hold the contents of the discovery messages.
		/// </summary>
		private readonly byte[] _buffer = new byte[8 + NetworkProtocol.ServerNameLength];

		/// <summary>
		///   The number of times the socket has faulted and has been recreated.
		/// </summary>
		private int _faultCount;

		/// <summary>
		///   The number of seconds that have passed since the last discovery message has been sent.
		/// </summary>
		private double _secondsSinceLastDiscoveryMessage = Double.MaxValue;

		/// <summary>
		///   The UDP socket that is used to send server discovery messages.
		/// </summary>
		private Socket _socket;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="serverName">The name of the server that should be sent in the discovery message.</param>
		/// <param name="serverPort">The port that the server is using to communicate with its clients.</param>
		public ServerDiscovery(string serverName, ushort serverPort)
		{
			Assert.ArgumentNotNull(serverName, nameof(serverName));

			serverName = String.IsNullOrWhiteSpace(serverName) ? "Unnamed Server" : serverName.Trim();
			if (serverName.Length > NetworkProtocol.ServerNameLength)
				serverName = serverName.Substring(0, NetworkProtocol.ServerNameLength);

			var writer = new BufferWriter(_buffer, Endianess.Big);
			writer.WriteUInt32(NetworkProtocol.AppIdentifier);
			writer.WriteByte(NetworkProtocol.Revision);
			writer.WriteUInt16(serverPort);
			writer.WriteString(serverName, NetworkProtocol.ServerNameLength);
		}

		/// <summary>
		///   Periodically sends a discovery message for the server.
		/// </summary>
		/// <param name="elapsedSeconds">The number of seconds that have passed since the last update.</param>
		public void SendDiscoveryMessage(double elapsedSeconds)
		{
			try
			{
				if (_faultCount > RetryCount)
					return;

				if (_socket == null)
				{
					_socket = new Socket(SocketType.Dgram, ProtocolType.Udp);
					_socket.InitializeMulticasting(NetworkProtocol.MulticastGroup.Address, port: 0);
					_socket.Connect(NetworkProtocol.MulticastGroup);
				}

				_secondsSinceLastDiscoveryMessage += elapsedSeconds;
				if (_secondsSinceLastDiscoveryMessage < 1 / NetworkProtocol.DiscoveryFrequency)
					return;

				_socket.Send(_buffer);
				_secondsSinceLastDiscoveryMessage = 0;
			}
			catch (SocketException e)
			{
				if (_faultCount >= RetryCount)
					Log.Error($"Server discovery has been disabled. {e.Message}");

				++_faultCount;
				_socket.SafeDispose();
				_socket = null;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_socket.SafeDispose();
		}
	}
}