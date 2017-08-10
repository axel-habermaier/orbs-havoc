namespace OrbsHavoc.Utilities
{
	using System;

	/// <summary>
	///   Represents a fatal error that causes the execution of the application to be aborted.
	/// </summary>
	internal class FatalErrorException : Exception
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="message">A message explaining the fatal error.</param>
		public FatalErrorException(string message)
			: base(message)
		{
		}
	}
}