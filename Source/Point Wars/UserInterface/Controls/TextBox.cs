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

namespace PointWars.UserInterface.Controls
{
	using System;
	using System.Numerics;
	using Assets;
	using Input;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Utilities;
	using static Platform.SDL2;

	/// <summary>
	///   Represents a control that can be used to edit text.
	/// </summary>
	public class TextBox : Control
	{
		private readonly Label _label;
		private Caret _caret;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public TextBox()
		{
			_label = new Label { TextWrapping = TextWrapping.Wrap };
			_caret = new Caret(this);

			Cursor = Assets.TextCursor;
			IsFocusable = true;
			Content = _label;
		}

		/// <summary>
		///   Gets or sets the maximum number of characters that can be manually entered into the text box.
		/// </summary>
		public int MaxLength { get; set; }

		/// <summary>
		///   Gets or sets the text content of the text block.
		/// </summary>
		public string Text
		{
			get { return _label.Text; }
			set
			{
				Assert.ArgumentNotNull(value, nameof(value));

				if (_label.Text == value)
					return;

				_label.Text = value;
				_caret.MoveToEndIfTextChanged();

				SetDirtyState(measure: true, arrange: true);
				TextChanged?.Invoke(_label.Text);
			}
		}

		/// <summary>
		///   Raised when the text contained in the text box has been changed.
		/// </summary>
		public Action<string> TextChanged;

		/// <summary>
		///   Invoked when a mouse button has been pressed while hovering the UI element.
		/// </summary>
		protected override void OnMousePressed(MouseButtonEventArgs e)
		{
			if (e.Button != MouseButton.Left)
				return;

			_caret.Position = _label.GetCharacterIndexAt(e.Position);
			e.Handled = true;
		}

		/// <summary>
		///   Invoked when the IsFocus property of the UI element has changed.
		/// </summary>
		protected internal override void OnFocusChanged()
		{
			_caret.Show();
			Keyboard.TextInputEnabled = IsFocused;
		}

		/// <summary>
		///   Invoked when a text has been entered while the UI element is focused.
		/// </summary>
		protected override void OnTextEntered(TextInputEventArgs e)
		{
			Insert(e.Text);
			e.Handled = true;
		}

		/// <summary>
		///   Inserts the given text at the current caret location, as long as the maximum allowed length is not exceeded.
		/// </summary>
		private void Insert(string text)
		{
			foreach (var character in text)
			{
				// Check if we've exceeded the maximum length
				if (MaxLength > 0 && Text.Length >= MaxLength)
					return;

				_caret.InsertCharacter(character);
			}
		}

		/// <summary>
		///   Invoked when a key has been pressed while the UI element is focused.
		/// </summary>
		protected override unsafe void OnKeyPressed(KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Right:
					if ((e.Modifiers & KeyModifiers.Control) == KeyModifiers.Control)
						_caret.Position = GetBeginningOfNextWord();
					else
						_caret.Move(1);
					break;
				case Key.Left:
					if ((e.Modifiers & KeyModifiers.Control) == KeyModifiers.Control)
						_caret.Position = GetBeginningOfPreviousWord();
					else
						_caret.Move(-1);
					break;
				case Key.Home:
					_caret.MoveToBeginning();
					break;
				case Key.End:
					_caret.MoveToEnd();
					break;
				case Key.Backspace:
					_caret.RemovePreviousCharacter();
					break;
				case Key.Delete:
				case Key.NumpadPeriod:
					_caret.RemoveCurrentCharacter();
					break;
				case Key.V:
					if ((e.Modifiers & KeyModifiers.Control) != 0 && SDL_HasClipboardText() != 0)
					{
						var text = SDL_GetClipboardText();
						if (text == null)
							Log.Error("Failed to retrieve clipboard text from OS: {0}", SDL_GetError());

						Insert(new string(text));
					}
					break;
				default:
					return;
			}

			e.Handled = true;
		}

		/// <summary>
		///   Gets the index of the beginning of the next word.
		/// </summary>
		private int GetBeginningOfNextWord()
		{
			using (var text = TextString.Create(Text))
			{
				var encounteredWhitespace = false;
				for (var i = _caret.Position; i < text.Length; ++i)
				{
					if (Char.IsWhiteSpace(text[i]))
						encounteredWhitespace = true;
					else if (encounteredWhitespace)
						return i;
				}

				return text.Length;
			}
		}

		/// <summary>
		///   Gets the index of the beginning of the previous word.
		/// </summary>
		private int GetBeginningOfPreviousWord()
		{
			using (var text = TextString.Create(Text))
			{
				var encounteredNonWhitespace = false;
				for (var i = _caret.Position; i > 0; --i)
				{
					if (!Char.IsWhiteSpace(text[i - 1]))
						encounteredNonWhitespace = true;
					else if (encounteredNonWhitespace)
						return i;
				}

				return 0;
			}
		}

		/// <summary>
		///   Performs a detailed hit test for the given position. The position is guaranteed to lie within the UI element's
		///   bounds. This method should be overridden to implement special hit testing logic that is more precise than a
		///   simple bounding box check.
		/// </summary>
		/// <param name="position">The position that should be checked for a hit.</param>
		/// <returns>Returns true if the UI element is hit; false, otherwise.</returns>
		protected override bool HitTestCore(Vector2 position)
		{
			return true;
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			if (_label == null || !IsFocused)
				return;

			_caret.Draw(spriteBatch, _label.ComputeCaretPosition(_caret.Position), _label.Font.LineHeight, Foreground);
			Keyboard.ChangeTextInputArea(VisualArea);
		}
	}
}