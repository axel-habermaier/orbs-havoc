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

namespace OrbsHavoc.Utilities
{
	using System;

	/// <summary>
	///   Allows CPU time measurements using C#'s using blocks.
	/// </summary>
	internal unsafe struct TimeMeasurement : IDisposable
	{
		/// <summary>
		///   A pointer to a variable that stores the measured CPU time in milliseconds.
		/// </summary>
		private double* _measuredTime;

		/// <summary>
		///   The starting time of the measurement in seconds.
		/// </summary>
		private double _startTime;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Assert.NotNull(new IntPtr(_measuredTime));
			*_measuredTime = (Clock.GetTime() - _startTime) * 1000;
		}

		/// <summary>
		///   Measures the CPU time until the Dispose method is called.
		/// </summary>
		/// <param name="measuredTime">A pointer to a variable that should store the measured CPU time in milliseconds.</param>
		internal static TimeMeasurement Measure(double* measuredTime)
		{
			Assert.ArgumentNotNull(new IntPtr(measuredTime), nameof(measuredTime));
			return new TimeMeasurement { _startTime = Clock.GetTime(), _measuredTime = measuredTime };
		}
	}
}