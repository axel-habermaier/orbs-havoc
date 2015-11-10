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
	using static OpenGL3;
	using static GraphicsHelpers;

	/// <summary>
	///   Represents a vertex, index, or uniform buffer whose contents are completely static or at least don't change every frame.
	/// </summary>
	public sealed unsafe class StaticBuffer : DisposableObject
	{
		private readonly int _type;
		private readonly int _buffer;

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		public StaticBuffer(int bufferType, int sizeInBytes, void* data)
		{
			_buffer = Allocate(glGenBuffers, nameof(StaticBuffer));
			SizeInBytes = sizeInBytes;
			_type = bufferType;

			glBindBuffer(_type, _buffer);
			glBufferData(_type, (void*)SizeInBytes, data, GL_STATIC_DRAW);
			CheckErrors();
		}

		/// <summary>
		///   Gets the size of the buffer in bytes.
		/// </summary>
		public int SizeInBytes { get; }

		/// <summary>
		///   Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(StaticBuffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		/// <summary>
		///   Copies the given data to the buffer, overwriting all previous data.
		/// </summary>
		/// <param name="data">The data that should be copied.</param>
		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			glBindBuffer(_type, _buffer);
			glBufferSubData(_type, (void*)0, (void*)SizeInBytes, data);
			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Deallocate(glDeleteBuffers, _buffer);
		}
	}
}