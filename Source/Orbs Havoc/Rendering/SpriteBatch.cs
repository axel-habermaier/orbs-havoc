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

namespace OrbsHavoc.Rendering
{
	using System.Collections.Generic;
	using System.Numerics;
	using Platform;
	using Platform.Graphics;
	using Utilities;

	/// <summary>
	///   Efficiently draws large amounts of 2D sprites.
	/// </summary>
	public sealed unsafe class SpriteBatch : RenderOperation
	{
		private static readonly LayerComparer _layerComparer = new LayerComparer();
		private readonly List<QuadPartition> _partitions = new List<QuadPartition>();

		/// <summary>
		///   The current render state of the sprite batch that determines how all subsequently added sprites are rendered.
		/// </summary>
		public RenderState RenderState = RenderState.Default;

		/// <summary>
		///   Executes the render operation.
		/// </summary>
		internal override void Execute()
		{
			Assert.NotPooled(this);

			Renderer.RenderBuffer.Bind();

			// Sort the partitions by layer so that we handle overdraw and transparency correctly then
			// draw the quads, starting with the lowest layer
			_partitions.Sort(_layerComparer);

			foreach (var partition in _partitions)
			{
				partition.RenderState.Bind(Renderer.GraphicsDevice, Renderer.DefaultCamera);
				Renderer.Draw(partition.RenderState.RenderTarget, partition.Count, partition.Offset, PrimitiveType.Points);
			}

			// Make sure we don't "leak out" the scissor rasterizer state
			Renderer.GraphicsDevice.DisableScissorTest();
		}

		/// <summary>
		///   Draws the outline of a rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="color">The color of the outline.</param>
		/// <param name="thickness">The thickness of the outline.</param>
		public void DrawOutline(Rectangle rectangle, Color color, float thickness = 1)
		{
			// Make sure there is no overdraw at the corners that would be visible depending on the opacity and blend mode
			DrawLine(rectangle.TopLeft, rectangle.TopRight, color, thickness);
			DrawLine(rectangle.BottomLeft + new Vector2(1, -2), rectangle.TopLeft + new Vector2(1, 0), color, thickness);
			DrawLine(rectangle.TopRight, rectangle.BottomRight - new Vector2(0, 1), color, thickness);
			DrawLine(rectangle.BottomLeft - new Vector2(0, 1), rectangle.BottomRight - new Vector2(0, 1), color, thickness);
		}

		/// <summary>
		///   Draws a textured rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the rectangle.</param>
		public void Draw(Rectangle rectangle, Texture texture)
		{
			Draw(rectangle, texture, Colors.White);
		}

		/// <summary>
		///   Draws a colored rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="color">The color that should be used to draw the rectangle.</param>
		public void Draw(Rectangle rectangle, Color color)
		{
			Draw(rectangle, Renderer.WhiteTexture, color);
		}

		/// <summary>
		///   Draws a textured rectangle.
		/// </summary>
		/// <param name="rectangle">The rectangle that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the rectangle.</param>
		/// <param name="color">The color of the quad.</param>
		/// <param name="texCoords">The texture coordinates that should be used.</param>
		public void Draw(Rectangle rectangle, Texture texture, Color color, Rectangle? texCoords = null)
		{
			var quad = new Quad
			{
				Color = color,
				Position = rectangle.Center,
				Size = rectangle.Size,
				TextureCoordinates = texCoords ?? Rectangle.Unit
			};

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws the given rectangle.
		/// </summary>
		/// <param name="position">The position of the quad that should be drawn.</param>
		/// <param name="size">The size of the quad that should be drawn.</param>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="color">The color of the quad.</param>
		/// <param name="orientation">The orientation (in radians) of the quad.</param>
		/// <param name="texCoords">The texture coordinates that should be used to draw the quad.</param>
		public void Draw(Vector2 position, Size size, Texture texture, Color color, float orientation, Rectangle? texCoords = null)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var quad = new Quad
			{
				Color = color,
				Position = position,
				Size = size,
				Orientation = orientation,
				TextureCoordinates = texCoords ?? Rectangle.Unit
			};

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws a textured rectangle at the given position with the texture's size.
		/// </summary>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="position">The position of the quad.</param>
		/// <param name="color">The color of the quad.</param>
		public void Draw(Texture texture, Vector2 position, Color color)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var quad = new Quad
			{
				Color = color,
				Position = position,
				Size = texture.Size,
				TextureCoordinates = Rectangle.Unit
			};

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws a textured rectangle at the given position with the texture's size and rotation.
		/// </summary>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		/// <param name="orientation">The orientation of the quad, in radians.</param>
		/// <param name="position">The position of the quad.</param>
		/// <param name="color">The color of the quad.</param>
		public void Draw(Texture texture, Vector2 position, float orientation, Color color)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			var quad = new Quad
			{
				Color = color,
				Position = position,
				Size = texture.Size,
				Orientation = orientation,
				TextureCoordinates = Rectangle.Unit
			};

			Draw(ref quad, texture);
		}

		/// <summary>
		///   Draws a line.
		/// </summary>
		/// <param name="start">The start of the line.</param>
		/// <param name="end">The end of the line.</param>
		/// <param name="color">The color of the line.</param>
		/// <param name="width">The width of the line.</param>
		public void DrawLine(Vector2 start, Vector2 end, Color color, float width)
		{
			if (MathUtils.Equals(width, 0))
				return;

			var direction = end - start;
			var length = direction.Length();

			if (MathUtils.Equals(length, 0))
				return;

			if (width <= 1) // prevents rounding errors on Nvidia GPUs
				start.Y -= 0.1f;

			var size = new Size(length, width);
			var quad = new Quad
			{
				Color = color,
				Orientation = MathUtils.ComputeAngle(start, end, new Vector2(1, 0)),
				Position = start + Vector2.Normalize(direction) * length / 2,
				Size = size,
				TextureCoordinates = Rectangle.Unit
			};

			Draw(ref quad, Renderer.WhiteTexture);
		}

		/// <summary>
		///   Draws the given quad.
		/// </summary>
		/// <param name="quad">The quad that should be added.</param>
		/// <param name="texture">The texture that should be used to draw the quad.</param>
		public void Draw(ref Quad quad, Texture texture)
		{
			*AddQuads(1, texture) = quad;
		}

		/// <summary>
		///   Draws the given quads.
		/// </summary>
		/// <param name="quads">The quads that should be added.</param>
		/// <param name="count">The number of quads to draw.</param>
		/// <param name="texture">The texture that should be used to draw the quads.</param>
		public void Draw(Quad[] quads, int count, Texture texture)
		{
			Assert.ArgumentNotNull(quads, nameof(quads));
			Assert.ArgumentNotNull(texture, nameof(texture));
			Assert.ArgumentInRange(count, 0, quads.Length, nameof(count));

			fixed (Quad* ptr = quads)
				Interop.Copy(AddQuads(count, texture), ptr, count * sizeof(Quad));
		}

		/// <summary>
		///   Allocates the given number of quads and returns a pointer to the memory location the quad data should be written to. The
		///   written number of quads must match the specified count exactly, otherwise undefined behavior occurs.
		/// </summary>
		/// <param name="count">The number of quads that will be written.</param>
		/// <param name="texture">The texture that should be used to draw the quads.</param>
		public Quad* AddQuads(int count, Texture texture)
		{
			Assert.NotPooled(this);

			RenderState.Texture = texture;
			RenderState.Validate();

			return Renderer.Quads.AddQuads(GetPartition(), count);
		}

		/// <summary>
		///   Gets a partition for the current render state.
		/// </summary>
		private QuadPartition GetPartition()
		{
			// First, check if we can continue to append to the last used partition as this would be most efficient
			if (_partitions.Count > 0 && _partitions[_partitions.Count - 1].RenderState.Matches(ref RenderState))
				return _partitions[_partitions.Count - 1];

			// Otherwise, search for a partition with a matching render state and return that one; also, we already
			// know that the last partition does not match, so skip it here
			for (var i = 0; i < _partitions.Count - 1; ++i)
			{
				if (_partitions[i].RenderState.Matches(ref RenderState))
					return _partitions[i];
			}

			// If we don't have a matching partition yet, create one
			var partition = Renderer.Quads.AllocatePartition(ref RenderState);
			_partitions.Add(partition);
			return partition;
		}

		/// <summary>
		///   Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected override void OnReturning()
		{
			_partitions.Clear();
			RenderState = RenderState.Default;
		}

		/// <summary>
		///   Used to sort quad partitions by layer.
		/// </summary>
		private class LayerComparer : IComparer<QuadPartition>
		{
			/// <summary>
			///   Compares the layers of the given quad partitions.
			/// </summary>
			public int Compare(QuadPartition x, QuadPartition y)
			{
				return x.RenderState.Layer.CompareTo(y.RenderState.Layer);
			}
		}
	}
}