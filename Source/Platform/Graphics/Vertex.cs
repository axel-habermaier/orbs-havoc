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

namespace PointWars.Platform.Graphics
{
	using System.Numerics;
	using System.Runtime.InteropServices;
	using Rendering;

	/// <summary>
	///   Holds position, texture coordinates, and color data for a vertex.
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct Vertex
	{
		/// <summary>
		///   The size in bytes of the structure.
		/// </summary>
		public const int Size = 20;

		/// <summary>
		///   Gets or sets the vertex' position.
		/// </summary>
		[FieldOffset(0)]
		public Vector2 Position;

		/// <summary>
		///   Gets or sets the vertex' texture coordinates.
		/// </summary>
		[FieldOffset(8)]
		public Vector2 TextureCoordinates;

		/// <summary>
		///   Gets or sets the vertex' color.
		/// </summary>
		[FieldOffset(16)]
		public Color Color;
	}
}