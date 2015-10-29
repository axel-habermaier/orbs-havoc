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

namespace PointWars.UserInterface.Controls
{
	using System;
	using System.Numerics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   An indicator that shows the current insertion and deletion position within an editable text.
	/// </summary>
	internal struct Caret
	{
		/// <summary>
		///   The blinking frequency of the caret (in times per second).
		/// </summary>
		private const int BlinkingFrequency = 2;

		/// <summary>
		///   The text box that uses the caret.
		/// </summary>
		private readonly TextBox _textBox;

		/// <summary>
		///   The clock that is used to determine whether the caret should be visible.
		/// </summary>
		private Clock _clock;

		/// <summary>
		///   The logical position of the caret, corresponding to an index of a character of the editable text.
		/// </summary>
		private int _position;

		/// <summary>
		///   The text contained in the text control during the previous caret operation.
		/// </summary>
		private string _text;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="textBox">The text box that uses the caret.</param>
		public Caret(TextBox textBox)
			: this()
		{
			Assert.ArgumentNotNull(textBox, nameof(textBox));

			_clock = new Clock();
			_textBox = textBox;
		}

		/// <summary>
		///   Gets or sets the logical position of the cursor. The position is always clamped into the valid range.
		/// </summary>
		public int Position
		{
			get { return _position; }
			set
			{
				using (var text = GetText())
					_position = Math.Min(text.Length, value);

				_position = Math.Max(0, _position);
				Show();
			}
		}

		/// <summary>
		///   Ensures that the caret is visible, even though it might in the hidden phase of its blinking interval at the moment.
		/// </summary>
		internal void Show()
		{
			_clock.Reset();
		}

		/// <summary>
		///   Gets the text of the text control.
		/// </summary>
		private TextString GetText()
		{
			Assert.ArgumentNotNull(_textBox, nameof(_textBox));
			Assert.ArgumentNotNull(_textBox.Text, nameof(_textBox.Text));

			if (_text != _textBox.Text)
			{
				_text = _textBox.Text;
				Position = 0;
			}

			return TextString.Create(_textBox.Text);
		}

		/// <summary>
		///   Sets the text of the text control.
		/// </summary>
		/// <param name="text">The text that should be set on the text control.</param>
		private void SetText(string text)
		{
			Assert.ArgumentNotNull(text, nameof(text));

			_text = text;
			_textBox.Text = text;
		}

		/// <summary>
		///   Moves the caret by the given delta.
		/// </summary>
		/// <param name="delta">The delta by which the caret should be moved.</param>
		public void Move(int delta)
		{
			Position += delta;
		}

		/// <summary>
		///   Moves the caret to the beginning of the text.
		/// </summary>
		public void MoveToBeginning()
		{
			Position = 0;
		}

		/// <summary>
		///   Moves the caret to the end of the text.
		/// </summary>
		public void MoveToEnd()
		{
			using (var text = GetText())
				Position = text.Length;
		}

		/// <summary>
		///   Moves to the end of the text if the text has been changed manually without using the cursor.
		/// </summary>
		public void MoveToEndIfTextChanged()
		{
			if (_text != _textBox.Text)
				MoveToEnd();
		}

		/// <summary>
		///   Inserts the given character at the current caret position.
		/// </summary>
		/// <param name="c">The character that should be inserted.</param>
		public void InsertCharacter(char c)
		{
			// Ignore non-ASCII printable characters
			if (c < 32 || c > 255)
				return;

			using (var text = GetText())
			{
				var length = text.Length;
				var insertIndex = text.MapToSource(_position);
				SetText(text.SourceString.Insert(insertIndex, c.ToString()));

				using (var updatedText = GetText())
				{
					// Due to the insertion, less characters might now be visible and we have to adjust the caret position accordingly. To do that,
					// we calculate the new text position of the inserted character and use the delta to adjust the caret. Then there are two cases:
					// If we inserted a character that completes a color specifier following the current position, we should not move the caret.
					// Otherwise, we move back by the delta and advance to the next character.
					var newPosition = updatedText.MapToText(insertIndex);
					if (updatedText.Length < length && newPosition == _position)
						Move(0);
					else
						Move(newPosition - _position + 1);
				}
			}
		}

		/// <summary>
		///   Removes the character at the current caret position (similar to pressing the Delete key in a Windows text box).
		/// </summary>
		public void RemoveCurrentCharacter()
		{
			using (var text = GetText())
			{
				if (_position >= text.SourceLength)
					return;

				// Calculate the position in the source text; as multiple source characters can be mapped onto the current position,
				// this ensures that the first mapped character is deleted
				var position = _position;
				if (position != 0)
					position = text.MapToSource(position - 1) + 1;

				if (position >= text.SourceLength)
					return;

				SetText(text.SourceString.Remove(position, 1));
			}

			// The caret position doesn't change, but we have to ensure that it does not get out of bounds
			Move(0);
		}

		/// <summary>
		///   Removes the character that is immediately before the current caret position (similar to
		///   pressing the Backspace key in a Windows text box).
		/// </summary>
		public void RemovePreviousCharacter()
		{
			using (var text = GetText())
			{
				if (_position <= 0 && text.SourceLength == 0)
					return;

				// Calculate the position in the source text; as multiple source characters can be mapped onto the previous position,
				// this ensures that the last mapped character is deleted
				int removalIndex;
				if (_position == text.Length)
					removalIndex = text.SourceLength - 1;
				else
					removalIndex = text.MapToSource(_position) - 1;

				if (removalIndex < 0)
					return;

				var sourceString = text.SourceString.Remove(removalIndex, 1);
				SetText(sourceString);

				using (var updatedText = TextString.Create(sourceString))
				{
					// Due to the deletion, more characters might now be visible and we have to adjust the caret position accordingly. To do that,
					// we calculate the new text position for the removed index and use the delta to adjust the caret
					var newPosition = updatedText.MapToText(removalIndex);
					Move(newPosition - _position);
				}
			}
		}

		/// <summary>
		///   Draws the caret.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing the caret.</param>
		/// <param name="position">The position of the caret's top left corner.</param>
		/// <param name="lineHeight">The height of a line.</param>
		/// <param name="color">The color the caret should be drawn in.</param>
		public void Draw(SpriteBatch spriteBatch, Vector2 position, int lineHeight, Color color)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			// Show and hide the caret depending on the frequency and offset
			if (((int)Math.Round(_clock.Seconds * BlinkingFrequency)) % 2 != 0)
				return;

			var top = new Vector2(position.X, position.Y - 1);
			var bottom = new Vector2(0, lineHeight) + top;

			++spriteBatch.Layer;
			spriteBatch.DrawLine(top, bottom, color, 1);
			--spriteBatch.Layer;
		}
	}
}