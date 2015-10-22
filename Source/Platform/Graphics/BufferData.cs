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

namespace PointWars.Platform.Graphics
{
	using System;
	using Memory;
	using Utilities;

	/// <summary>
	///   Provides access to the data of a mapped buffer.
	/// </summary>
	public unsafe struct BufferData : IDisposable
	{
		/// <summary>
		///   The buffer that is mapped.
		/// </summary>
		private readonly Buffer _buffer;

		/// <summary>
		///   The pointer to the mapped buffer data.
		/// </summary>
		private void* _pointer;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="buffer">The buffer that should be mapped.</param>
		/// <param name="pointer">The pointer to the mapped buffer data.</param>
		internal BufferData(Buffer buffer, void* pointer)
			: this()
		{
			Assert.ArgumentNotNull(buffer, nameof(buffer));
			Assert.ArgumentNotNull(new IntPtr(pointer), nameof(pointer));

			_buffer = buffer;
			_pointer = pointer;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Assert.NotNull(_buffer);

			_buffer.Unmap();
			_pointer = null;
		}

		/// <summary>
		///   Writes the given data to the mapped buffer.
		/// </summary>
		/// <param name="data">The data that should be written.</param>
		/// <param name="offsetInBytes">The offset into the buffer where the new data should be written to.</param>
		/// <param name="sizeInBytes">The size of the data that should be written in bytes.</param>
		public void Write(IntPtr data, int offsetInBytes, int sizeInBytes)
		{
			Write((void*)data, offsetInBytes, sizeInBytes);
		}

		/// <summary>
		///   Writes the given data to the mapped buffer.
		/// </summary>
		/// <param name="data">The data that should be written.</param>
		/// <param name="offsetInBytes">The offset into the buffer where the new data should be written to.</param>
		/// <param name="sizeInBytes">The size of the data that should be written in bytes.</param>
		public void Write(void* data, int offsetInBytes, int sizeInBytes)
		{
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));
			Assert.ArgumentSatisfies(sizeInBytes >= 0, nameof(sizeInBytes), "Invalid size.");
			Assert.ArgumentSatisfies(offsetInBytes >= 0, nameof(offsetInBytes), "Invalid size.");
			Assert.ArgumentSatisfies(sizeInBytes <= _buffer.SizeInBytes - offsetInBytes, nameof(sizeInBytes),
				"Attempted to write outside the bounds of the buffer.");

			MemCopy.Copy((byte*)_pointer + offsetInBytes, data, sizeInBytes);
		}
	}
}