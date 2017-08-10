﻿namespace OrbsHavoc.Scripting.Validators
{
	using System;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the given bounds.
	/// </summary>
	internal class RangeAttribute : ValidatorAttribute
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