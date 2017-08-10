namespace OrbsHavoc.Platform.Memory
{
	using System;

	/// <summary>
	///   Raised when an attempt was made to read or write past the end of a buffer.
	/// </summary>
	internal class BufferOverflowException : Exception
	{
	}
}