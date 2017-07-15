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

namespace OrbsHavoc.Platform.Graphics
{
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents the vertex layout expected by a vertex shader.
	/// </summary>
	public sealed unsafe class VertexLayout : DisposableObject
	{
		private readonly int _vertexLayout = Allocate(glGenVertexArrays, "Vertex Layout");

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="attributes">The attributes the layout should contain.</param>
		public VertexLayout(params VertexAttribute[] attributes)
		{
			Assert.ArgumentNotNull(attributes, nameof(attributes));
			Assert.ArgumentSatisfies(attributes.Length > 0, nameof(attributes), "Expected at least one vertex attribute.");

			glBindVertexArray(_vertexLayout);

			var index = 1;
			byte* offset = null;
			foreach (var attribute in attributes)
				AddAttribute(attribute, ref offset, ref index);

			glBindVertexArray(0);
		}

		/// <summary>
		///   Adds the vertex attribute to the layout.
		/// </summary>
		private static void AddAttribute(VertexAttribute attribute, ref byte* offset, ref int index)
		{
			glBindBuffer(GL_ARRAY_BUFFER, attribute.Buffer);
			glEnableVertexAttribArray(index);
			glVertexAttribPointer(index, attribute.ComponentCount, (int)attribute.DataFormat, attribute.Normalize, attribute.StrideInBytes, offset);

			index += 1;
			offset += attribute.ComponentCount * attribute.SizeInBytes;

			glBindBuffer(GL_ARRAY_BUFFER, 0);
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