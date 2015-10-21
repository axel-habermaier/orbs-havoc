// The MIT License (MIT)
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
	/// <summary>
	///   Configures a particle property of an emitter.
	/// </summary>
	/// <typeparam name="T">The type of the configured property.</typeparam>
	public struct Range<T>
	{
		/// <summary>
		///   The inclusive lower bound of the range. For multi-component values such as vectors or colors, this
		///   is the lower bound value per component.
		/// </summary>
		public readonly T LowerBound;

		/// <summary>
		///   The inclusive upper bound of the range. For multi-component values such as vectors or colors, this
		///   is the upper bound value per component.
		/// </summary>
		public readonly T UpperBound;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="lowerBoundValue">
		///   The inclusive lower bound of the range. For multi-component values such as vectors or colors, this
		///   is the lower bound value per component.
		/// </param>
		/// <param name="upperBoundValue">
		///   The inclusive upper bound of the range. For multi-component values such as vectors or colors, this
		///   is the upper bound value per component.
		/// </param>
		public Range(T lowerBoundValue, T upperBoundValue)
		{
			LowerBound = lowerBoundValue;
			UpperBound = upperBoundValue;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="bounds">
		///   The lower and upper bound of the range. For multi-component values such as vectors or colors, this
		///   is the lower and upper bound per component.
		/// </param>
		public Range(T bounds)
		{
			LowerBound = bounds;
			UpperBound = bounds;
		}
	}
}