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

namespace OrbsHavoc.Network
{
	using System;
	using System.Net;

	/// <summary>
	///   Provides access to network constants.
	/// </summary>
	internal static class NetworkProtocol
	{
		/// <summary>
		///   The frequency in Hz that determines how often per second the server computes and broadcasts a new game state.
		/// </summary>
		public const int ServerUpdateFrequency = 60;

		/// <summary>
		///   The frequency in Hz that determines how often the user input is sent to the server.
		/// </summary>
		public const int InputUpdateFrequency = 60;

		/// <summary>
		///   The frequency in Hz that determines how often player stats update are sent by the server.
		/// </summary>
		public const int PlayerStatsUpdateFrequency = 1;

		/// <summary>
		///   The application identifier that is used to determine whether a packet has been sent by another instance of the same
		///   application.
		/// </summary>
		public const uint AppIdentifier = 0xf61137c5;

		/// <summary>
		///   The factor that angles sent over the network are scaled with.
		/// </summary>
		public const float AngleFactor = 100.0f;

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
		public const byte PlayerNameLength = 32;

		/// <summary>
		///   The maximum allowed byte length of an UTF8-encoded player name within a packet; must be greater than the maximum player
		///   name length as the server might assign a suffix to make player names unique.
		/// </summary>
		public const byte MessagePlayerNameLength = 40;

		/// <summary>
		///   The maximum allowed byte length of an UTF8-encoded server name.
		/// </summary>
		public const byte ServerNameLength = 32;

		/// <summary>
		///   The maximum allowed byte length of an UTF8-encoded chat message.
		/// </summary>
		public const byte ChatMessageLength = 255;

		/// <summary>
		///   The maximum allowed packet size in bytes.
		/// </summary>
		public const int MaxPacketSize = 1400;

		/// <summary>
		///   The size of the packet header in bytes.
		/// </summary>
		public const int HeaderSize = 8;

		/// <summary>
		///   The maximum allowed number of concurrently active players.
		/// </summary>
		public const int MaxPlayers = 8;

		/// <summary>
		///   The maximum allowed number of concurrently active entities.
		/// </summary>
		public const int MaxEntities = 4096;

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
		///   The multicast group that is used for automatic server discovery.
		/// </summary>
		public static readonly IPEndPoint MulticastGroup = new IPEndPoint(IPAddress.Parse("FF05::3"), 32456);

		/// <summary>
		///   The identity of the player that represents the server.
		/// </summary>
		public static readonly NetworkIdentity ServerPlayerIdentity = new NetworkIdentity(0, 0);

		/// <summary>
		///   The reserved entity identity.
		/// </summary>
		public static readonly NetworkIdentity ReservedEntityIdentity = new NetworkIdentity(UInt16.MaxValue, 0);
	}
}