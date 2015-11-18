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

namespace OrbsHavoc.Scripting.Parsing
{
	using System;
	using JetBrains.Annotations;
	using Utilities;

	/// <summary>
	///   Provides information about parsing errors.
	/// </summary>
	public class ParseException : Exception
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		[StringFormatMethod("format")]
		public ParseException(InputStream inputStream, string format, params object[] args)
			: base(String.Format(format, args))
		{
			Assert.ArgumentNotNull(inputStream, nameof(inputStream));

			InputStream = inputStream;
		}

		/// <summary>
		///   Gets the input stream the exception was raised for.
		/// </summary>
		public InputStream InputStream { get; }

		/// <summary>
		///   Gets zero-based position within the input where the parse error occurred.
		/// </summary>
		public int Position => InputStream.State.Position;

		/// <summary>
		///   Gets the input that was parsed.
		/// </summary>
		public string Input => InputStream.Input;
	}
}