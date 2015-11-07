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

namespace PointWars.Utilities
{
	using System;
	using System.Collections;
	using Platform.Logging;

	/// <summary>
	///   A random number generator that uses the FastRand algorithm to generate random values.
	/// </summary>
	internal static class RandomNumberGenerator
	{
		/// <summary>
		///   The internal state that is used to determine the next random value.
		/// </summary>
		private static int _state = 1;

		/// <summary>
		///   Gets the next random integer value.
		/// </summary>
		public static int NextInteger()
		{
			_state = 214013 * _state + 2531011;
			return (_state >> 16) & 0x7FFF;
		}

		/// <summary>
		///   Gets the next random integer value which is greater than zero and less than or equal to
		///   the specified maximum value.
		/// </summary>
		/// <param name="max">The maximum random integer value to return.</param>
		public static int NextInteger(int max)
		{
			return (int)(max * NextSingle());
		}

		/// <summary>
		///   Gets the next random integer between the specified minimum and maximum values.
		/// </summary>
		/// <param name="min">The inclusive minimum value.</param>
		/// <param name="max">The v maximum value.</param>
		public static int NextInteger(int min, int max)
		{
			return (int)((max - min) * NextSingle()) + min;
		}

		/// <summary>
		///   Gets the next random index.
		/// </summary>
		/// <param name="collection">The collection the index should be retrieved for.</param>
		public static int NextIndex(ICollection collection)
		{
			return NextInteger(0, collection.Count - 1);
		}

		/// <summary>
		///   Gets the next random integer between the specified minimum and maximum values.
		/// </summary>
		/// <param name="min">The inclusive minimum value.</param>
		/// <param name="max">The inclusive maximum value.</param>
		public static byte NextByte(byte min, byte max)
		{
			return (byte)((max - min) * NextSingle() + min);
		}

		/// <summary>
		///   Gets the next random single value in the range [0,1].
		/// </summary>
		public static float NextSingle()
		{
			return NextInteger() / (float)Int16.MaxValue;
		}

		/// <summary>
		///   Gets the next random single value which is greater than zero and less than or equal to
		///   the specified maximum value.
		/// </summary>
		/// <param name="max">The maximum random single value to return.</param>
		public static float NextSingle(float max)
		{
			return max * NextSingle();
		}

		/// <summary>
		///   Gets the next random single value between the specified minimum and maximum values.
		/// </summary>
		/// <param name="min">The inclusive minimum value.</param>
		/// <param name="max">The inclusive maximum value.</param>
		public static float NextSingle(float min, float max)
		{
			return ((max - min) * NextSingle()) + min;
		}

		/// <summary>
		///   Gets the next random three-dimensional unit vector.
		/// </summary>
		/// <param name="vector">A pointer to an array of three floating point values where the resulting unit vector should be stored.</param>
		public static unsafe void NextUnitVector(float* vector)
		{
			var theta = NextSingle(0.0f, MathUtils.TwoPi);
			var r = MathUtils.Sqrt(NextSingle(0.0f, 1.0f));
			var z = MathUtils.Sqrt(1.0f - r * r) * (NextSingle() > 0.5f ? -1.0f : 1.0f);

			vector[0] = r * MathUtils.Cos(theta);
			vector[1] = r * MathUtils.Sin(theta);
			vector[2] = z;
		}
	}
}