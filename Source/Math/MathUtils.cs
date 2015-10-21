﻿// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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
	using Utilities;

	/// <summary>
	///   Provides math utility functions.
	/// </summary>
	public static class MathUtils
	{
		/// <summary>
		///   Epsilon value for float-point equality comparisons.
		/// </summary>
		public const float Epsilon = 10e-6f;

		/// <summary>
		///   Represents a 180 degree rotation or the ratio of the circumference of a circle to its diameter.
		/// </summary>
		public const float Pi = (float)Math.PI;

		/// <summary>
		///   Represents a 360 degree rotation.
		/// </summary>
		public const float TwoPi = (float)Math.PI * 2;

		/// <summary>
		///   Represents the value of Pi divided by two, i.e., a 90 degree rotation.
		/// </summary>
		public const float PiOver2 = (float)Math.PI / 2;

		/// <summary>
		///   Represents the value of Pi divided by four, i.e., a 45 degree rotation.
		/// </summary>
		public const float PiOver4 = (float)Math.PI / 4;

		/// <summary>
		///   Checks whether two float values are equal. If the difference between the two floats is
		///   small enough, they are considered equal.
		/// </summary>
		/// <param name="left">The first value to compare.</param>
		/// <param name="right">The second value to compare.</param>
		public static bool Equals(float left, float right)
		{
			return Math.Abs(left - right) < Epsilon;
		}

		/// <summary>
		///   Scales the given value from the range [0, previousMax] to [0, newMax].
		/// </summary>
		/// <param name="value">The value that should be scaled.</param>
		/// <param name="previousMax">The previous maximum value.</param>
		/// <param name="newMax">The new maximum value.</param>
		public static float Scale(float value, float previousMax, float newMax)
		{
			Assert.ArgumentSatisfies(!Equals(previousMax, 0), nameof(previousMax), "Invalid previous value.");
			return value / previousMax * newMax;
		}

		/// <summary>
		///   Clamps the given value to be in the range [min, max].
		/// </summary>
		/// <param name="value">The value that should be clamped.</param>
		/// <param name="min">The lower bound of the clamped interval.</param>
		/// <param name="max">The upper bound of the clamped interval.</param>
		public static float Clamp(float value, float min, float max)
		{
			if (value < min || max <= min)
				return min;

			if (value > max)
				return max;

			return value;
		}

		/// <summary>
		///   Clamps the given value to be in the range [min, max].
		/// </summary>
		/// <param name="value">The value that should be clamped.</param>
		/// <param name="min">The lower bound of the clamped interval.</param>
		/// <param name="max">The upper bound of the clamped interval.</param>
		public static int Clamp(int value, int min, int max)
		{
			if (value < min || max <= min)
				return min;

			if (value > max)
				return max;

			return value;
		}

		/// <summary>
		///   Computes the angle in radians between the two vectors starting at the given start position and ending in end and
		///   relativeTo.
		/// </summary>
		/// <param name="start">The starting point of the vectors whose angle should be computed.</param>
		/// <param name="end">The end point of the vector whose angle should be computed.</param>
		/// <param name="relativeTo">The vector that represents an angle of 0.</param>
		public static float ComputeAngle(Vector2 start, Vector2 end, Vector2 relativeTo)
		{
			Assert.ArgumentSatisfies(relativeTo != Vector2.Zero, nameof(relativeTo), "The origin is not a valid value.");

			// Calculate the direction vector
			var startToEnd = end - start;

			// The rotation is computed from the dot product of the two direction vectors.
			var rotation = (float)Math.Acos(Vector2.Dot(startToEnd, relativeTo) / startToEnd.Length / relativeTo.Length);

			// Math.Acos can only return results in the first and second quadrant, so make sure we rotate correctly if the rotation
			// actually extends into the third or fourth quadrant
			if (start.Y > end.Y)
				rotation = TwoPi - rotation;

			return rotation;
		}

		/// <summary>
		///   Converts degrees to radians.
		/// </summary>
		/// <param name="degrees">The value in degrees that should be converted.</param>
		public static float DegToRad(float degrees)
		{
			return degrees / 180.0f * Pi;
		}

		/// <summary>
		///   Converts radians to degrees.
		/// </summary>
		/// <param name="radians">The value in radians that should be converted.</param>
		public static float RadToDeg(float radians)
		{
			return radians / Pi * 180.0f;
		}

		/// <summary>
		///   Converts radians to degrees. The result lies within the range of [0; 360[.
		/// </summary>
		/// <param name="radians">The value in radians that should be converted.</param>
		public static float RadToDeg360(float radians)
		{
			return ((radians / Pi * 180.0f) % 360 + 360) % 360;
		}

		/// <summary>
		///   Rounds the given value to nearest integral value.
		/// </summary>
		/// <param name="value">The value to round.</param>
		public static float Round(float value)
		{
			return (float)Math.Round(value);
		}

		/// <summary>
		///   Rounds the given value to nearest integral value.
		/// </summary>
		/// <param name="value">The value to round.</param>
		public static int RoundIntegral(float value)
		{
			return (int)Math.Round(value);
		}

		/// <summary>
		///   Returns the cosine of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Cos(float value)
		{
			return (float)Math.Cos(value);
		}

		/// <summary>
		///   Returns the sine of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Sin(float value)
		{
			return (float)Math.Sin(value);
		}

		/// <summary>
		///   Returns the tangent of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Tan(float value)
		{
			return (float)Math.Tan(value);
		}

		/// <summary>
		///   Returns the angle in radians whose cosine is the given number.
		/// </summary>
		/// <param name="value">A value between -1 and 1.</param>
		public static float Acos(float value)
		{
			Assert.ArgumentInRange(value, -1, 1, nameof(value));
			return (float)Math.Acos(value);
		}

		/// <summary>
		///   Returns the angle in radians whose sine is the given number.
		/// </summary>
		/// <param name="value">A value between -1 and 1.</param>
		public static float Asin(float value)
		{
			return (float)Math.Asin(value);
		}

		/// <summary>
		///   Returns the angle in radians whose tangent is the given number.
		/// </summary>
		/// <param name="value">A value representing a tangent.</param>
		public static float Atan(float value)
		{
			return (float)Math.Atan(value);
		}

		/// <summary>
		///   Returns the angle in radians whose tangent is the quotient of the two given numbers.
		/// </summary>
		/// <param name="y">The Y-coordinate of a point.</param>
		/// <param name="x">The X-coordinate of a point.</param>
		public static float Atan2(float y, float x)
		{
			return (float)Math.Atan2(y, x);
		}

		/// <summary>
		///   Returns the square root of the given value.
		/// </summary>
		/// <param name="value">The value the square root should be returned for.</param>
		public static float Sqrt(float value)
		{
			return (float)Math.Sqrt(value);
		}

		/// <summary>
		///   Returns the cosine of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Cos(double value)
		{
			return (float)Math.Cos(value);
		}

		/// <summary>
		///   Returns the sine of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Sin(double value)
		{
			return (float)Math.Sin(value);
		}

		/// <summary>
		///   Returns the tangent of the given angle.
		/// </summary>
		/// <param name="value">The angle, measured in radians.</param>
		public static float Tan(double value)
		{
			return (float)Math.Tan(value);
		}

		/// <summary>
		///   Returns the angle in radians whose cosine is the given number.
		/// </summary>
		/// <param name="value">A value between -1 and 1.</param>
		public static float Acos(double value)
		{
			Assert.ArgumentInRange(value, -1, 1, nameof(value));
			return (float)Math.Acos(value);
		}

		/// <summary>
		///   Returns the angle in radians whose sine is the given number.
		/// </summary>
		/// <param name="value">A value between -1 and 1.</param>
		public static float Asin(double value)
		{
			return (float)Math.Asin(value);
		}

		/// <summary>
		///   Returns the angle in radians whose tangent is the given number.
		/// </summary>
		/// <param name="value">A value representing a tangent.</param>
		public static float Atan(double value)
		{
			return (float)Math.Atan(value);
		}

		/// <summary>
		///   Returns the angle in radians whose tangent is the quotient of the two given numbers.
		/// </summary>
		/// <param name="y">The Y-coordinate of a point.</param>
		/// <param name="x">The X-coordinate of a point.</param>
		public static float Atan2(double y, double x)
		{
			return (float)Math.Atan2(y, x);
		}

		/// <summary>
		///   Returns the square root of the given value.
		/// </summary>
		/// <param name="value">The value the square root should be returned for.</param>
		public static float Sqrt(double value)
		{
			return (float)Math.Sqrt(value);
		}
	}
}