namespace OrbsHavoc.Network
{
	using System.Net;
	using System.Net.Sockets;
	using Utilities;

	/// <summary>
	///   Provides extension methods for network related .NET framework classes.
	/// </summary>
	internal static class NetworkExtensions
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