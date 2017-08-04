namespace OrbsHavoc.Utilities
{
	using System;

	public static class StringExtensions
	{
		public static string Truncate(this string value, int maxLength)
		{
			Assert.ArgumentNotNull(value, nameof(value));

			if (String.IsNullOrEmpty(value))
				return value;

			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}
	}
}