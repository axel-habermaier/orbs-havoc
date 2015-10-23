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
	using System.Runtime.InteropServices;
	using Math;
	using Platform.Graphics;
	using Utilities;
	using static Platform.Graphics.OpenGL3;

	/// <summary>
	///   Represents a rectangle with possibly non-axis aligned edges.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Quad
	{
		/// <summary>
		///   Creates a dynamic vertex buffer that holds the given number of quads.
		/// </summary>
		/// <param name="quadCount">The number of quads that the dynamic vertex buffer should be able to hold.</param>
		public static DynamicBuffer CreateDynamicVertexBuffer(uint quadCount)
		{
			return DynamicBuffer.Create<Vertex>(GL_ARRAY_BUFFER, quadCount * 4);
		}

		/// <summary>
		///   The vertex that conceptually represents the bottom left corner of the quad.
		/// </summary>
		public Vertex BottomLeft;

		/// <summary>
		///   The vertex that conceptually represents the bottom right corner of the quad.
		/// </summary>
		public Vertex BottomRight;

		/// <summary>
		///   The vertex that conceptually represents the top left corner of the quad.
		/// </summary>
		public Vertex TopLeft;

		/// <summary>
		///   The vertex that conceptually represents the top right corner of the quad.
		/// </summary>
		public Vertex TopRight;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="rectangle">The position and size of the rectangular quad.</param>
		/// <param name="color">The color of the quad.</param>
		/// <param name="textureArea">
		///   The area of the texture that contains the quad's image data. If not given, the whole texture
		///   is drawn onto the quad.
		/// </param>
		public Quad(Rectangle rectangle, Color color, Rectangle? textureArea = null)
			: this()
		{
			BottomLeft.Position = new Vector2(rectangle.Left, rectangle.Bottom);
			BottomRight.Position = new Vector2(rectangle.Right, rectangle.Bottom);
			TopLeft.Position = new Vector2(rectangle.Left, rectangle.Top);
			TopRight.Position = new Vector2(rectangle.Right, rectangle.Top);

			BottomLeft.Color = color;
			BottomRight.Color = color;
			TopLeft.Color = color;
			TopRight.Color = color;

			var texture = textureArea ?? new Rectangle(0, 0, 1, 1);
			BottomLeft.TextureCoordinates = new Vector2(texture.Left, texture.Bottom);
			BottomRight.TextureCoordinates = new Vector2(texture.Right, texture.Bottom);
			TopLeft.TextureCoordinates = new Vector2(texture.Left, texture.Top);
			TopRight.TextureCoordinates = new Vector2(texture.Right, texture.Top);
		}

		/// <summary>
		///   Changes the color of the quad.
		/// </summary>
		/// <param name="color">The new color of the quad.</param>
		public void ChangeColor(Color color)
		{
			BottomLeft.Color = color;
			BottomRight.Color = color;
			TopLeft.Color = color;
			TopRight.Color = color;
		}

		/// <summary>
		///   Applies the given transformation matrix to the quad's vertices.
		/// </summary>
		/// <param name="quad">The quad that should be transformed.</param>
		/// <param name="matrix">The transformation matrix that should be applied.</param>
		public static void Transform(ref Quad quad, ref Matrix matrix)
		{
			quad.BottomLeft.Position = Vector2.Transform(ref quad.BottomLeft.Position, ref matrix);
			quad.BottomRight.Position = Vector2.Transform(ref quad.BottomRight.Position, ref matrix);
			quad.TopLeft.Position = Vector2.Transform(ref quad.TopLeft.Position, ref matrix);
			quad.TopRight.Position = Vector2.Transform(ref quad.TopRight.Position, ref matrix);
		}

		/// <summary>
		///   Applies the given position offset to the quad's vertices.
		/// </summary>
		/// <param name="quad">The quad that should be transformed.</param>
		/// <param name="positionOffset">The position offset that should be applied.</param>
		public static void Offset(ref Quad quad, ref Vector2 positionOffset)
		{
			quad.BottomLeft.Position = quad.BottomLeft.Position + positionOffset;
			quad.BottomRight.Position = quad.BottomRight.Position + positionOffset;
			quad.TopLeft.Position = quad.TopLeft.Position + positionOffset;
			quad.TopRight.Position = quad.TopRight.Position + positionOffset;
		}

		/// <summary>
		///   Creates a quad that represents a line between the given start and end points.
		/// </summary>
		/// <param name="start">The start of the line.</param>
		/// <param name="end">The end of the line.</param>
		/// <param name="color">The color of the line.</param>
		/// <param name="width">The width of the line.</param>
		/// <param name="textureArea">
		///   The area of the texture that contains the quad's image data. If not given, the whole texture
		///   is drawn onto the quad.
		/// </param>
		public static Quad FromLine(Vector2 start, Vector2 end, Color color, float width, Rectangle? textureArea = null)
		{
			if (MathUtils.Equals(width, 0) || MathUtils.Equals((start - end).LengthSquared, 0))
				return new Quad();

			var length = (end - start).Length;
			var rotation = MathUtils.ComputeAngle(start, end, new Vector2(1, 0));

			return FromLine(start, length, rotation, color, width, textureArea);
		}

		/// <summary>
		///   Creates a quad that represents a line.
		/// </summary>
		/// <param name="position">The position of the line's start.</param>
		/// <param name="length">The length of the line.</param>
		/// <param name="rotation">The rotation of the line.</param>
		/// <param name="color">The color of the line.</param>
		/// <param name="width">The width of the line.</param>
		/// <param name="textureArea">
		///   The area of the texture that contains the quad's image data. If not given, the whole texture
		///   is drawn onto the quad.
		/// </param>
		public static Quad FromLine(Vector2 position, float length, float rotation, Color color, float width, Rectangle? textureArea = null)
		{
			Assert.ArgumentSatisfies(width >= 0, nameof(width), "Invalid width.");

			if (MathUtils.Equals(width, 0) || MathUtils.Equals(length, 0))
				return new Quad();

			// We first define a default quad to draw a line that goes from left to right. The center of the 
			// rectangle lies on the start point of the line.
			var rectangle = new Rectangle(0, -width / 2.0f, length, width);
			var quad = new Quad(rectangle, color, textureArea);

			// Construct the transformation matrix and draw the transformed quad
			var transformMatrix = Matrix.CreateRotationZ(-rotation) *
								  Matrix.CreateTranslation(position.X, position.Y, 0);

			Transform(ref quad, ref transformMatrix);
			return quad;
		}
	}
}