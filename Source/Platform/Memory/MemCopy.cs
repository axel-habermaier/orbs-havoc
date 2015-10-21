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

namespace PointWars.Platform.Memory
{
	using System;
	using Utilities;

	/// <summary>
	///   Allows copying of unmanaged memory.
	/// </summary>
	internal static class MemCopy
	{
		/// <summary>
		///   Copies given number of bytes from the source to the destination.
		/// </summary>
		/// <param name="destination">The address of the first byte that should be written.</param>
		/// <param name="source">The address of the first byte that should be read.</param>
		/// <param name="byteCount">The number of bytes that should be copied.</param>
		internal static unsafe void Copy(IntPtr destination, IntPtr source, int byteCount)
		{
			Copy(destination.ToPointer(), source.ToPointer(), byteCount);
		}

		/// <summary>
		///   Copies given number of bytes from the source to the destination.
		/// </summary>
		/// <param name="destination">The address of the first byte that should be written.</param>
		/// <param name="source">The address of the first byte that should be read.</param>
		/// <param name="byteCount">The number of bytes that should be copied.</param>
		internal static unsafe void Copy(void* destination, void* source, int byteCount)
		{
			Assert.ArgumentNotNull(new IntPtr(destination), nameof(destination));
			Assert.ArgumentNotNull(new IntPtr(source), nameof(source));
			Assert.ArgumentSatisfies(byteCount > 0, nameof(byteCount), "At least 1 byte must be copied.");
			Assert.ArgumentSatisfies((source < destination && (byte*)source + byteCount <= destination) ||
									 (destination < source && (byte*)destination + byteCount <= source),
				nameof(source), "The memory regions overlap.");

			CopyBlock.Copy(destination, source, byteCount);
		}
	}
}