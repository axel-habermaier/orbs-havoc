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

namespace OrbsHavoc.Scripting.Validators
{
	using System;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the given bounds.
	/// </summary>
	public class RangeAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="lowerBound">The lower bound of the range.</param>
		/// <param name="upperBound">The upper bound of the range.</param>
		public RangeAttribute(object lowerBound, object upperBound)
		{
			Assert.ArgumentNotNull(lowerBound, nameof(lowerBound));
			Assert.ArgumentNotNull(upperBound, nameof(upperBound));
			Assert.That(lowerBound.GetType() == upperBound.GetType(), "The types of the lower and upper bounds do not match.");
			Assert.ArgumentSatisfies(lowerBound is IComparable, nameof(lowerBound),
				"The types of the lower and upper bounds must implement IComparable.");

			LowerBound = (IComparable)lowerBound;
			UpperBound = (IComparable)upperBound;
		}

		/// <summary>
		///   Gets the lower bound of the range.
		/// </summary>
		public IComparable LowerBound { get; }

		/// <summary>
		///   Gets the upper bound of the range.
		/// </summary>
		public IComparable UpperBound { get; }

		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			$"The given value does not lie within {TypeRegistry.ToString(LowerBound)} and {TypeRegistry.ToString(UpperBound)}.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description => $"must lie within {TypeRegistry.ToString(LowerBound)} and {TypeRegistry.ToString(UpperBound)}";

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value.GetType() == LowerBound.GetType(), nameof(value), "The value does not have the same type as the bounds.");

			return LowerBound.CompareTo(value) <= 0 && UpperBound.CompareTo(value) >= 0;
		}
	}
}