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
		/// <param name="exception">The exception the message should be retrieved for.</param>
		public static string GetMessage(this SocketException exception)
		{
			Assert.ArgumentNotNull(exception, nameof(exception));

			var message = exception.Message.Trim();
			if (!message.EndsWith("."))
				message += ".";

			return message;
		}

		/// <summary>
		///   Initializes the socket to support multicasting.
		/// </summary>
		/// <param name="socket">The socket that should be initialized.</param>
		/// <param name="multicastGroup">The IP address representing the multicast group.</param>
		/// <param name="port">The port the socket should be bound to or 0 if the port is irrelevant.</param>
		public static void InitializeMulticasting(this Socket socket, IPAddress multicastGroup, ushort port)
		{
			Assert.ArgumentNotNull(socket, nameof(socket));
			Assert.ArgumentNotNull(multicastGroup, nameof(multicastGroup));

			socket.Bind(new IPEndPoint(IPAddress.IPv6Any, port));
			socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.AddMembership, new IPv6MulticastOption(multicastGroup));
			socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastTimeToLive, 255);
		}
	}
}