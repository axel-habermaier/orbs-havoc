// The MIT License (MIT)
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
	using Math;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   An input-layout holds a definition of how to feed vertex data that is laid out in memory into the
	///   input-assembler stage of the graphics pipeline.
	/// </summary>
	public sealed unsafe class VertexLayout : GraphicsObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public VertexLayout(Buffer vertexBuffer, Buffer indexBuffer)
		{
			Assert.ArgumentNotNull(vertexBuffer, nameof(vertexBuffer));
			Assert.ArgumentNotNull(indexBuffer, nameof(indexBuffer));

			Handle = Allocate(glGenVertexArrays, "VertexLayout");

			glBindVertexArray(Handle);
			glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, indexBuffer);
			glBindBuffer(GL_ARRAY_BUFFER, vertexBuffer);

			glEnableVertexAttribArray(0);
			glVertexAttribPointer(0, 2, GL_FLOAT, false, sizeof(Vertex), (void*)0);

			glEnableVertexAttribArray(1);
			glVertexAttribPointer(1, 2, GL_FLOAT, false, sizeof(Vertex), (void*)sizeof(Vector2));

			// The type of color inputs in the shader is Vector4; however, we only send RGBA8 values and let the shader
			// convert the data to save 12 bytes per vertex
			glEnableVertexAttribArray(2);
			glVertexAttribPointer(2, 4, GL_UNSIGNED_BYTE, true, sizeof(Vertex), (void*)(2 * sizeof(Vector2)));

			glBindVertexArray(0);
		}

		/// <summary>
		///   Binds the vertex layout for rendering.
		/// </summary>
		public void Bind()
		{
			// Do not actually bind the input layout here, as that causes all sorts of problems with buffer updates between
			// the binding of the input layout and the actual draw call using the input layout
			State.VertexLayout = this;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.VertexLayout, this);
			Deallocate(glDeleteVertexArrays, Handle);
		}
	}
}