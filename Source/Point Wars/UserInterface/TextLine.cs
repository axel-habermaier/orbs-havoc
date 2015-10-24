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

namespace PointWars.UserInterface
{
	using System.Numerics;
	using Utilities;

	/// <summary>
	///   Represents a line of text.
	/// </summary>
	internal struct TextLine
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="left">The position of the line's left edge.</param>
		/// <param name="top">The position of the line's top edge.</param>
		/// <param name="lineHeight">The height of the line.</param>
		public TextLine(float left, float top, float lineHeight)
			: this()
		{
			Area = new Rectangle(left, top, 0, lineHeight);
			FirstCharacter = -1;
			LastCharacter = -1;
		}

		/// <summary>
		///   Gets the area occupied by the text line.
		/// </summary>
		public Rectangle Area { get; private set; }

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
		///   Appends the given sequence to the line.
		/// </summary>
		/// <param name="sequence">The sequence that should be appended.</param>
		/// <param name="width">The new width of the line.</param>
		public void Append(TextSequence sequence, float width)
		{
			if (FirstCharacter == -1)
				FirstCharacter = sequence.FirstCharacter;

			LastCharacter = sequence.LastCharacter;
			Area = new Rectangle(Area.Left, Area.Top, width, Area.Height);
		}

		/// <summary>
		///   Adds the given offsets to the position of the line.
		/// </summary>
		/// <param name="offset">The offset that should be applied to the line's position.</param>
		public void Offset(Vector2 offset)
		{
			Area = Area.Offset(offset);
		}
	}
}