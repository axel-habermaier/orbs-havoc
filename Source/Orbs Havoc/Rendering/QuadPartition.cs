// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using Platform;
	using Platform.Memory;

	/// <summary>
	///     Represents a partition of a quad collection consisting of quads that use the same render states.
	/// </summary>
	public sealed class QuadPartition : PooledObject
	{
		/// <summary>
		///     The subsections of quads within the quad collection the partition consists of.
		/// </summary>
		private readonly List<(int Offset, int Length)> _sections = new List<(int Offset, int Length)>();

		/// <summary>
		///     Gets the number of quads contained in the partition.
		/// </summary>
		public int Count { get; private set; }

		/// <summary>
		///     Gets the zero-based index of the partition's first quad in the render buffer.
		/// </summary>
		public int Offset { get; private set; }

		/// <summary>
		///     Gets or sets the render state used by the quads.
		/// </summary>
		public RenderState RenderState { get; set; }

		/// <summary>
		///     Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected override void OnReturning()
		{
			_sections.Clear();

			Count = 0;
			Offset = Int32.MinValue;
		}

		/// <summary>
		///     Adds quads to the partition.
		/// </summary>
		/// <param name="offset">The zero-based index of the first quad that should be added.</param>
		/// <param name="count">The number of quads that should be added.</param>
		public void AddQuads(int offset, int count)
		{
			// See if we can append to the last section, which would be most efficient; otherwise, add a new section
			var lastSection = _sections.Count != 0 ? _sections[_sections.Count - 1] : (Offset:0, Length: 0);
			if (_sections.Count != 0 && lastSection.Offset + lastSection.Length == offset)
				_sections[_sections.Count - 1] = ( lastSection.Offset, lastSection.Length + count);
			else
				_sections.Add((offset, count));

			Count += count;
		}

		/// <summary>
		///     Uploads the quad partition to the buffer contiguously.
		/// </summary>
		/// <param name="cpuBuffer">The buffer the quads should be copied from.</param>
		/// <param name="gpuBuffer">The buffer the quads should be uploaded to.</param>
		/// <param name="gpuOffset">The zero-based index to the partition's first quad in the GPU buffer.</param>
		internal unsafe void UploadQuads(Quad* cpuBuffer, Quad* gpuBuffer, ref int gpuOffset)
		{
			Offset = gpuOffset;

			foreach (var section in _sections)
			{
				Interop.Copy(gpuBuffer + gpuOffset, cpuBuffer + section.Offset, section.Length * Quad.SizeInBytes);
				gpuOffset += section.Length;
			}
		}
	}
}