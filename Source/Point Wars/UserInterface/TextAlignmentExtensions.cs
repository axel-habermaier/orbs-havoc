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
	/// <summary>
	///   Provides extension methods for the text alignment enumeration.
	/// </summary>
	public static class TextAlignmentExtensions
	{
		/// <summary>
		///   Checks whether the given alignment specifies the given flag. We cannot use .Net's Enum.HasFlag method,
		///   as this method boxes the enumeration value each time it is called.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		/// <param name="flag">The flag that should be checked.</param>
		private static bool HasFlag(TextAlignment alignment, TextAlignment flag)
		{
			return (alignment & flag) == flag;
		}

		/// <summary>
		///   Checks whether the text should be left aligned.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsLeftAligned(this TextAlignment alignment)
		{
			return !alignment.IsRightAligned() && !alignment.IsHorizontallyCentered();
		}

		/// <summary>
		///   Checks whether the text should be right aligned.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsRightAligned(this TextAlignment alignment)
		{
			return HasFlag(alignment, TextAlignment.Right);
		}

		/// <summary>
		///   Checks whether the text should be horizontally centered.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsHorizontallyCentered(this TextAlignment alignment)
		{
			return HasFlag(alignment, TextAlignment.Centered);
		}

		/// <summary>
		///   Checks whether the text should be top aligned.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsTopAligned(this TextAlignment alignment)
		{
			return !alignment.IsBottomAligned() && !alignment.IsVerticallyCentered();
		}

		/// <summary>
		///   Checks whether the text should be bottom aligned.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsBottomAligned(this TextAlignment alignment)
		{
			return HasFlag(alignment, TextAlignment.Bottom);
		}

		/// <summary>
		///   Checks whether the text should be vertically centered.
		/// </summary>
		/// <param name="alignment">The alignment value that should be checked.</param>
		public static bool IsVerticallyCentered(this TextAlignment alignment)
		{
			return HasFlag(alignment, TextAlignment.Middle);
		}
	}
}