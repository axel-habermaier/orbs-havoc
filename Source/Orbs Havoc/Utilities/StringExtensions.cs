﻿namespace OrbsHavoc.Utilities
{
	using System;

	internal static class StringExtensions
	{
		public static string Truncate(this string value, int maxLength)
		{
			Assert.ArgumentNotNull(value, nameof(value));

			if (String.IsNullOrEmpty(value))
				return value;

			return value.Length <= maxLength ? value : value.Substring(0, maxLength);
		}

		public static string EnsureEndsWithDot(this string str)
		{
			return !str.EndsWith(".") && !str.EndsWith("!") && !str.EndsWith("?") ? str + "." : str;
		}
	}
}