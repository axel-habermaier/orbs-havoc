﻿// The MIT License (MIT)
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

namespace OrbsHavoc.Utilities
{
	using System;
	using System.Globalization;
	using System.Numerics;
	using System.Runtime.InteropServices;

	/// <summary>
	///   Represents a size value with the width and height stored as 32-bit floating point values.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Size : IEquatable<Size>
	{
		/// <summary>
		///   The width.
		/// </summary>
		public float Width;

		/// <summary>
		///   The height.
		/// </summary>
		public float Height;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public Size(float width, float height)
			: this()
		{
			Width = width;
			Height = height;
		}

		/// <summary>
		///   Gets the width rounded to the nearest integral value.
		/// </summary>
		public int IntegralWidth => MathUtils.RoundIntegral(Width);

		/// <summary>
		///   Gets the height rounded to the nearest integral value.
		/// </summary>
		public int IntegralHeight => MathUtils.RoundIntegral(Height);

		/// <summary>
		///   Determines whether the specified object is equal to this size instance.
		/// </summary>
		/// <param name="obj">The object to compare with this instance.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			if (obj.GetType() != typeof(Size))
				return false;
			return Equals((Size)obj);
		}

		/// <summary>
		///   Determines whether the specified size instance is equal to this size instance.
		/// </summary>
		/// <param name="other">The size instance to compare with this instance.</param>
		public bool Equals(Size other)
		{
			return MathUtils.Equals(Width, other.Width) && MathUtils.Equals(Height, other.Height);
		}

		/// <summary>
		///   Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		///   A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			var result = Width.GetHashCode();
			result = (result * 397) ^ Height.GetHashCode();
			return result;
		}

		/// <summary>
		///   Tests for equality between two sizes.
		/// </summary>
		/// <param name="left">The first size to compare.</param>
		/// <param name="right">The second size to compare.</param>
		public static bool operator ==(Size left, Size right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Tests for inequality between two sizes.
		/// </summary>
		/// <param name="left">The first size to compare.</param>
		/// <param name="right">The second size to compare.</param>
		public static bool operator !=(Size left, Size right)
		{
			return !(left == right);
		}

		/// <summary>
		///   Computes the sum of the two sizes.
		/// </summary>
		/// <param name="left">The first size to add.</param>
		/// <param name="right">The second size to add.</param>
		public static Size operator +(Size left, Size right)
		{
			return new Size(left.Width + right.Width, left.Height + right.Height);
		}

		/// <summary>
		///   Computes the difference of the two sizes.
		/// </summary>
		/// <param name="left">The first size.</param>
		/// <param name="right">The second size.</param>
		public static Size operator -(Size left, Size right)
		{
			return new Size(left.Width - right.Width, left.Height - right.Height);
		}

		/// <summary>
		///   Scales the size by the given factor.
		/// </summary>
		/// <param name="size">The size that should be scaled.</param>
		/// <param name="factor">The factor that should be applied.</param>
		public static Size operator *(Size size, float factor)
		{
			return new Size(size.Width * factor, size.Height * factor);
		}

		/// <summary>
		///   Scales the size by the given factor.
		/// </summary>
		/// <param name="factor">The factor that should be applied.</param>
		/// <param name="size">The size that should be scaled.</param>
		public static Size operator *(float factor, Size size)
		{
			return new Size(factor * size.Width, factor * size.Height);
		}

		/// <summary>
		///   Divides the size by the given factor.
		/// </summary>
		/// <param name="size">The size that should be divided.</param>
		/// <param name="factor">The scalar value the vector should be divided by.</param>
		public static Size operator /(Size size, float factor)
		{
			return new Size(size.Width / factor, size.Height / factor);
		}

		/// <summary>
		///   Implicitly converts a size to a vector.
		/// </summary>
		/// <param name="size">The size that should be converted.</param>
		public static implicit operator Vector2(Size size)
		{
			return new Vector2(size.Width, size.Height);
		}

		/// <summary>
		///   Returns a string representation of this size instance.
		/// </summary>
		public override string ToString()
		{
			return $"Width: {Width}, Height: {Height}";
		}
	}
}