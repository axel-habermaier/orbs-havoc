﻿// The MIT License (MIT)
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

namespace PointWars.UserInterface
{
	/// <summary>
	///   Identifies the type of a text token.
	/// </summary>
	internal enum TextTokenType
	{
		/// <summary>
		///   Indicates that the token represents of a word, including digits and special characters.
		/// </summary>
		Word,

		/// <summary>
		///   Indicates that the token represents a space character.
		/// </summary>
		Space,

		/// <summary>
		///   Indicates that the token represents a space character that should be replaced by a new line.
		/// </summary>
		WrappedSpace,

		/// <summary>
		///   Indicates that the token represents a new line marker.
		/// </summary>
		NewLine,

		/// <summary>
		///   Indicates that the token represents a wrap marker.
		/// </summary>
		Wrap,

		/// <summary>
		///   Indicates that the sequence represents the end of the text.
		/// </summary>
		EndOfText
	}
}