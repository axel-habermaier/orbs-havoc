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
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a fullscreen effect that applies a shader to an input texture originating from a render target, rendering the
	///   result fullscreen to the output render target.
	/// </summary>
	public abstract class FullscreenEffect : DisposableObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="inputRenderTarget">The input render target the effect should be applied to.</param>
		/// <param name="outputRenderTarget">The output render target that stores the rendered result.</param>
		protected FullscreenEffect(RenderTarget inputRenderTarget, RenderTarget outputRenderTarget)
		{
			Assert.ArgumentNotNull(inputRenderTarget, nameof(inputRenderTarget));
			Assert.ArgumentNotNull(outputRenderTarget, nameof(outputRenderTarget));
			Assert.That(inputRenderTarget != outputRenderTarget, "Input and output must differ.");

			InputRenderTarget = inputRenderTarget;
			OutputRenderTarget = outputRenderTarget;
		}

		/// <summary>
		///   Gets the input render target the effect should be applied to.
		/// </summary>
		public RenderTarget InputRenderTarget { get; }

		/// <summary>
		///   Gets the output render target that stores the rendered result.
		/// </summary>
		public RenderTarget OutputRenderTarget { get; }

		/// <summary>
		///   Prepares the effect for rendering and returns the number of required rendering passes..
		/// </summary>
		/// <param name="size">The size of the effect's render area.</param>
		public abstract int PrepareForRendering(Size size);

		/// <summary>
		///   Prepares the effect for rendering the given pass and returns the render target that should be rendered to..
		/// </summary>
		/// <param name="pass">The zero-based index of the pass that should be prepared.</param>
		public abstract RenderTarget PreparePass(int pass);
	}
}