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

namespace OrbsHavoc.Rendering
{
	using System;
	using System.Collections.Generic;
	using System.Runtime.InteropServices;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a collection of quads.
	/// </summary>
	public sealed unsafe class QuadCollection : DisposableObject
	{
		/// <summary>
		///   The maximum number of quads that can be drawn.
		/// </summary>
		public const int MaxQuads = UInt16.MaxValue;

		private readonly List<QuadPartition> _partitions = new List<QuadPartition>();
		private readonly ObjectPool<QuadPartition> _pooledPartitions = new ObjectPool<QuadPartition>();
		private readonly Quad* _quads;
		private readonly int _sizeInBytes = sizeof(Quad) * MaxQuads;
		private int _count;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public QuadCollection()
		{
			_quads = (Quad*)Marshal.AllocHGlobal(_sizeInBytes).ToPointer();
			GC.AddMemoryPressure(_sizeInBytes);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Marshal.FreeHGlobal(new IntPtr(_quads));
			GC.RemoveMemoryPressure(_sizeInBytes);

			_partitions.SafeDisposeAll();
			_pooledPartitions.SafeDispose();
		}

		/// <summary>
		///   Clears the collection.
		/// </summary>
		public void Clear()
		{
			_count = 0;
			_partitions.SafeDisposeAll();
			_partitions.Clear();
		}

		/// <summary>
		///   Adds the given number of quads to the collection and returns a pointer to the memory location of the first added quad
		///   where the quad information should be written to. The written number of quads must match the specified count exactly,
		///   otherwise undefined behavior occurs.
		/// </summary>
		/// <param name="partition">The partition the quads should be added to.</param>
		/// <param name="count">The number of quads that should be added.</param>
		public Quad* AddQuads(QuadPartition partition, int count)
		{
			Assert.ArgumentNotNull(partition, nameof(partition));
			Assert.ArgumentSatisfies(_partitions.Contains(partition), nameof(partition), "Unknown partition.");
			Assert.ArgumentSatisfies(count > 0, nameof(count), "At least one quad must be added.");

			CheckQuadCount(count);
			partition.AddQuads(offset: _count, count: count);

			var quads = &_quads[_count];
			_count += count;
			return quads;
		}

		/// <summary>
		///   Allocates a new quad partition with the given render state.
		/// </summary>
		/// <param name="renderState">The render state of the new partition.</param>
		public QuadPartition AllocatePartition(ref RenderState renderState)
		{
			var partition = _pooledPartitions.Allocate();
			partition.RenderState = renderState;

			_partitions.Add(partition);
			return partition;
		}

		/// <summary>
		///   Uploads the quad collection to the buffer in a way that allows efficient rendering with minimal GPU state changes.
		/// </summary>
		/// <param name="buffer">The buffer the quads should be uploaded to.</param>
		internal void UploadToGpu(RenderBuffer buffer)
		{
			var gpuQuads = buffer.GetPointer();
			var offset = 0;

			foreach (var partition in _partitions)
				partition.UploadQuads(_quads, gpuQuads, ref offset);
		}

		/// <summary>
		///   Check that adding the given number of quads does not overflow the buffer.
		/// </summary>
		/// <param name="quadCount">The additional number of quads that should be drawn.</param>
		private void CheckQuadCount(int quadCount)
		{
			Assert.ArgumentInRange(quadCount, 0, MaxQuads, nameof(quadCount));

			// Check whether we would overflow if we added the given batch.
			var tooManyQuads = _count + quadCount >= MaxQuads;
			if (tooManyQuads)
				Log.Die("Out of memory while trying to {0} add additional quads.", quadCount);
		}
	}
}