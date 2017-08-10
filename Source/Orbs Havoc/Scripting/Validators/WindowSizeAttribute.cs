namespace OrbsHavoc.Scripting.Validators
{
	using Platform;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the acceptable bounds of a window size.
	/// </summary>
	internal class WindowSizeAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			$"Only window sizes between {TypeRegistry.ToString(new Size(Window.MinimumSize.Width, Window.MinimumSize.Height))} and " +
			$"{TypeRegistry.ToString(new Size(Window.MaximumSize.Width, Window.MaximumSize.Height))} are supported.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description =>
			$"must lie within {TypeRegistry.ToString(new Size(Window.MinimumSize.Width, Window.MinimumSize.Height))} " +
			$"and {TypeRegistry.ToString(new Size(Window.MaximumSize.Width, Window.MaximumSize.Height))}";

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is Size, nameof(value), $"The value is not of type '{nameof(Size)}'.");

			var size = (Size)value;
			return Window.MinimumSize.Width <= size.Width && Window.MinimumSize.Height <= size.Height &&
				   Window.MaximumSize.Width >= size.Width && Window.MaximumSize.Height >= size.Height;
		}
	}
}