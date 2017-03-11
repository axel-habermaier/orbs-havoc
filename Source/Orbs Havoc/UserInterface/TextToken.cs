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

namespace OrbsHavoc.UserInterface
{
	using Utilities;

	/// <summary>
	///   Represents a text marker or text sequence.
	/// </summary>
	internal struct TextToken
	{
		/// <summary>
		///   The end of text token.
		/// </summary>
		public static readonly TextToken EndOfText = new TextToken { Type = TextTokenType.EndOfText };

		/// <summary>
		///   The wrap token.
		/// </summary>
		public static readonly TextToken Wrap = new TextToken { Type = TextTokenType.Wrap };

		/// <summary>
		///   Gets the type of the token.
		/// </summary>
		public TextTokenType Type { get; private set; }

		/// <summary>
		///   Gets the text sequence corresponding to token. This property is only valid
		///   for tokens of type Word, NewLine, and Space.
		/// </summary>
		public TextSequence Sequence { get; private set; }

		/// <summary>
		///   Gets the next token for the given text, starting at the given offset.
		/// </summary>
		/// <param name="text">The text for which the next token should be returned.</param>
		/// <param name="offset">The offset into the text string.</param>
		/// <returns></returns>
		public static TextToken Next(TextString text, int offset)
		{
			Assert.ArgumentSatisfies(offset >= 0, nameof(offset), "Out of bounds.");

			if (offset >= text.Length)
				return EndOfText;

			var character = text[offset];
			if (character == '\n')
				return CreateNewLineToken(offset);

			if (character == ' ')
				return CreateSpaceToken(offset);

			return CreateWordToken(text, offset);
		}

		/// <summary>
		///   Creates a new line token.
		/// </summary>
		/// <param name="index">The index of the new line character in the text.</param>
		public static TextToken CreateNewLineToken(int index)
		{
			return new TextToken { Type = TextTokenType.NewLine, Sequence = new TextSequence(index) };
		}

		/// <summary>
		///   Creates a space token.
		/// </summary>
		/// <param name="index">The index of the space character in the text.</param>
		public static TextToken CreateSpaceToken(int index)
		{
			return new TextToken { Type = TextTokenType.Space, Sequence = new TextSequence(index) };
		}

		/// <summary>
		///   Creates a wrapped space token.
		/// </summary>
		/// <param name="index">The index of the space character in the text.</param>
		public static TextToken CreateWrappedSpaceToken(int index)
		{
			return new TextToken { Type = TextTokenType.WrappedSpace, Sequence = new TextSequence(index) };
		}

		/// <summary>
		///   Creates a word token.
		/// </summary>
		/// <param name="text">The text for which the token should be created.</param>
		/// <param name="index">The index of the space character in the text.</param>
		public static TextToken CreateWordToken(TextString text, int index)
		{
			return new TextToken { Type = TextTokenType.Word, Sequence = new TextSequence(text, index) };
		}

		/// <summary>
		///   Splits the given token into two tokens, with the first split part's width being
		///   less than or equal to the given allowed width. Only allowed on word tokens.
		/// </summary>
		/// <param name="font">The font that should be used to determine the width of tokens.</param>
		/// <param name="text">The text the token was created for.</param>
		/// <param name="allowedWidth">The maximum allowed with for the first split part.</param>
		public (TextToken part1, TextToken part2) Split(Font font, TextString text, float allowedWidth)
		{
			Assert.That(Type == TextTokenType.Word, "Wrong token type.");

			var (sequence1, sequence2) = Sequence.Split(font, text, allowedWidth);

			var part1 = new TextToken { Type = TextTokenType.Word, Sequence = sequence1 };
			var part2 = new TextToken { Type = TextTokenType.Word, Sequence = sequence2 };

			return (part1, part2);
		}
	}
}