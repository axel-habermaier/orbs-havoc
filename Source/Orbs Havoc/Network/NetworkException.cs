namespace OrbsHavoc.Network
{
	using System;

	/// <summary>
	///   Raised when a network error occurred.
	/// </summary>
	public class NetworkException : Exception
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="message">A message explaining the exception.</param>
		public NetworkException(string message)
			: base(message)
		{
		}
	}
}