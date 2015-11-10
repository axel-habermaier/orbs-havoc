﻿// The MIT License (MIT)
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
	using System.Diagnostics;
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
		///   In debug builds, checks whether the assigned input and output render targets are valid.
		/// </summary>
		[Conditional("DEBUG")]
		protected void ValidateRenderTargets()
		{
			Assert.NotNull(Input);
			Assert.NotNull(Output);
			Assert.That(Input != Output, "Input and output must differ.");
		}

		/// <summary>
		///   Draws fullscreen into the given render target.
		/// </summary>
		protected void DrawFullscreen(RenderTarget renderTarget)
		{
			Assert.ArgumentNotNull(renderTarget, nameof(renderTarget));

			// TODO
		}
	}
}