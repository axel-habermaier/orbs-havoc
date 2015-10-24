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
	using System.Runtime.InteropServices;
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
	public class DynamicBuffer : DisposableObject
	{
		private const int MapMode = GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT | GL_MAP_UNSYNCHRONIZED_BIT;
		private const int ChunkCount = 3;

		/// <summary>
		///   The size of a single chunk in bytes.
		/// </summary>
		private int _chunkSize;

		/// <summary>
		///   The index of the current chunk that the dynamic vertex buffer uses for mapping and drawing operations.
		/// </summary>
		private int _currentChunk;

		/// <summary>
		///   The number of elements stored in each chunk.
		/// </summary>
		private int _elementCount;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private DynamicBuffer()
		{
		}

		/// <summary>
		///   Gets the underlying vertex buffer. The returned buffer should only be used to initialize vertex layouts;
		///   mapping the buffer will most likely result in some unexpected behavior.
		/// </summary>
		public Buffer Buffer { get; private set; }

		/// <summary>
		///   Gets the vertex offset that must be applied to a drawing operation when the data of the last buffer mapping operation
		///   should be drawn.
		/// </summary>
		public int VertexOffset => _currentChunk * _elementCount;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <typeparam name="T">The type of the data stored in the vertex buffer.</typeparam>
		/// <param name="bufferType">The type of the buffer.</param>
		/// <param name="elementCount">The number of elements of type T that the buffer should be able to store.</param>
		public static unsafe DynamicBuffer Create<T>(int bufferType, int elementCount)
			where T : struct
		{
			Assert.ArgumentInRange(elementCount, 1, Int32.MaxValue, nameof(elementCount));

			return new DynamicBuffer
			{
				Buffer = new Buffer(bufferType, GL_DYNAMIC_DRAW, elementCount * ChunkCount * Marshal.SizeOf(typeof(T)), null),
				_elementCount = elementCount,
				_chunkSize = elementCount * Marshal.SizeOf(typeof(T))
			};
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Buffer.SafeDispose();
		}

		/// <summary>
		///   Maps the next chunk of the buffer and returns a pointer to the first byte of the chunk.
		/// </summary>
		public BufferData Map()
		{
			_currentChunk = (_currentChunk + 1) % ChunkCount;
			return Buffer.MapRange(MapMode, _currentChunk * _chunkSize, _chunkSize);
		}

		/// <summary>
		///   Maps the next chunk of the buffer and returns a pointer to the first byte of the chunk.
		/// </summary>
		/// <param name="offset">A zero-based index denoting the first byte of the next chunk that should be mapped.</param>
		/// <param name="byteCount">The number of bytes of the next chunk that should be mapped.</param>
		public BufferData MapRange(int offset, int byteCount)
		{
			Assert.ArgumentSatisfies(offset < _chunkSize, nameof(offset), "Offset is out-of-bounds.");
			Assert.ArgumentSatisfies(byteCount <= _chunkSize - offset, nameof(byteCount), "Size is out-of-bounds.");

			_currentChunk = (_currentChunk + 1) % ChunkCount;
			return Buffer.MapRange(MapMode, _currentChunk * _chunkSize + offset, byteCount);
		}
	}
}