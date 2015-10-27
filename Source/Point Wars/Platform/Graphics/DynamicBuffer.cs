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
	using Memory;
	using Utilities;
	using static OpenGL3;

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
		private const int ChunkCount = 3;

		/// <summary>
		///   The number of elements stored in each chunk.
		/// </summary>
		private readonly int _elementCount;

		/// <summary>
		///   The persistently mapped pointer to the OpenGL buffer contents.
		/// </summary>
		private readonly void* _pointer;

		/// <summary>
		///   The index of the current chunk that the dynamic vertex buffer uses for mapping and drawing operations.
		/// </summary>
		private int _currentChunk;

		/// <summary>
		///   The size of a single element in bytes.
		/// </summary>
		private readonly int _elementSize;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="bufferType">The type of the buffer.</param>
		/// <param name="elementCount">The number of elements that the buffer should be able to store.</param>
		/// <param name="elementSize">The size of each element in the buffer.</param>
		public DynamicBuffer(int bufferType, int elementCount, int elementSize)
		{
			var bufferSize = ChunkCount * elementCount * elementSize;
			Buffer = new Buffer(bufferType, bufferSize);

			_elementCount = elementCount;
			_elementSize = elementSize;
			_pointer = Buffer.MapRange(MapMode, 0, bufferSize);
		}

		/// <summary>
		///   Gets the underlying vertex buffer. The returned buffer should only be used to initialize vertex layouts;
		///   mapping the buffer will most likely result in some unexpected behavior.
		/// </summary>
		public Buffer Buffer { get; }

		/// <summary>
		///   Gets the vertex offset that must be applied to a drawing operation when the data of the last buffer mapping operation
		///   should be drawn.
		/// </summary>
		public int VertexOffset => _currentChunk * _elementCount;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Buffer.Unmap();
			Buffer.SafeDispose();
		}

		/// <summary>
		///   Maps the next chunk of the buffer and returns a pointer to the first byte of the chunk.
		/// </summary>
		/// <param name="offset">A zero-based index denoting the first byte of the next chunk that should be mapped.</param>
		public void* Map(int offset = 0)
		{
			Assert.ArgumentSatisfies(offset < _elementSize * _elementCount, nameof(offset), "Offset is out-of-bounds.");

			_currentChunk = (_currentChunk + 1) % ChunkCount;
			return (byte*)_pointer + _currentChunk * _elementSize * _elementCount + offset;
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot)
		{
			Bind(slot, 0, _elementSize * _elementCount);
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot, int elementOffset, int elementCount)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.ConstantBufferSlotCount);
			Assert.InRange(elementOffset, 0, _elementCount);
			Assert.InRange(elementCount, 0, _elementCount);

			Buffer.Bind(slot, _currentChunk * _elementSize * _elementCount + elementOffset * _elementSize, elementCount * _elementSize);
		}
	}
}