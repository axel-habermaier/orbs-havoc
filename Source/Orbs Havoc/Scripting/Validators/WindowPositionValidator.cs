// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Scripting.Validators
{
	using System.Numerics;
	using Platform;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the acceptable bounds of a window position.
	/// </summary>
	public class WindowPositionAttribute : ValidatorAttribute
	{
		/// <summary>
		///   Gets an error message that describes a validation error.
		/// </summary>
		public override string ErrorMessage =>
			$"Only screen positions between ({-Window.MaximumSize.Width},{-Window.MaximumSize.Height}) " +
			$"and ({Window.MaximumSize.Width},{Window.MaximumSize.Height}) are supported.";

		/// <summary>
		///   Gets a description of the validation performed by the validator.
		/// </summary>
		public override string Description =>
			$"must lie within ({-Window.MaximumSize.Width},{-Window.MaximumSize.Height}) " +
			$"and ({Window.MaximumSize.Width},{Window.MaximumSize.Height})";

		/// <summary>
		///   Validates the given value, returning true to indicate that validation succeeded.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		public override bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));
			Assert.ArgumentSatisfies(value is Vector2, nameof(value), "The value is not of type 'Vector2'.");

			var position = (Vector2)value;
			return -Window.MaximumSize.Width <= position.X && -Window.MaximumSize.Height <= position.Y &&
				   Window.MaximumSize.Width >= position.X && Window.MaximumSize.Height >= position.Y;
		}
	}
}