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

namespace OrbsHavoc.Rendering
{
	using Platform.Graphics;
	using Utilities;

	/// <summary>
	///   Represents a render operation that clears a render target.
	/// </summary>
	public sealed class ClearRenderTarget : RenderOperation
	{
		/// <summary>
		///   Gets or sets the render target cleared by the operation.
		/// </summary>
		public RenderTarget RenderTarget { get; set; }

		/// <summary>
		///   Gets or sets the color that should be used to clear the render target.
		/// </summary>
		public Color ClearColor { get; set; }

		/// <summary>
		///   Executes the render operation.
		/// </summary>
		internal override void Execute()
		{
			Assert.NotNull(RenderTarget, "No render target has been set.");
			RenderTarget.Clear(ClearColor);
		}
	}
}