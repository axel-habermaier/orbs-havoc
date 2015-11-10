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
	using Utilities;

	/// <summary>
	///   Represents the state of the graphics device.
	/// </summary>
	public sealed class GraphicsState
	{
		/// <summary>
		///   The maximum number of constant buffers that can be bound simultaneously.
		/// </summary>
		public const int ConstantBufferSlotCount = 14;

		/// <summary>
		///   The maximum number of constant buffers that can be bound simultaneously.
		/// </summary>
		public const int TextureSlotCount = 8;

		/// <summary>
		///   The maximum number of frames the GPU is allowed to be behind the CPU.
		/// </summary>
		public const int MaxFrameLag = 3;

		/// <summary>
		/// The monotonically increasing GPU frame number.
		/// </summary>
		public uint FrameNumber = 1;

		/// <summary>
		///   The constant buffers that are currently bound.
		/// </summary>
		public readonly DynamicBuffer[] ConstantBuffers = new DynamicBuffer[ConstantBufferSlotCount];

		/// <summary>
		///   The currently bound sampler states.
		/// </summary>
		public readonly SamplerState[] SamplerStates = new SamplerState[TextureSlotCount];

		/// <summary>
		///   The currently bound textures.
		/// </summary>
		public readonly Texture[] Textures = new Texture[TextureSlotCount];

		/// <summary>
		///   The currently active texture slot.
		/// </summary>
		public int ActiveTextureSlot = -1;

		/// <summary>
		///   The currently bound blend operation.
		/// </summary>
		public BlendOperation BlendOperation;

		/// <summary>
		///   Indicates whether drawing operations are currently allowed.
		/// </summary>
		public bool CanDraw;

		/// <summary>
		///   The currently bound render target.
		/// </summary>
		public RenderTarget RenderTarget;

		/// <summary>
		///   The currently bound shader.
		/// </summary>
		public Shader Shader;

		/// <summary>
		///   The currently bound vertex layout.
		/// </summary>
		public int VertexLayout = -1;

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
			Assert.NotNull(RenderTarget);
			Assert.InRange(BlendOperation);
			Assert.That(VertexLayout >= 0, "Invalid vertex layout.");
			Assert.That(Viewport.Size.Width * Viewport.Size.Height > 0, "Viewport has an area of 0.");
		}
	}
}