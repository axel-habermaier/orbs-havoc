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

namespace PointWars.Rendering
{
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;
	using static Platform.Graphics.OpenGL3;
	using static Platform.Graphics.GraphicsHelpers;

	/// <summary>
	///   Manages the GPU memory that contains all quad data used for rendering a frame.
	/// </summary>
	internal sealed unsafe class RenderBuffer : DisposableObject
	{
		/// <summary>
		///   The vertex buffer that is used to store the quad data.
		/// </summary>
		private readonly DynamicBuffer _dataBuffer;

		/// <summary>
		///   The vertex buffer that stores the quad vertices.
		/// </summary>
		private readonly StaticBuffer _vertexBuffer;

		/// <summary>
		///   The vertex input layout that describes the vertex buffer.
		/// </summary>
		private readonly int _vertexLayout;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public RenderBuffer()
		{
			Assert.That(sizeof(Quad) == Quad.SizeInBytes, "Unexpected quad size.");

			var vertices = stackalloc Vector2[4];
			vertices[0] = new Vector2(-0.5f, -0.5f);
			vertices[1] = new Vector2(-0.5f, 0.5f);
			vertices[2] = new Vector2(0.5f, -0.5f);
			vertices[3] = new Vector2(0.5f, 0.5f);

			_vertexBuffer = new StaticBuffer(GL_ARRAY_BUFFER, 4 * sizeof(Vector2), vertices);
			_dataBuffer = new DynamicBuffer(GL_ARRAY_BUFFER, QuadCollection.MaxQuads, Quad.SizeInBytes);
			_vertexLayout = Allocate(glGenVertexArrays, "Vertex Layout");

			glBindVertexArray(_vertexLayout);
			glBindBuffer(GL_ARRAY_BUFFER, _vertexBuffer);
			glEnableVertexAttribArray(0);
			glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(Vector2), (void*)0);
			glVertexAttribDivisor(0, 0);
			CheckErrors();

			glBindBuffer(GL_ARRAY_BUFFER, _dataBuffer);
			CheckErrors();

			var offset = (byte*)0;
			var index = 1;

			InitializeVertexAttribute(ref offset, ref index, 2, GL_FLOAT, sizeof(float), false); // Positions
			InitializeVertexAttribute(ref offset, ref index, 1, GL_FLOAT, sizeof(float), false); // Orientations
			InitializeVertexAttribute(ref offset, ref index, 4, GL_UNSIGNED_BYTE, sizeof(byte), true); // Colors
			InitializeVertexAttribute(ref offset, ref index, 2, GL_UNSIGNED_SHORT, sizeof(ushort), false); // Sizes
			InitializeVertexAttribute(ref offset, ref index, 4, GL_UNSIGNED_SHORT, sizeof(ushort), true); // Tex Coords

			glBindVertexArray(0);
		}

		/// <summary>
		///   Gets the element offset that must be applied to all drawing operations.
		/// </summary>
		public int ElementOffset => _dataBuffer.ElementOffset;

		/// <summary>
		///   Binds the render buffer for rendering.
		/// </summary>
		public void Bind()
		{
			// Do not actually bind the vertex layout here, as that causes all sorts of problems with buffer updates between
			// the binding of the vertex layout and the actual draw call using the vertex layout
			State.VertexLayout = _vertexLayout;
		}

		/// <summary>
		///   Initializes a vertex attribute.
		/// </summary>
		private static void InitializeVertexAttribute(ref byte* offset, ref int index, int componentCount, int type, int size, bool normalize)
		{
			glEnableVertexAttribArray(index);
			glVertexAttribPointer(index, componentCount, type, normalize, Quad.SizeInBytes, offset);
			glVertexAttribDivisor(index, 1);
			CheckErrors();

			index += 1;
			offset += componentCount * size;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_dataBuffer.SafeDispose();
			_vertexBuffer.SafeDispose();

			State.VertexLayout = -1;
			Deallocate(glDeleteVertexArrays, _vertexLayout);
		}

		/// <summary>
		///   Gets the pointer to the GPU memory for the current frame.
		/// </summary>
		public Quad* GetPointer()
		{
			return (Quad*)_dataBuffer.GetChunkPointer();
		}
	}
}