namespace OrbsHavoc.Scripting.Validators
{
	using Utilities;

	/// <summary>
	///     Ensures that the validated string does not consist of whitespaces only.
	/// </summary>
	internal class NotEmptyAttribute : ValidatorAttribute
	{
		/// <summary>
		///     Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage => "The given string cannot consist of whitespaces only.";

		/// <summary>
		///     Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description => "must not be empty";

		/// <summary>
		///     Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is string, nameof(value), "The value must be a string.");

			return !TextString.IsNullOrWhiteSpace((string)value);
		}
	}
}