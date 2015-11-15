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

namespace OrbsHavoc.UserInterface
{
	/// <summary>
	///   Represents a line of text.
	/// </summary>
	internal struct TextLine
	{
		/// <summary>
		///   Gets the with of the text line.
		/// </summary>
		public float Width { get; private set; }

		/// <summary>
		///   Gets the index of the first character of the text line. A value of -1 indicates that the index has not yet
		///   been determined.
		/// </summary>
		public int FirstCharacter { get; private set; }

		/// <summary>
		///   Gets the index of the last character of the text line. A value of -1 indicates that the index has not yet
		///   been determined.
		/// </summary>
		public int LastCharacter { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the line is invalid.
		/// </summary>
		public bool IsInvalid => FirstCharacter == -1 || LastCharacter == -1 || FirstCharacter >= LastCharacter;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		public static TextLine Create()
		{
			return new TextLine
			{
				FirstCharacter = -1,
				LastCharacter = -1
			};
		}

		/// <summary>
		///   Appends the given sequence to the line.
		/// </summary>
		/// <param name="sequence">The sequence that should be appended.</param>
		/// <param name="width">The new width of the line.</param>
		public void Append(TextSequence sequence, float width)
		{
			if (FirstCharacter == -1)
				FirstCharacter = sequence.FirstCharacter;

			LastCharacter = sequence.LastCharacter;
			Width = width;
		}
	}
}