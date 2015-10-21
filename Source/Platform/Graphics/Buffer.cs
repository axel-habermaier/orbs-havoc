﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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
		private bool _isMapped;
		private readonly uint _sizeInBytes;
		private readonly uint _type;

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		public Buffer(uint bufferType, uint usage, uint sizeInBytes, void* data)
		{
			Handle = Allocate(glGenBuffers, "Buffer");
			_sizeInBytes = sizeInBytes;
			_type = bufferType;

			glBindBuffer(_type, Handle);
			glBufferData(_type, (int)_sizeInBytes, data, usage);
		}

		/// <summary>
		///   Maps the buffer and returns a pointer that the CPU can access. The operations that are allowed on the
		///   returned pointer depend on the given map mode.
		/// </summary>
		/// <param name="mapMode">Indicates which CPU operations are allowed on the buffer memory.</param>
		public void* Map(uint mapMode)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentInRange(mapMode, nameof(mapMode));
			Assert.That(!_isMapped, "Buffer is already mapped.");

			glBindBuffer(_type, Handle);
			var mappedBuffer = glMapBuffer(_type, mapMode);

			if (mappedBuffer == null)
				Log.Die("Failed to map buffer.");

			_isMapped = true;
			return mappedBuffer;
		}

		/// <summary>
		///   Maps the buffer and returns a pointer that the CPU can access. The operations that are allowed on the
		///   returned pointer depend on the given map mode.
		/// </summary>
		/// <param name="mapMode">Indicates which CPU operations are allowed on the buffer memory.</param>
		/// <param name="offsetInBytes">A zero-based index denoting the first byte of the buffer that should be mapped.</param>
		/// <param name="byteCount">The number of bytes that should be mapped.</param>
		public void* MapRange(uint mapMode, uint offsetInBytes, uint byteCount)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentInRange(mapMode, nameof(mapMode));
			Assert.That(offsetInBytes < _sizeInBytes, "Invalid offset.");
			Assert.InRange(byteCount, 1u, _sizeInBytes - 1);
			Assert.That(offsetInBytes + byteCount <= _sizeInBytes, "Buffer overflow.");
			Assert.That(!_isMapped, "Buffer is already mapped.");

			glBindBuffer(_type, Handle);
			var mappedBuffer = glMapBufferRange(_type, (void*)offsetInBytes, (int)byteCount, mapMode);

			if (mappedBuffer == null)
				Log.Die("Failed to map buffer.");

			_isMapped = true;
			return mappedBuffer;
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

			_isMapped = false;
		}

		/// <summary>
		///   Binds the constant buffer to the given slot without uploading any possible changes of the buffer to the GPU.
		/// </summary>
		public void Bind(uint slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0u, GraphicsState.ConstantBufferSlotCount);

			if (Change(State.ConstantBuffers, (int)slot, this))
				glBindBufferBase(GL_UNIFORM_BUFFER, slot, Handle);
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
			glBufferSubData(_type, (void*)0, (int)_sizeInBytes, data);
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