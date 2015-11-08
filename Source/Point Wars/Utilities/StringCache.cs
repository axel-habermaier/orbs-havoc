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
	using System.Globalization;
	using Gameplay;

	/// <summary>
	///   Caches commonly used string representations of numbers.
	/// </summary>
	public static class StringCache
	{
		private static readonly string[] Integers = new string[1000];
		private static readonly string[] FrameTimes = new string[2000];
		private static readonly string[] RespawnDelay = new string[100];

		/// <summary>
		///   Initializes the cached strings.
		/// </summary>
		static StringCache()
		{
			for (var i = 0; i < Integers.Length; ++i)
				Integers[i] = i.ToString(CultureInfo.InvariantCulture);

			for (var i = 0; i < FrameTimes.Length; ++i)
				FrameTimes[i] = (i / 100.0).ToString("F2");

			for (var i = 0; i < RespawnDelay.Length; ++i)
				RespawnDelay[i] = (i / 10.0f).ToString("F1");
		}

		/// <summary>
		///   Gets a possibly cached string representation for the given integer.
		/// </summary>
		public static string GetString(int value)
		{
			if (value >= 0 && value < Integers.Length)
				return Integers[value];

			return value.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		///   Gets a possible cached string representation for the given frame time.
		/// </summary>
		public static string GetFrameTimeString(double value)
		{
			if (value >= 0 && value * 100 < FrameTimes.Length)
				return FrameTimes[MathUtils.RoundIntegral((float)(value * 100))];

			return value.ToString("F2");
		}

		/// <summary>
		///   Gets a possible cached string representation for the given frame time.
		/// </summary>
		public static string GetRespawnDelayTimeString(float value)
		{
			if (value >= 0 && value * 10 < FrameTimes.Length)
				return RespawnDelay[MathUtils.RoundIntegral(value * 10)];

			return value.ToString("F1");
		}
	}
}