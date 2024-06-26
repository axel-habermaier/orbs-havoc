﻿namespace OrbsHavoc.UserInterface
{
	using Utilities;

	/// <summary>
	///   Represents a sequence of text, i.e., sequence of letters and digits or special characters, as well as
	///   individual new line tokens or spaces.
	/// </summary>
	internal struct TextSequence
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="character">The index of the one and only character of the sequence.</param>
		public TextSequence(int character)
			: this()
		{
			FirstCharacter = character;
			LastCharacter = character + 1;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="text">The text the sequence should be created for.</param>
		/// <param name="index">The index of the first character in the sequence.</param>
		public TextSequence(TextString text, int index)
			: this()
		{
			FirstCharacter = index;
			LastCharacter = FindFirstWhitespace(text, index + 1);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="text">The text for which the sequence should be created.</param>
		public TextSequence(TextString text)
			: this()
		{
			FirstCharacter = 0;
			LastCharacter = text.Length;
		}

		/// <summary>
		///   Gets the index of the first character of the sequence.
		/// </summary>
		public int FirstCharacter { get; private set; }

		/// <summary>
		///   Gets the index one past the last character of the sequence.
		/// </summary>
		public int LastCharacter { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the sequence is invalid and should be ignored. Some operations
		///   might produce invalid sequences with zero or less characters.
		/// </summary>
		public bool IsInvalid => FirstCharacter >= LastCharacter;

		/// <summary>
		///   Computes the width of the sequence.
		/// </summary>
		/// <param name="font">The font that should be used to determine the width of sequence.</param>
		/// <param name="text">The text the sequence was created for.</param>
		public float ComputeWidth(Font font, TextString text)
		{
			if (text.Length == 0)
				return 0;

			return font.MeasureWidth(text, FirstCharacter, LastCharacter);
		}

		/// <summary>
		///   Splits the given sequence into two sequences, with the first split part's width being
		///   less than or equal to the given allowed width.
		/// </summary>
		/// <param name="font">The font that should be used to determine the width of sequence.</param>
		/// <param name="text">The text the sequence was created for.</param>
		/// <param name="allowedWidth">The maximum allowed with for the first split part.</param>
		public (TextSequence part1, TextSequence part2) Split(Font font, TextString text, float allowedWidth)
		{
			var splitIndex = FindSplitIndex(font, text, allowedWidth);

			var part1 = new TextSequence { FirstCharacter = FirstCharacter, LastCharacter = splitIndex };
			var part2 = new TextSequence { FirstCharacter = splitIndex, LastCharacter = LastCharacter };

			return (part1, part2);
		}

		/// <param name="font">The font that should be used to determine the width of sequence.</param>
		/// <param name="text">The text the sequence was created for.</param>
		/// <param name="allowedWidth">The maximum allowed with for the first split part.</param>
		private int FindSplitIndex(Font font, TextString text, float allowedWidth)
		{
			var splitIndex = FirstCharacter;
			var width = 0.0f;

			for (; splitIndex < LastCharacter; ++splitIndex)
			{
				width += font.MeasureWidth(text, splitIndex, splitIndex + 1);

				if (width > allowedWidth)
					break;
			}

			return splitIndex;
		}

		/// <summary>
		///   Searches the text, starting at the given offset, for the first whitespace character and
		///   returns its index.
		/// </summary>
		/// <param name="text">The text that should be searched.</param>
		/// <param name="offset">The offset into the text where the search should begin.</param>
		private static int FindFirstWhitespace(TextString text, int offset)
		{
			while (offset < text.Length && text[offset] != '\n' && text[offset] != ' ')
				offset++;

			return offset;
		}
	}
}