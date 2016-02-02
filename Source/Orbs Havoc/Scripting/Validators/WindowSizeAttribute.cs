// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using Platform;
	using Utilities;

	/// <summary>
	///   Ensures that the validated value lies within the acceptable bounds of a window size.
	/// </summary>
	public class WindowSizeAttribute : ValidatorAttribute
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