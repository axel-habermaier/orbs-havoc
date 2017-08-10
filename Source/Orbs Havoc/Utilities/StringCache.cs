namespace OrbsHavoc.Utilities
{
	using System.Globalization;

	/// <summary>
	///   Caches commonly used string representations of numbers.
	/// </summary>
	internal static class StringCache
	{
		private static readonly string[] _integers = new string[1000];
		private static readonly string[] _doubles = new string[2000];

		/// <summary>
		///   Initializes the cached strings.
		/// </summary>
		static StringCache()
		{
			for (var i = 0; i < _integers.Length; ++i)
				_integers[i] = i.ToString(CultureInfo.InvariantCulture);

			for (var i = 0; i < _doubles.Length; ++i)
				_doubles[i] = (i / 100.0).ToString("F2");
		}

		/// <summary>
		///   Gets a possibly cached string representation for the given value.
		/// </summary>
		public static string GetString(int value)
		{
			if (value >= 0 && value < _integers.Length)
				return _integers[value];

			return value.ToString(CultureInfo.InvariantCulture);
		}

		/// <summary>
		///   Gets a possibly cached string representation for the given value.
		/// </summary>
		public static string GetString(double value)
		{
			if (value >= 0 && value * 100 < _doubles.Length)
				return _doubles[MathUtils.RoundIntegral((float)(value * 100))];

			return value.ToString("F2");
		}
	}
}