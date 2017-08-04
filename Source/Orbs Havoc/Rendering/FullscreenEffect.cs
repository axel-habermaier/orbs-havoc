﻿namespace OrbsHavoc.Rendering
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
			Renderer.Draw(renderTarget, 3, 0, PrimitiveType.Triangles);
		}
	}
}