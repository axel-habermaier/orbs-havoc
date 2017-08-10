namespace OrbsHavoc.Utilities
{
	/// <summary>
	///   Represents a range of values.
	/// </summary>
	/// <typeparam name="T">The type of the values within the range.</typeparam>
	internal struct Range<T>
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

		/// <summary>
		///   Creates a range that consists of the given value only. For multi-component values such as vectors or colors, this
		///   is the lower and upper bound per component.
		/// </summary>
		/// <param name="value">The single value of the range.</param>
		public static implicit operator Range<T>(T value)
		{
			return new Range<T>(value);
		}
	}
}