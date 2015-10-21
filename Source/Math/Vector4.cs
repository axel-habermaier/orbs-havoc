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

namespace PointWars.Math
{
	using System;
	using System.Globalization;
	using System.Runtime.InteropServices;
	using JetBrains.Annotations;

	/// <summary>
	///   Represents a four-component vector.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4 : IEquatable<Vector4>, IFormattable
	{
		/// <summary>
		///   A vector with all components set to zero.
		/// </summary>
		public static readonly Vector4 Zero = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

		/// <summary>
		///   The X-component of the vector.
		/// </summary>
		public float X;

		/// <summary>
		///   The Y-component of the vector.
		/// </summary>
		public float Y;

		/// <summary>
		///   The Z-component of the vector.
		/// </summary>
		public float Z;

		/// <summary>
		///   The W-component of the vector.
		/// </summary>
		public float W;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="x">The X-component of the vector.</param>
		/// <param name="y">The Y-component of the vector.</param>
		/// <param name="z">The Z-component of the vector.</param>
		/// <param name="w">The W-component of the vector.</param>
		public Vector4(float x, float y, float z, float w = 1.0f)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="value">The value of the X, Y, Z, and W components of the vector.</param>
		public Vector4(float value)
		{
			X = value;
			Y = value;
			Z = value;
			W = value;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="vector">The X, Y, and Z components of the vector.</param>
		/// <param name="w">The W-component of the vector.</param>
		public Vector4(Vector3 vector, float w = 1.0f)
		{
			X = vector.X;
			Y = vector.Y;
			Z = vector.Z;
			W = w;
		}

		/// <summary>
		///   Determines whether the given vector is equal to this vector.
		/// </summary>
		/// <param name="other">The other vector to compare with this vector.</param>
		public bool Equals(Vector4 other)
		{
			return MathUtils.Equals(X, other.X) &&
				   MathUtils.Equals(Y, other.Y) &&
				   MathUtils.Equals(Z, other.Z) &&
				   MathUtils.Equals(W, other.W);
		}

		/// <summary>
		///   Determines whether the specified object is equal to this vector.
		/// </summary>
		/// <param name="value">The object to compare with this vector.</param>
		public override bool Equals(object value)
		{
			if (value == null)
				return false;

			if (!ReferenceEquals(value.GetType(), typeof(Vector4)))
				return false;

			return Equals((Vector4)value);
		}

		/// <summary>
		///   Returns a hash code for this vector.
		/// </summary>
		public override int GetHashCode()
		{
			return X.GetHashCode() + Y.GetHashCode() + Z.GetHashCode() + W.GetHashCode();
		}

		/// <summary>
		///   Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The format to use.</param>
		/// <param name="formatProvider">The provider to use to format the value</param>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			return String.Format(formatProvider, String.Format("X: {{0:{0}}}, Y: {{1:{0}}}, Z: {{2:{0}}}, W: {{3:{0}}}", format), X, Y, Z, W);
		}

		/// <summary>
		///   Returns a string representation of this vector.
		/// </summary>
		public override string ToString()
		{
			return String.Format(CultureInfo.InvariantCulture, "X: {0}, Y: {1}, Z: {2}, W: {3}", X, Y, Z, W);
		}

		/// <summary>
		///   Tests for equality between two vectors.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		public static bool operator ==(Vector4 left, Vector4 right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Tests for inequality between two vectors.
		/// </summary>
		/// <param name="left">The first vector to compare.</param>
		/// <param name="right">The second vector to compare.</param>
		public static bool operator !=(Vector4 left, Vector4 right)
		{
			return !(left == right);
		}

		/// <summary>
		///   Performs a vector addition.
		/// </summary>
		/// <param name="left">The first vector to add.</param>
		/// <param name="right">The second vector to add.</param>
		public static Vector4 operator +(Vector4 left, Vector4 right)
		{
			return new Vector4(left.X + right.X, left.Y + right.Y, left.Z + right.Z, left.W + right.W);
		}

		/// <summary>
		///   Performs a vector subtraction.
		/// </summary>
		/// <param name="left">The first vector.</param>
		/// <param name="right">The second vector.</param>
		public static Vector4 operator -(Vector4 left, Vector4 right)
		{
			return new Vector4(left.X - right.X, left.Y - right.Y, left.Z - right.Z, left.W - right.W);
		}

		/// <summary>
		///   Negates the components of a vector.
		/// </summary>
		/// <param name="vector">The vector whose components should be negated.</param>
		public static Vector4 operator -(Vector4 vector)
		{
			return new Vector4(-vector.X, -vector.Y, -vector.Z, -vector.W);
		}

		/// <summary>
		///   Multiplies a vector with a scalar value.
		/// </summary>
		/// <param name="vector">The vector that should be multiplied.</param>
		/// <param name="factor">The scalar value the vector should be multiplied with.</param>
		public static Vector4 operator *(Vector4 vector, float factor)
		{
			return new Vector4(vector.X * factor, vector.Y * factor, vector.Z * factor, vector.W * factor);
		}

		/// <summary>
		///   Multiplies a vector with a scalar value.
		/// </summary>
		/// <param name="factor">The scalar value the vector should be multiplied with.</param>
		/// <param name="vector">The vector that should be multiplied.</param>
		public static Vector4 operator *(float factor, Vector4 vector)
		{
			return vector * factor;
		}

		/// <summary>
		///   Divides the vector by a scalar value.
		/// </summary>
		/// <param name="vector">The vector that should be divided.</param>
		/// <param name="factor">The scalar value the vector should be divided by.</param>
		public static Vector4 operator /(Vector4 vector, float factor)
		{
			return new Vector4(vector.X / factor, vector.Y / factor, vector.Z / factor, vector.W / factor);
		}

		/// <summary>
		///   Gets the length of the vector.
		/// </summary>
		public float Length => (float)Math.Sqrt(LengthSquared);

		/// <summary>
		///   Gets the squared length of the vector.
		/// </summary>
		public float LengthSquared => X * X + Y * Y + Z * Z + W * W;

		/// <summary>
		///   Constructs a new vector instance that is normalized to a length of 1, but still points into the same direction.
		/// </summary>
		[Pure]
		public Vector4 Normalize()
		{
			var length = Length;
			if (length < MathUtils.Epsilon)
				length = MathUtils.Epsilon;

			return new Vector4(X / length, Y / length, Z / length, W / length);
		}

		/// <summary>
		///   Applies the given transformation matrix to the vector.
		/// </summary>
		/// <param name="vector">The vector that should be transformed.</param>
		/// <param name="matrix">The transformation matrix that should be applied.</param>
		public static Vector4 Transform(ref Vector4 vector, ref Matrix matrix)
		{
			return new Vector4(
				matrix.M11 * vector.X + matrix.M21 * vector.Y + matrix.M31 * vector.Z + matrix.M41 * vector.W,
				matrix.M12 * vector.X + matrix.M22 * vector.Y + matrix.M32 * vector.Z + matrix.M42 * vector.W,
				matrix.M13 * vector.X + matrix.M23 * vector.Y + matrix.M33 * vector.Z + matrix.M43 * vector.W,
				matrix.M14 * vector.X + matrix.M24 * vector.Y + matrix.M34 * vector.Z + matrix.M44 * vector.W);
		}

		/// <summary>
		///   Applies the given transformation matrix to the vector.
		/// </summary>
		/// <param name="vector">The vector that should be transformed.</param>
		/// <param name="matrix">The transformation matrix that should be applied.</param>
		/// <param name="result">The vector that stores the result of the transformation.</param>
		public static void Transform(ref Vector4 vector, ref Matrix matrix, out Vector4 result)
		{
			result = new Vector4(
				matrix.M11 * vector.X + matrix.M21 * vector.Y + matrix.M31 * vector.Z + matrix.M41 * vector.W,
				matrix.M12 * vector.X + matrix.M22 * vector.Y + matrix.M32 * vector.Z + matrix.M42 * vector.W,
				matrix.M13 * vector.X + matrix.M23 * vector.Y + matrix.M33 * vector.Z + matrix.M43 * vector.W,
				matrix.M14 * vector.X + matrix.M24 * vector.Y + matrix.M34 * vector.Z + matrix.M44 * vector.W);
		}
	}
}