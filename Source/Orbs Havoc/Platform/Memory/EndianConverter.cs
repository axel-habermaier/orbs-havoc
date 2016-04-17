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

namespace OrbsHavoc.Platform.Memory
{
	using System;

	/// <summary>
	///   Converts between little and big endian encoding.
	/// </summary>
	public static class EndianConverter
	{
		/// <summary>
		///   Gets a value indicating whether an endianess conversion is required for multi-byte values.
		/// </summary>
		/// <param name="endianess">The endianess of the data that must potentially be converted.</param>
		public static bool RequiresConversion(Endianess endianess) => BitConverter.IsLittleEndian && endianess != Endianess.Little;

		/// <summary>
		///   Converts an 8 byte signed integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static long Convert(long value)
		{
			return ((long)(byte)(value)) << 56 |
				   ((long)(byte)(value >> 8)) << 48 |
				   ((long)(byte)(value >> 16)) << 40 |
				   ((long)(byte)(value >> 24)) << 32 |
				   ((long)(byte)(value >> 32)) << 24 |
				   ((long)(byte)(value >> 40)) << 16 |
				   ((long)(byte)(value >> 48)) << 8 |
				   (byte)(value >> 56);
		}

		/// <summary>
		///   Converts an 8 byte unsigned integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static ulong Convert(ulong value)
		{
			return ((ulong)((byte)(value)) << 56 |
					((ulong)(byte)(value >> 8)) << 48 |
					((ulong)(byte)(value >> 16)) << 40 |
					((ulong)(byte)(value >> 24)) << 32 |
					((ulong)(byte)(value >> 32)) << 24 |
					((ulong)(byte)(value >> 40)) << 16 |
					((ulong)(byte)(value >> 48)) << 8 |
					(byte)(value >> 56));
		}

		/// <summary>
		///   Converts a 4 byte signed integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static int Convert(int value)
		{
			return ((byte)(value)) << 24 |
				   ((byte)(value >> 8)) << 16 |
				   ((byte)(value >> 16)) << 8 |
				   ((byte)(value >> 24));
		}

		/// <summary>
		///   Converts a 4 byte unsigned integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static uint Convert(uint value)
		{
			return (uint)(((byte)(value)) << 24 |
						  ((byte)(value >> 8)) << 16 |
						  ((byte)(value >> 16)) << 8 |
						  ((byte)(value >> 24)));
		}

		/// <summary>
		///   Converts a 2 byte signed integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static short Convert(short value)
		{
			return (short)(((byte)(value)) << 8 | ((byte)(value >> 8)));
		}

		/// <summary>
		///   Converts a 2 byte unsigned integer.
		/// </summary>
		/// <param name="value">The value that should be converted.</param>
		public static ushort Convert(ushort value)
		{
			return (ushort)(((byte)(value)) << 8 | ((byte)(value >> 8)));
		}
	}
}