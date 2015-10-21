// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace PointWars.Scripting.Validators
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
		public override string ErrorMessage
			=> "The given string is not a valid file name. It either contains a path specifier such as '/', or it contains illegal characters.";

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

			return FileSystem.IsValidFileName((string)value);
		}
	}
}