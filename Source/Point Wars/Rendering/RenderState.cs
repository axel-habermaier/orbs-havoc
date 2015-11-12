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
	using System.Diagnostics;
	using Platform.Graphics;
	using Utilities;

	/// <summary>
	///   Contains the GPU render state information required for sprite rendering.
	/// </summary>
	public struct RenderState
	{
		/// <summary>
		///   The blend operation that is used when rendering the sprites.
		/// </summary>
		public BlendOperation BlendOperation;

		/// <summary>
		///   The layer that determines the drawing order, which is important for transparent objects. Higher layers are drawn later.
		/// </summary>
		public int Layer;

		/// <summary>
		///   The sampler state that is used when sampling the sprite textures.
		/// </summary>
		public SamplerState SamplerState;

		/// <summary>
		///   The optional scissor area that restricts the rendering area of the sprites.
		/// </summary>
		public Rectangle? ScissorArea;

		/// <summary>
		///   The texture that is used to draw the sprites.
		/// </summary>
		public Texture Texture;

		/// <summary>
		///   The render target the sprites are rendered to.
		/// </summary>
		public RenderTarget RenderTarget;

		/// <summary>
		///   The camera the sprites are rendered for.
		/// </summary>
		public Camera Camera;

		/// <summary>
		///   In debug builds, validates the render state.
		/// </summary>
		[Conditional("DEBUG")]
		public void Validate()
		{
			Assert.InRange(BlendOperation);
			Assert.NotNull(SamplerState);
			Assert.NotNull(Texture);
			Assert.NotNull(RenderTarget);
		}

		/// <summary>
		///   Checks whether the this render state matches the given one.
		/// </summary>
		public bool Matches(ref RenderState renderState)
		{
			return
				BlendOperation == renderState.BlendOperation &&
				Layer == renderState.Layer &&
				SamplerState == renderState.SamplerState &&
				ScissorArea == renderState.ScissorArea &&
				Texture == renderState.Texture &&
				RenderTarget == renderState.RenderTarget &&
				Camera == renderState.Camera;
		}

		/// <summary>
		///   Resets the render state to its default values.
		/// </summary>
		public void Reset()
		{
			BlendOperation = BlendOperation.Premultiplied;
			SamplerState = SamplerState.Bilinear;
			Layer = 0;
			ScissorArea = null;
			Texture = null;
			RenderTarget = null;
			Camera = null;
		}
	}
}