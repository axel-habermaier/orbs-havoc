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
	using System.Diagnostics;
	using Math;
	using Utilities;

	/// <summary>
	///   Represents the state of the graphics device.
	/// </summary>
	public sealed class GraphicsState
	{
		/// <summary>
		///   The maximum number of constant buffers that can be bound simultaneously.
		/// </summary>
		public const uint ConstantBufferSlotCount = 14;

		/// <summary>
		///   The constant buffers that are currently bound.
		/// </summary>
		public readonly Buffer[] ConstantBuffers = new Buffer[ConstantBufferSlotCount];

		/// <summary>
		///   Indicates whether drawing operations are currently allowed.
		/// </summary>
		public bool CanDraw;

		/// <summary>
		///   The currently bound render target.
		/// </summary>
		public RenderTarget RenderTarget;

		/// <summary>
		///   The currently bound sampler state.
		/// </summary>
		public SamplerState SamplerState;

		/// <summary>
		///   The currently bound shader.
		/// </summary>
		public Shader Shader;

		/// <summary>
		///   The currently bound texture.
		/// </summary>
		public Texture Texture;

		/// <summary>
		///   The currently bound vertex layout.
		/// </summary>
		public VertexLayout VertexLayout;

		/// <summary>
		///   The currently bound viewport.
		/// </summary>
		public Rectangle Viewport;

		/// <summary>
		///   The currently bound window.
		/// </summary>
		public Window Window;

		/// <summary>
		///   In debug builds, validates the state of the graphics device before drawing.
		/// </summary>
		[Conditional("DEBUG")]
		internal void Validate()
		{
			Assert.That(CanDraw, "Drawing commands can only be issued between a call to BeginFrame() and EndFrame().");
			Assert.NotNull(RenderTarget);
			Assert.NotNull(Shader);
			Assert.NotNull(VertexLayout);
			Assert.NotNull(RenderTarget);
			Assert.That(Viewport.Size.Width * Viewport.Size.Height > 0, "Viewport has an area of 0.");
		}
	}
}