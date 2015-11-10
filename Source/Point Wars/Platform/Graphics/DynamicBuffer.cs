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
	using Memory;
	using Utilities;
	using static OpenGL3;
	using static GraphicsHelpers;

	/// <summary>
	///   Represents a dynamic buffer that is split into several chunks. Each chunk is of the requested size; therefore,
	///   the amount of memory allocated on the GPU is the product of the chunk size and the number of chunks. The chunks are
	///   updated in a round-robin fashion, updating only those parts of the vertex buffer that the GPU is currently not using.
	///   This frees the GPU driver from copying the buffer, which sometimes causes noticeable hiccups.
	///   The data of the dynamic buffer can only be safely modified once per frame.
	/// </summary>
	public unsafe class DynamicBuffer : DisposableObject
	{
		private const int MapMode = GL_MAP_WRITE_BIT | GL_MAP_PERSISTENT_BIT | GL_MAP_COHERENT_BIT;
		private const int ChunkCount = GraphicsState.MaxFrameLag;

		/// <summary>
		///   The persistently mapped pointer to the OpenGL buffer contents.
		/// </summary>
		private readonly void* _pointer;

		/// <summary>
		///   The size in bytes of each element, disregarding possible alignment requirements.
		/// </summary>
		private readonly int _sizeInBytes;

		/// <summary>
		///   The OpenGL type of the buffer.
		/// </summary>
		private readonly int _type;

		/// <summary>
		///   The underlying OpenGL handle of the buffer.
		/// </summary>
		private readonly int _buffer;

		/// <summary>
		///   The index of the current chunk that the dynamic vertex buffer uses for mapping and drawing operations.
		/// </summary>
		private int _currentChunk;

		/// <summary>
		///   The GPU frame number when the buffer was last changed.
		/// </summary>
		private uint _lastChanged;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="bufferType">The type of the buffer.</param>
		/// <param name="elementCount">The number of elements that the buffer should be able to store.</param>
		/// <param name="elementSize">The size of each element in the buffer.</param>
		public DynamicBuffer(int bufferType, int elementCount, int elementSize)
		{
			Assert.That(bufferType != GL_UNIFORM_BUFFER || elementCount == 1, "Uniform buffers can only have a single element.");

			_buffer = Allocate(glGenBuffers, nameof(DynamicBuffer));
			_sizeInBytes = elementSize;
			_type = bufferType;

			// For uniform buffers, we have to respect the OpenGL uniform buffer alignment requirements
			if (bufferType == GL_UNIFORM_BUFFER)
			{
				int alignment;
				glGetIntegerv(GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT, &alignment);
				CheckErrors();

				var remainder = elementSize % alignment;
				if (remainder != 0)
					elementSize += alignment - remainder;
			}

			var size = ChunkCount * elementCount * elementSize;
			ElementCount = elementCount;
			ElementSize = elementSize;

			glBindBuffer(bufferType, _buffer);
			glBufferStorage(bufferType, (void*)size, null, GL_MAP_WRITE_BIT | GL_MAP_COHERENT_BIT | GL_MAP_PERSISTENT_BIT);
			CheckErrors();

			_pointer = glMapBufferRange(_type, (void*)0, (void*)size, MapMode);
			CheckErrors();

			if (_pointer == null)
				Log.Die("Failed to map buffer.");
		}

		/// <summary>
		///   Gets the number of elements stored in each chunk.
		/// </summary>
		public int ElementCount { get; }

		/// <summary>
		///   Gets the size of a single element in bytes.
		/// </summary>
		public int ElementSize { get; }

		/// <summary>
		///   Gets the element offset that must be applied to all drawing operations.
		/// </summary>
		public int ElementOffset => _currentChunk * ElementCount;

		/// <summary>
		///   Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(DynamicBuffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			glBindBuffer(_type, _buffer);
			if (!glUnmapBuffer(_type))
				Log.Error("Failed to unmap buffer.");

			CheckErrors();

			Deallocate(glDeleteBuffers, _buffer);
			Unset(State.ConstantBuffers, this);
		}

		/// <summary>
		///   Gets a pointer to the first byte of the next chunk of the buffer that should be used by the current frame.
		/// </summary>
		/// <param name="offset">A zero-based index denoting the first byte of the next chunk that should be used.</param>
		public void* GetChunkPointer(int offset = 0)
		{
			Assert.ArgumentSatisfies(offset < ElementSize * ElementCount, nameof(offset), "Offset is out-of-bounds.");
			Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");

			_lastChanged = State.FrameNumber;
			//_currentChunk = (_currentChunk + 1) % ChunkCount;
			return (byte*)_pointer + _currentChunk * ElementSize * ElementCount + offset;
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.ConstantBufferSlotCount);
			Assert.InRange(slot, 0, GraphicsState.ConstantBufferSlotCount);

			if (!Change(State.ConstantBuffers, slot, this))
				return;

			var offset = ElementOffset * ElementSize;
			var size = ElementCount * ElementSize;
			glBindBufferRange(GL_UNIFORM_BUFFER, slot, _buffer, (void*)offset, (void*)size);

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

			MemCopy.Copy(GetChunkPointer(), data, _sizeInBytes);
		}
	}
}