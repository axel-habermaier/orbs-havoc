﻿namespace OrbsHavoc.Scripting.Validators
{
	using System.Text;
	using Utilities;

	/// <summary>
	///   Ensures that the validated string has a length less than or equal to the maximum allowed length.
	/// </summary>
	internal class MaximumLengthAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="maximum">The maximum allowed length of the string value.</param>
		/// <param name="checkUtf8Length">
		///   Indicates whether instead of checking the number of characters in the string, the size of the UTF8-encoded
		///   representation of the string should be checked checked.
		/// </param>
		public MaximumLengthAttribute(int maximum, bool checkUtf8Length = false)
		{
			Maximum = maximum;
			CheckUtf8Length = checkUtf8Length;
		}

		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			$"The given string exceeds the maximum allowed length of {Maximum} {(CheckUtf8Length ? "UTF-8 bytes" : "characters")}.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description => $"length must not exceed {Maximum} {(CheckUtf8Length ? "UTF-8 bytes" : "characters")}";

		/// <summary>
		///   Gets the maximum allowed length of the string value.
		/// </summary>
		public int Maximum { get; }

		/// <summary>
		///   Gets a value indicating whether instead of checking the number of characters in the string, the size of the
		///   UTF8-encoded representation of the string is checked.
		/// </summary>
		public bool CheckUtf8Length { get; }

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is string, nameof(value), "The value must be a string.");

			int length;
			if (CheckUtf8Length)
				length = Encoding.UTF8.GetByteCount((string)value);
			else
				length = ((string)value).Length;

			return length <= Maximum;
		}
	}
}