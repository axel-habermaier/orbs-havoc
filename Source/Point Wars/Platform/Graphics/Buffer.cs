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
	using Logging;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a vertex, index, or uniform buffer.
	/// </summary>
	public sealed unsafe class Buffer : GraphicsObject
	{
		private readonly int _type;
		private bool _isMapped;

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		public Buffer(int bufferType, int usage, int sizeInBytes, void* data)
		{
			Handle = Allocate(glGenBuffers, nameof(Buffer));
			SizeInBytes = sizeInBytes;
			_type = bufferType;

			glBindBuffer(_type, Handle);
			glBufferData(_type, (void*)SizeInBytes, data, usage);
			CheckErrors();
		}

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		public Buffer(int bufferType, int sizeInBytes)
		{
			Handle = Allocate(glGenBuffers, nameof(Buffer));
			SizeInBytes = sizeInBytes;
			_type = bufferType;
			
			glBindBuffer(_type, Handle);
			glBufferStorage(_type, (void*)SizeInBytes, null, GL_MAP_WRITE_BIT | GL_MAP_COHERENT_BIT | GL_MAP_PERSISTENT_BIT);
			CheckErrors();
		}

		/// <summary>
		///   Gets the size of the buffer in bytes.
		/// </summary>
		public int SizeInBytes { get; }

		/// <summary>
		///   Maps the buffer and returns a pointer that the CPU can access. The operations that are allowed on the
		///   returned pointer depend on the given map mode.
		/// </summary>
		/// <param name="mapMode">Indicates which CPU operations are allowed on the buffer memory.</param>
		public BufferData Map(int mapMode)
		{
			Assert.NotDisposed(this);
			Assert.That(!_isMapped, "Buffer is already mapped.");

			glBindBuffer(_type, Handle);
			var mappedBuffer = glMapBuffer(_type, mapMode);
			CheckErrors();

			if (mappedBuffer == null)
				Log.Die("Failed to map buffer.");

			_isMapped = true;
			return new BufferData(this, mappedBuffer);
		}

		/// <summary>
		///   Maps the buffer and returns a pointer that the CPU can access. The operations that are allowed on the
		///   returned pointer depend on the given map mode.
		/// </summary>
		/// <param name="mapMode">Indicates which CPU operations are allowed on the buffer memory.</param>
		/// <param name="offsetInBytes">A zero-based index denoting the first byte of the buffer that should be mapped.</param>
		/// <param name="byteCount">The number of bytes that should be mapped.</param>
		public BufferData MapRange(int mapMode, int offsetInBytes, int byteCount)
		{
			Assert.NotDisposed(this);
			Assert.That(offsetInBytes < SizeInBytes, "Invalid offset.");
			Assert.InRange(byteCount, 0, SizeInBytes);
			Assert.That(offsetInBytes + byteCount <= SizeInBytes, "Buffer overflow.");
			Assert.That(!_isMapped, "Buffer is already mapped.");

			glBindBuffer(_type, Handle);
			var mappedBuffer = glMapBufferRange(_type, (void*)offsetInBytes, (void*)byteCount, mapMode);
			CheckErrors();

			if (mappedBuffer == null)
				Log.Die("Failed to map buffer.");

			_isMapped = true;
			return new BufferData(this, mappedBuffer);
		}

		/// <summary>
		///   Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			Assert.NotDisposed(this);
			Assert.That(_isMapped, "Buffer is not mapped.");

			glBindBuffer(_type, Handle);
			if (!glUnmapBuffer(_type))
				Log.Die("Failed to unmap buffer.");

			CheckErrors();
			_isMapped = false;
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.ConstantBufferSlotCount);

			if (Change(State.ConstantBuffers, slot, this))
				glBindBufferBase(GL_UNIFORM_BUFFER, slot, Handle);

			CheckErrors();
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot, int offset, int sizeInBytes)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.ConstantBufferSlotCount);

			// Do not track state in this case, as we're very unlikely to bind the same uniform buffer
			// with the same offset and sizes twice
			Change(State.ConstantBuffers, slot, null);
			glBindBufferRange(GL_UNIFORM_BUFFER, slot, Handle, (void*)offset, (void*)sizeInBytes);

			CheckErrors();
		}

		/// <summary>
		///   Copies the given data to the buffer, overwriting all previous data.
		/// </summary>
		/// <param name="data">The data that should be copied.</param>
		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			glBindBuffer(_type, Handle);
			glBufferSubData(_type, (void*)0, (void*)SizeInBytes, data);
			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Deallocate(glDeleteBuffers, Handle);
		}
	}
}