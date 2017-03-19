// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using System.Text;
	using Utilities;

	/// <summary>
	///   Ensures that the validated string has a length less than or equal to the maximum allowed length.
	/// </summary>
	public class MaximumLengthAttribute : ValidatorAttribute
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