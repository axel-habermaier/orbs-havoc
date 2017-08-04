namespace OrbsHavoc.Platform.Memory
{
	using System;

	/// <summary>
	///   Raised when an attempt was made to read or write past the end of a buffer.
	/// </summary>
	public class BufferOverflowException : Exception
	{
	}
}