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
	using Memory;
	using Rendering;
	using Utilities;
	using static GraphicsHelpers;
	using static OpenGL3;

	/// <summary>
	///   Represents the vertex layout expected by a vertex shader.
	/// </summary>
	public sealed unsafe class VertexLayout : DisposableObject
	{
		private readonly int _vertexLayout = Allocate(glGenVertexArrays, "Vertex Layout");
		private int _index = 1;
		private byte* _offset;

		/// <summary>
		///   Adds a vertex attribute to the layout.
		/// </summary>
		/// <param name="buffer">The buffer the data should be read from.</param>
		/// <param name="componentCount">The number of components accessed by the attribute.</param>
		/// <param name="type">The data type of the attribute.</param>
		/// <param name="size">The size in bytes of the attribute.</param>
		/// <param name="normalize">Indicates whether the attribute values should be normalized.</param>
		public void AddAttribute(Buffer buffer, int componentCount, DataFormat type, int size, bool normalize)
		{
			glBindVertexArray(_vertexLayout);
			CheckErrors();

			glBindBuffer(GL_ARRAY_BUFFER, buffer);
			CheckErrors();

			glEnableVertexAttribArray(_index);
			glVertexAttribPointer(_index, componentCount, (int)type, normalize, Quad.SizeInBytes, _offset);
			CheckErrors();

			_index += 1;
			_offset += componentCount * size;

			glBindVertexArray(0);
			CheckErrors();
		}

		/// <summary>
		///   Binds the the vertex layout for rendering.
		/// </summary>
		public void Bind()
		{
			// Do not actually bind the vertex layout here, as that causes all sorts of problems with buffer updates between
			// the binding of the vertex layout and the actual draw call using the vertex layout
			State.VertexLayout = this;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.VertexLayout, this);
			Deallocate(glDeleteVertexArrays, _vertexLayout);
		}

		/// <summary>
		///   Casts the vertex layout to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(VertexLayout obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._vertexLayout;
		}
	}
}