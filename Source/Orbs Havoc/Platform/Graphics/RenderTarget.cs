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
	using Logging;
	using Memory;
	using Rendering;
	using Utilities;
	using static GraphicsHelpers;
	using static OpenGL3;

	/// <summary>
	///   Represents the target of a rendering operation.
	/// </summary>
	public sealed unsafe class RenderTarget : DisposableObject
	{
		private readonly int _renderTarget;
		private readonly Size _size;
		private readonly Texture _texture;
		private readonly Window _window;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="window">The window whose back buffer should be represented by the render target.</param>
		internal RenderTarget(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.Resized += ChangeViewport;

			ChangeViewport(_window.Size);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="size">The size of the render target.</param>
		public RenderTarget(Size size)
		{
			Assert.That(size.Width > 0 && size.Height > 0, "Invalid render target size.");

			_size = size;
			_texture = new Texture(size, GL_RGBA, null);

			Viewport = new Rectangle(0, 0, _size);
			_renderTarget = Allocate(glGenFramebuffers, nameof(RenderTarget));

			glBindFramebuffer(GL_DRAW_FRAMEBUFFER, _renderTarget);
			glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, _texture, 0);
			CheckErrors();

			switch (glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER))
			{
				case GL_FRAMEBUFFER_COMPLETE:
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT)}.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT)}.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER)}.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER)}.");
					break;
				case GL_FRAMEBUFFER_UNSUPPORTED:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_UNSUPPORTED)}.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE)}.");
					break;
				case GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS:
					Log.Die($"Frame buffer status: {nameof(GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS)}.");
					break;
				default:
					Log.Die("The frame buffer is incomplete for an unknown reason.");
					break;
			}

			var buffer = GL_COLOR_ATTACHMENT0;
			glDrawBuffers(1, &buffer);
			CheckErrors();

			State.RenderTarget = null;
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
		public bool IsBackBuffer => _window != null;

		/// <summary>
		///   Gets the size of the render target.
		/// </summary>
		public Size Size => IsBackBuffer ? _window.Size : _size;

		/// <summary>
		///   Gets or sets the viewport used when drawing to the render target.
		/// </summary>
		public Rectangle Viewport { get; set; }

		/// <summary>
		///   Changes the size of the viewport.
		/// </summary>
		private void ChangeViewport(Size size)
		{
			Viewport = new Rectangle(0, 0, size);
		}

		/// <summary>
		///   Sets the viewport used for rendering, if necessary.
		/// </summary>
		private void SetViewport()
		{
			if (!Change(ref State.Viewport, Viewport))
				return;

			glViewport(
				MathUtils.RoundIntegral(Viewport.Left),
				MathUtils.RoundIntegral(Viewport.Top),
				MathUtils.RoundIntegral(Viewport.Width),
				MathUtils.RoundIntegral(Viewport.Height));
		}

		/// <summary>
		///   Binds the render target for rendering.
		/// </summary>
		public void Bind()
		{
			if (IsBackBuffer && Change(ref State.Window, _window))
				_window.MakeCurrent();

			if (Change(ref State.RenderTarget, this))
				glBindFramebuffer(GL_DRAW_FRAMEBUFFER, _renderTarget);

			SetViewport();
		}

		/// <summary>
		///   Clears the render target.
		/// </summary>
		/// <param name="color">The color the color buffer should be set to.</param>
		public void Clear(Color color)
		{
			Bind();
			SetViewport();

			glClearColor(color.Red / 255.0f, color.Green / 255.0f, color.Blue / 255.0f, color.Alpha / 255.0f);
			glClear(GL_COLOR_BUFFER_BIT);

			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			if (_window != null)
				_window.Resized -= ChangeViewport;

			Unset(ref State.RenderTarget, this);
			Deallocate(glDeleteFramebuffers, _renderTarget);

			if (!IsBackBuffer)
				Texture.SafeDispose();
		}
	}
}