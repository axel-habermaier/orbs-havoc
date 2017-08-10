namespace OrbsHavoc.Network
{
	using System;

	/// <summary>
	///   Raised when a server has reached the maximum number of clients that can connect concurrently.
	/// </summary>
	internal class ServerFullException : Exception
	{
	}
}