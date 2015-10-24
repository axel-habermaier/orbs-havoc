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

namespace PointWars.Network
{
	using System.Net;

	/// <summary>
	///   Provides access to network constants.
	/// </summary>
	internal static class NetworkProtocol
	{
		/// <summary>
		///   The application identifier that is used to determine whether a packet has been sent by another instance of the same
		///   application.
		/// </summary>
		public const uint AppIdentifier = 0xf61137c5;

		/// <summary>
		///   The duration in milliseconds that the connection waits for a new packet from the remote peer before the connection is
		///   considered to be dropped.
		/// </summary>
		public const int DroppedTimeout = 10000;

		/// <summary>
		///   The duration in milliseconds that the connection waits for a new packet from the remote peer before the connection is
		///   considered to be lagging.
		/// </summary>
		public const int LaggingTimeout = 500;

		/// <summary>
		///   The maximum allowed byte length of an UTF8-encoded player name.
		/// </summary>
		public const byte PlayerNameLength = 64;

		/// <summary>
		///   The maximum allowed byte length of an UTF8-encoded chat message.
		/// </summary>
		public const byte ChatMessageLength = 255;

		/// <summary>
		///   The maximum allowed packet size in bytes.
		/// </summary>
		public const int MaxPacketSize = 512;

		/// <summary>
		///   The size of the packet header in bytes.
		/// </summary>
		public const int HeaderSize = 8;

		/// <summary>
		///   The maximum allowed number of concurrently active players.
		/// </summary>
		public const int MaxPlayers = 8;

		/// <summary>
		///   The default server port.
		/// </summary>
		public const ushort DefaultServerPort = 32455;

		/// <summary>
		///   The revision number of the network protocol.
		/// </summary>
		public const byte Revision = 1;

		/// <summary>
		///   The time in seconds after which a discovered server has presumably shut down.
		/// </summary>
		public const float DiscoveryTimeout = 5;

		/// <summary>
		///   The frequency in Hz that determines how often a server sends a discovery message.
		/// </summary>
		public const float DiscoveryFrequency = 1;

		/// <summary>
		///   The multicast time to live that is used for automatic server discovery.
		/// </summary>
		public const int MulticastTimeToLive = 1;

		/// <summary>
		///   The multicast group that is used for automatic server discovery.
		/// </summary>
		public static readonly IPEndPoint MulticastGroup = new IPEndPoint(IPAddress.Parse("FF05::3"), 32456);
	}
}