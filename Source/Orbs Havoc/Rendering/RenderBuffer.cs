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
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;
	using static Platform.Graphics.GraphicsHelpers;
	using static Platform.Graphics.OpenGL3;

	/// <summary>
	///   Manages the GPU memory that contains all quad data used for rendering a frame.
	/// </summary>
	internal sealed unsafe class RenderBuffer : DisposableObject
	{
		/// <summary>
		///   The vertex buffers that are used to store the quad data.
		/// </summary>
		private readonly Buffer[] _dataBuffers = new Buffer[GraphicsState.MaxFrameLag];

		/// <summary>
		///   The vertex input layouts that describe the vertex buffers.
		/// </summary>
		private readonly int[] _vertexLayouts = new int[GraphicsState.MaxFrameLag];

		/// <summary>
		///   The current index into the vertex layout and data buffer arrays.
		/// </summary>
		private int _index;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public RenderBuffer()
		{
			Assert.That(sizeof(Quad) == Quad.SizeInBytes, "Unexpected quad size.");

			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				_dataBuffers[i] = new Buffer(GL_ARRAY_BUFFER, GL_DYNAMIC_DRAW, QuadCollection.MaxQuads * Quad.SizeInBytes);
				_vertexLayouts[i] = Allocate(glGenVertexArrays, "Vertex Layout");

				glBindVertexArray(_vertexLayouts[i]);
				CheckErrors();

				glBindBuffer(GL_ARRAY_BUFFER, _dataBuffers[i]);
				CheckErrors();

				var offset = (byte*)0;
				var index = 1;

				InitializeVertexAttribute(ref offset, ref index, 2, GL_FLOAT, sizeof(float), false); // Positions
				InitializeVertexAttribute(ref offset, ref index, 1, GL_FLOAT, sizeof(float), false); // Orientations
				InitializeVertexAttribute(ref offset, ref index, 4, GL_UNSIGNED_BYTE, sizeof(byte), true); // Colors
				InitializeVertexAttribute(ref offset, ref index, 2, GL_UNSIGNED_SHORT, sizeof(ushort), false); // Sizes
				InitializeVertexAttribute(ref offset, ref index, 4, GL_UNSIGNED_SHORT, sizeof(ushort), true); // Tex Coords
			}

			glBindVertexArray(0);
		}

		/// <summary>
		///   Binds the render buffer for rendering.
		/// </summary>
		public void Bind()
		{
			// Do not actually bind the vertex layout here, as that causes all sorts of problems with buffer updates between
			// the binding of the vertex layout and the actual draw call using the vertex layout
			State.VertexLayout = _vertexLayouts[_index];
		}

		/// <summary>
		///   Initializes a vertex attribute.
		/// </summary>
		private static void InitializeVertexAttribute(ref byte* offset, ref int index, int componentCount, int type, int size, bool normalize)
		{
			glEnableVertexAttribArray(index);
			glVertexAttribPointer(index, componentCount, type, normalize, Quad.SizeInBytes, offset);
			CheckErrors();

			index += 1;
			offset += componentCount * size;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_dataBuffers.SafeDisposeAll();

			State.VertexLayout = -1;

			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
				Deallocate(glDeleteVertexArrays, _vertexLayouts[i]);
		}

		/// <summary>
		///   Maps the buffer for data upload to the GPU.
		/// </summary>
		/// <param name="sizeInBytes">The number of bytes that can be written.</param>
		public Quad* Map(int sizeInBytes)
		{
			_index = (_index + 1) % GraphicsState.MaxFrameLag;
			return (Quad*)_dataBuffers[_index].Map(sizeInBytes);
		}

		/// <summary>
		///   Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			_dataBuffers[_index].Unmap();
		}
	}
}