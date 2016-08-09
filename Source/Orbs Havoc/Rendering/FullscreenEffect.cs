﻿// The MIT License (MIT)
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
	using Utilities;

	/// <summary>
	///   Represents a fullscreen effect that applies a shader to an input texture originating from a render target, rendering the
	///   result fullscreen to the output render target.
	/// </summary>
	public abstract class FullscreenEffect : RenderOperation
	{
		/// <summary>
		///   Gets or sets the input render target the effect should be applied to.
		/// </summary>
		public RenderTarget Input { get; set; }

		/// <summary>
		///   Gets or sets the output render target that stores the rendered result.
		/// </summary>
		public RenderTarget Output { get; set; }

		/// <summary>
		///   Draws fullscreen into the given render target.
		/// </summary>
		/// <param name="renderTarget">The render target that should be drawn into.</param>
		protected void DrawFullscreen(RenderTarget renderTarget)
		{
			Assert.ArgumentNotNull(renderTarget, nameof(renderTarget));
			Assert.NotNull(Input);
			Assert.NotNull(Output);
			Assert.That(Input != Output, "Input and output must differ.");

			// We're rendering a fullscreen triangle that is generated by the vertex shader; the shader
			// does not require any inputs. Unfortunately, OpenGL raises an error when no vertex layout
			// is bound, so let's just bind the render buffer
			Renderer.RenderBuffer.Bind();
			Renderer.Draw(renderTarget, 3, 0);
		}
	}
}