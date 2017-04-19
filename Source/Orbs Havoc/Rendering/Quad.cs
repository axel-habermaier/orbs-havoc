// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using System;
	using System.Numerics;
	using System.Runtime.InteropServices;
	using Utilities;

	/// <summary>
	///   Represents a rectangle with possibly non-axis aligned edges.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = SizeInBytes)]
	public struct Quad
	{
		/// <summary>
		///   The size of a quad in bytes.
		/// </summary>
		public const int SizeInBytes = 28;

		/// <summary>
		///   Gets or sets the position of the quad's center.
		/// </summary>
		public Vector2 Position { get; set; }

		/// <summary>
		///   Gets or sets the quad's orientation in radians.
		/// </summary>
		public float Orientation { get; set; }

		/// <summary>
		///   Gets or sets the quad's color.
		/// </summary>
		public Color Color { get; set; }

		private ushort _width;
		private ushort _height;
		private ushort _texOffsetX;
		private ushort _texOffsetY;
		private ushort _texWidth;
		private ushort _texHeight;

		/// <summary>
		///   Gets or sets the quad's size.
		/// </summary>
		public Size Size
		{
			get => new Size(_width, _height);
			set
			{
				Assert.InRange(value.Width, 0, UInt16.MaxValue);
				Assert.InRange(value.Height, 0, UInt16.MaxValue);

				_width = (ushort)value.Width;
				_height = (ushort)value.Height;
			}
		}

		/// <summary>
		///   Gets or sets the quad's texture coordinates.
		/// </summary>
		public Rectangle TextureCoordinates
		{
			get => new Rectangle(
				_texOffsetX / (float)UInt16.MaxValue,
				_texOffsetY / (float)UInt16.MaxValue,
				_texWidth / (float)UInt16.MaxValue,
				_texHeight / (float)UInt16.MaxValue);
			set
			{
				Assert.InRange(value.Left, 0, 1);
				Assert.InRange(value.Top, 0, 1);
				Assert.InRange(value.Width, 0, 1);
				Assert.InRange(value.Height, 0, 1);

				_texOffsetX = (ushort)(value.Left * UInt16.MaxValue);
				_texOffsetY = (ushort)(value.Top * UInt16.MaxValue);
				_texWidth = (ushort)(value.Width * UInt16.MaxValue);
				_texHeight = (ushort)(value.Height * UInt16.MaxValue);
			}
		}
	}
}