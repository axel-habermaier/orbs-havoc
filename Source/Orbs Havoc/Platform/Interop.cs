// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Platform
{
	using System;
	using System.Text;
	using Memory;
	using Utilities;

	/// <summary>
	///   Provides methods that facilitate native code interop.
	/// </summary>
	internal static unsafe class Interop
	{
		/// <summary>
		///   Converts the given native string to a .NET string.
		/// </summary>
		/// <param name="str">The string that should be converted.</param>
		public static string ToString(byte* str)
		{
			var length = 0;
			while (str[length] != '\0')
				++length;

			return Encoding.UTF8.GetString(str, length);
		}

		/// <summary>
		///   Gets a pinned pointer to the string.
		/// </summary>
		/// <param name="str">The string the pointer should be created for.</param>
		public static PinnedPointer ToPointer(string str)
		{
			return PinnedPointer.Create(Encoding.UTF8.GetBytes(str));
		}

		/// <summary>
		///   Copies given number of bytes from the source to the destination.
		/// </summary>
		/// <param name="destination">The address of the first byte that should be written.</param>
		/// <param name="source">The address of the first byte that should be read.</param>
		/// <param name="byteCount">The number of bytes that should be copied.</param>
		internal static void Copy(void* destination, void* source, int byteCount)
		{
			Assert.ArgumentNotNull(new IntPtr(destination), nameof(destination));
			Assert.ArgumentNotNull(new IntPtr(source), nameof(source));
			Assert.ArgumentSatisfies(byteCount > 0, nameof(byteCount), "At least 1 byte must be copied.");
			Assert.ArgumentSatisfies(
				(source < destination && (byte*)source + byteCount <= destination) ||
				(destination < source && (byte*)destination + byteCount <= source),
				nameof(source), "The memory regions overlap.");

			MemCopy.Copy(destination, source, byteCount);
		}
	}
}