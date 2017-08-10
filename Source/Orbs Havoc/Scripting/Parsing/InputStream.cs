namespace OrbsHavoc.Scripting.Parsing
{
	using System;
	using Utilities;

	/// <summary>
	///   Provides read-access to a sequence of UTF-16 characters.
	/// </summary>
	internal class InputStream
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="input">The string that should be parsed.</param>
		public InputStream(string input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Input = input;
		}

		/// <summary>
		///   Gets the input string that should be parsed.
		/// </summary>
		public string Input { get; }

		/// <summary>
		///   Gets the current position of the input stream.
		/// </summary>
		internal int Position { get; set; }

		/// <summary>
		///   Gets a value indicating whether the end of the input has been reached.
		/// </summary>
		internal bool EndOfInput => Position >= Input.Length;

		/// <summary>
		///   Returns the current character without changing the stream state.
		/// </summary>
		internal char Peek()
		{
			return EndOfInput ? Char.MaxValue : Input[Position];
		}

		/// <summary>
		///   Returns and consumes the current character.
		/// </summary>
		internal char Read()
		{
			var c = Peek();
			Skip(1);
			return c;
		}

		/// <summary>
		///   Returns a substring of the input with the requested length, starting at the given offset.
		/// </summary>
		/// <param name="position">The first character of the substring.</param>
		/// <param name="length">The length of the substring.</param>
		internal string Substring(int position, int length)
		{
			Assert.ArgumentInRange(position, 0, Int32.MaxValue, nameof(position));
			Assert.ArgumentInRange(length, 0, Int32.MaxValue, nameof(length));
			Assert.That(position + length <= Input.Length, "Buffer overflow");

			return Input.Substring(position, length);
		}

		/// <summary>
		///   Skips the given number of characters.
		/// </summary>
		/// <param name="count">The number of characters that should be skipped.</param>
		internal void Skip(int count)
		{
			Assert.ArgumentInRange(count, 0, Int32.MaxValue, nameof(count));

			for (var i = 0; i < count && !EndOfInput; ++i)
				++Position;
		}

		/// <summary>
		///   Skips zero or more white spaces until the first non-white space character or the end of the input is reached.
		/// </summary>
		internal void SkipWhitespaces()
		{
			while (!EndOfInput && Char.IsWhiteSpace(Peek()))
				Skip(1);
		}

		/// <summary>
		///   Skips all characters that satisfy the given predicate and returns the number of skipped characters.
		/// </summary>
		/// <param name="predicate">The predicate that is used to decide how many characters to skip.</param>
		internal int Skip(Predicate<char> predicate)
		{
			Assert.ArgumentNotNull(predicate, nameof(predicate));

			var count = 0;
			while (!EndOfInput && predicate(Peek()))
			{
				Skip(1);
				++count;
			}
			return count;
		}

		/// <summary>
		///   Checks whether the remainder of the input consists of whitespace only.
		/// </summary>
		internal bool WhiteSpaceUntilEndOfInput()
		{
			for (var i = Position; i < Input.Length; ++i)
			{
				if (!Char.IsWhiteSpace(Input[i]))
					return false;
			}

			return true;
		}
	}
}