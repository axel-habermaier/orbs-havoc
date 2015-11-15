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

namespace OrbsHavoc.Platform.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using Utilities;

	/// <summary>
	///   Represents a pointer to a byte buffer.
	/// </summary>
	public unsafe struct BufferPointer : IDisposable
	{
		/// <summary>
		///   The handle of the pinned byte array.
		/// </summary>
		private GCHandle _handle;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="buffer">The buffer the pointer should point to.</param>
		public BufferPointer(byte[] buffer)
			: this(buffer, 0, buffer.Length)
		{
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="buffer">The buffer the pointer should point to.</param>
		/// <param name="offset">The offset to the first byte the pointer should point to.</param>
		/// <param name="size">The size in bytes of the buffer.</param>
		public BufferPointer(byte[] buffer, int offset, int size)
			: this()
		{
			Assert.ArgumentNotNull(buffer, nameof(buffer));
			Assert.ArgumentInRange(offset, 0, buffer.Length - 1, nameof(offset));
			Assert.ArgumentSatisfies(size <= buffer.Length, nameof(size), "Invalid length.");

			_handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
			Pointer = (byte*)_handle.AddrOfPinnedObject() + offset;
			Size = size;
		}

		/// <summary>
		///   Gets the pointer to the beginning of the buffer.
		/// </summary>
		public byte* Pointer { get; }

		/// <summary>
		///   Converts the pointer to its underlying pointer value.
		/// </summary>
		public static implicit operator void*(BufferPointer pointer)
		{
			return pointer.Pointer;
		}

		/// <summary>
		///   Gets a value indicating whether the buffer pointer has been initialized.
		/// </summary>
		public bool IsInitialized => Pointer != null;

		/// <summary>
		///   Gets the size of the buffer in bytes.
		/// </summary>
		public int Size { get; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (IsInitialized)
				_handle.Free();
		}
	}
}