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

namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using Utilities;
	using static GraphicsHelpers;
	using static OpenGL3;

	/// <summary>
	///   Represents a vertex or index buffer accessible by the GPU.
	/// </summary>
	public sealed unsafe class Buffer : DisposableObject
	{
		private readonly int _buffer;
		private readonly int _type;

		/// <summary>
		///   Indicates whether the buffer is currently mapped.
		/// </summary>
		private bool _isMapped;

		/// <summary>
		///   The GPU frame number when the buffer was last changed.
		/// </summary>
		private uint _lastChanged;

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		/// <param name="bufferType">The OpenGL type of the buffer.</param>
		/// <param name="usage">The OpenGL buffer usage flag.</param>
		/// <param name="sizeInBytes">The size of the buffer in bytes.</param>
		/// <param name="data">The data that should be copied into the buffer, or null if no data should be copied.</param>
		public Buffer(int bufferType, int usage, int sizeInBytes, void* data = null)
		{
			_buffer = Allocate(glGenBuffers, nameof(Buffer));
			_type = bufferType;
			SizeInBytes = sizeInBytes;

			glBindBuffer(_type, _buffer);
			glBufferData(_type, (void*)SizeInBytes, data, usage);
			CheckErrors();
		}

		/// <summary>
		///   Gets the size of the buffer in bytes.
		/// </summary>
		public int SizeInBytes { get; }

		/// <summary>
		///   Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(Buffer obj)
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
		///   Gets a pointer to the first byte of the buffer.
		/// </summary>
		/// <param name="sizeInBytes">The number of bytes that can be written.</param>
		public void* Map(int sizeInBytes)
		{
			Assert.That(!_isMapped, "The buffer has already been mapped.");
			Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");

			_isMapped = true;
			_lastChanged = State.FrameNumber;

			glBindBuffer(_type, _buffer);
			var pointer = glMapBufferRange(_type, (void*)0, (void*)sizeInBytes,
				GL_MAP_UNSYNCHRONIZED_BIT | GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT);

			CheckErrors();
			return pointer;
		}

		/// <summary>
		///   Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			if (!_isMapped)
				return;

			glBindBuffer(_type, _buffer);
			glUnmapBuffer(_type);
			CheckErrors();

			_isMapped = false;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unmap();
			Deallocate(glDeleteBuffers, _buffer);
		}
	}
}