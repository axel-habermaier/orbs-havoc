namespace OrbsHavoc.Scripting.Validators
{
	using Platform;
	using Utilities;

	/// <summary>
	///   Ensures that the validated string is a file name.
	/// </summary>
	public class FileNameAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			"The given string is not a valid file name. It either contains a path specifier such as '/', or it contains illegal characters.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description => "must be a valid file name";

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is string, nameof(value), "The value must be a string.");

			return UserFile.IsValidFileName((string)value);
		}
	}
}