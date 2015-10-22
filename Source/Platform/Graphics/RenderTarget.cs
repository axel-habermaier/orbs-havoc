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
	using Logging;
	using Math;
	using Rendering;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents the target of a rendering operation.
	/// </summary>
	public sealed unsafe class RenderTarget : GraphicsObject
	{
		private readonly Texture _texture;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		internal RenderTarget()
		{
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="size">The size of the render target.</param>
		public RenderTarget(Size size)
		{
			Assert.That(size.Width > 0 && size.Height > 0, "Invalid render target size.");

			Size = size;
			_texture = new Texture(size, GL_RGBA, null);
			Handle = Allocate(glGenFramebuffers, nameof(RenderTarget));

			glBindFramebuffer(GL_DRAW_FRAMEBUFFER, Handle);
			glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _texture, 0);
			CheckErrors();

			switch (glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER))
			{
				case GL_FRAMEBUFFER_COMPLETE:
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER.");
					break;
				case GL_FRAMEBUFFER_UNSUPPORTED:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_UNSUPPORTED.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS:
					Log.Die("Frame buffer status: GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS.");
					break;
				default:
					Log.Die("The frame buffer is incomplete for an unknown reason.");
					break;
			}

			var buffer = GL_COLOR_ATTACHMENT0;
			glDrawBuffers(1, &buffer);
			CheckErrors();
		}

		/// <summary>
		///   Gets the texture the render target renders to.
		/// </summary>
		public Texture Texture
		{
			get
			{
				Assert.NotDisposed(this);
				Assert.That(!IsBackBuffer, "Cannot retrieve back buffer texture.");

				return _texture;
			}
		}

		/// <summary>
		///   Gets a value indicating whether the render target is the back buffer of a swap chain.
		/// </summary>
		public bool IsBackBuffer => Handle == 0;

		/// <summary>
		///   Gets the size of the render target.
		/// </summary>
		public Size Size { get; }

		/// <summary>
		///   Clears the render target.
		/// </summary>
		/// <param name="color">The color the color buffer should be set to.</param>
		public void Clear(Color color)
		{
			if (Change(ref State.RenderTarget, this))
				glBindFramebuffer(GL_DRAW_FRAMEBUFFER, Handle);

			glClearColor(color.Red / 255.0f, color.Green / 255.0f, color.Blue / 255.0f, color.Alpha / 255.0f);
			glClear(GL_COLOR_BUFFER_BIT);
			CheckErrors();
		}

		/// <summary>
		///   Draws primitiveCount-many primitives, starting at the given offset into the currently bound vertex buffers.
		/// </summary>
		/// <param name="primitiveCount">The number of primitives that should be drawn.</param>
		/// <param name="vertexOffset">The offset into the vertex buffers.</param>
		private void Draw(int primitiveCount, int vertexOffset)
		{
			if (Change(ref State.RenderTarget, this))
				glBindFramebuffer(GL_DRAW_FRAMEBUFFER, Handle);

			glBindVertexArray(State.VertexLayout);
			glDrawArrays(GL_TRIANGLES, vertexOffset, 3 * primitiveCount);
			glBindVertexArray(0);
			CheckErrors();
		}

		/// <summary>
		///   Draws indexCount-many indices, starting at the given index offset into the currently bound index buffer, where the
		///   vertex offset is added to each index before accessing the currently bound vertex buffers.
		/// </summary>
		/// <param name="indexCount">The number of indices to draw.</param>
		/// <param name="indexOffset">The location of the first index read by the GPU from the index buffer.</param>
		/// <param name="vertexOffset">The value that should be added to each index before reading a vertex from the vertex buffer.</param>
		private void DrawIndexed(int indexCount, int indexOffset, int vertexOffset)
		{
			if (Change(ref State.RenderTarget, this))
				glBindFramebuffer(GL_DRAW_FRAMEBUFFER, Handle);

			glBindVertexArray(State.VertexLayout);
			glDrawElementsBaseVertex(GL_TRIANGLES, indexCount, GL_UNSIGNED_INT, (void*)(indexOffset * sizeof(uint)), vertexOffset);
			glBindVertexArray(0);
			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.RenderTarget, this);
			Deallocate(glDeleteFramebuffers, Handle);
		}
	}
}