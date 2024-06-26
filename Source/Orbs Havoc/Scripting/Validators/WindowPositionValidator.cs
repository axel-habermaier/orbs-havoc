﻿namespace OrbsHavoc.Scripting.Validators
{
	using System.Numerics;
	using Platform;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the acceptable bounds of a window position.
	/// </summary>
	internal class WindowPositionAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			$"Only window positions between {TypeRegistry.ToString(new Vector2(-Window.MaximumSize.Width, -Window.MaximumSize.Height))} " +
			$"and {TypeRegistry.ToString(new Vector2(Window.MaximumSize.Width, Window.MaximumSize.Height))} are supported.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description =>
			$"must lie within {TypeRegistry.ToString(new Vector2(-Window.MaximumSize.Width, -Window.MaximumSize.Height))} " +
			$"and {TypeRegistry.ToString(new Vector2(Window.MaximumSize.Width, Window.MaximumSize.Height))}";

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is Vector2, nameof(value), $"The value is not of type '{nameof(Vector2)}'.");

			var position = (Vector2)value;
			return -Window.MaximumSize.Width <= position.X && -Window.MaximumSize.Height <= position.Y &&
				   Window.MaximumSize.Width >= position.X && Window.MaximumSize.Height >= position.Y;
		}
	}
}