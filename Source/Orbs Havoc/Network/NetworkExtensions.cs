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

namespace OrbsHavoc.Network
{
	using System.Net;
	using System.Net.Sockets;
	using Utilities;

	/// <summary>
	///   Provides extension methods for network related .NET framework classes.
	/// </summary>
	public static class NetworkExtensions
	{
		/// <summary>
		///   Gets the cleaned-up message of the given exception.
		/// </summary>
		public static string GetMessage(this SocketException e)
		{
			Assert.ArgumentNotNull(e, nameof(e));

			var message = e.Message.Trim();
			if (!message.EndsWith("."))
				message += ".";

			return message;
		}

		/// <summary>
		///   Initializes the socket to support multicasting.
		/// </summary>
		public static void InitializeMulticasting(this Socket socket)
		{
			Assert.ArgumentNotNull(socket, nameof(socket));

			socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership,
				new IPv6MulticastOption(NetworkProtocol.MulticastGroup.Address));
			socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, 255);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			socket.Bind(new IPEndPoint(IPAddress.IPv6Any, NetworkProtocol.MulticastGroup.Port));
		}
	}
}