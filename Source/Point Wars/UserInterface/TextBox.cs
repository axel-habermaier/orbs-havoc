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
	using System;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Utilities;
	using static Platform.SDL2;

	/// <summary>
	///   Control for text input.
	/// </summary>
	public sealed class TextBox : DisposableObject
	{
		/// <summary>
		///   The caret of the text box.
		/// </summary>
		private readonly Caret _caret = new Caret();

		/// <summary>
		///   The layout of the text box's text.
		/// </summary>
		private readonly TextLayout _layout;

		/// <summary>
		///   The maximum allowed length of the input.
		/// </summary>
		private int _maxLength;

		/// <summary>
		///   The renderer for the text box' text.
		/// </summary>
		private TextRenderer _textRenderer;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="font">The font that is used to draw the text.</param>
		/// <param name="maxLength">The maximum allowed length of the input.</param>
		public TextBox(Font font, int maxLength)
		{
			_maxLength = maxLength;
			_layout = new TextLayout(font, String.Empty);
			_layout.LayoutChanged += text => _textRenderer.RebuildCache(Font, text, _layout.LayoutData);
			_caret.TextChanged += text => _layout.Text = text;

			Text = String.Empty;
			Color = new Color(255, 255, 255, 255);
		}

		/// <summary>
		///   Gets or sets the label's area.
		/// </summary>
		public Rectangle Area
		{
			get { return _layout.DesiredArea; }
			set
			{
				Assert.NotDisposed(this);

				// Subtract the caret width from the desired area's size to ensure that we always have
				// enough space left to draw the caret
				var width = value.Width - _caret.GetWidth(_layout.Font);
				_layout.DesiredArea = new Rectangle(value.Left, value.Top, width, value.Height);
			}
		}

		/// <summary>
		///   Gets the actual text rendering area. Usually, the actual area is smaller than the desired size.
		///   If any words overlap, however, the actual area is bigger.
		/// </summary>
		public Rectangle ActualArea
		{
			get
			{
				Assert.NotDisposed(this);

				// Ensure that the layout is up to date
				_layout.UpdateLayout();
				return _layout.ActualArea;
			}
		}

		/// <summary>
		///   Gets or sets the text that is shown in the text box.
		/// </summary>
		public string Text
		{
			get { return _layout.Text; }
			set
			{
				Assert.NotDisposed(this);

				_layout.Text = value;
				_caret.Text = value;
			}
		}

		/// <summary>
		///   Gets or sets the font that is used to draw the text.
		/// </summary>
		public Font Font
		{
			get { return _layout.Font; }
			set { _layout.Font = value; }
		}

		/// <summary>
		///   Gets or sets the text color.
		/// </summary>
		public Color Color
		{
			get { return _textRenderer.Color; }
			set { _textRenderer.Color = value; }
		}

		/// <summary>
		///   Inserts the given character at the current caret position.
		/// </summary>
		/// <param name="c">The character that should be inserted.</param>
		public void InsertCharacter(char c)
		{
			Assert.NotDisposed(this);

			if (Text.Length < _maxLength)
				_caret.InsertCharacter(c);
		}

		/// <summary>
		///   Injects a key press event.
		/// </summary>
		public unsafe void InjectKeyPress(Key key, KeyModifiers modifiers)
		{
			Assert.NotDisposed(this);

			switch (key)
			{
				case Key.Right:
					if ((modifiers & KeyModifiers.Control) != 0)
						_caret.Position = GetBeginningOfNextWord();
					else
						_caret.Move(1);
					break;
				case Key.Left:
					if ((modifiers & KeyModifiers.Control) != 0)
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
				case Key.NumpadDecimal:
					_caret.RemoveCurrentCharacter();
					break;
				case Key.V:
					if ((modifiers & KeyModifiers.Control) != 0 && SDL_HasClipboardText() != 0)
					{
						var text = SDL_GetClipboardText();
						if (text == null)
							Log.Error("Failed to retrieve clipboard text from OS: {0}", SDL_GetError());

						foreach (var c in new string(text))
							InsertCharacter(c);
					}
					break;
				default:
					return;
			}
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
		///   Draws the text box.
		/// </summary>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			_layout.UpdateLayout();
			_layout.DrawDebugVisualizations(spriteBatch);
			_textRenderer.DrawCached(spriteBatch, _layout.Font.Texture);

			var position = _layout.ComputeCaretPosition(_caret.Position);
			_caret.Draw(spriteBatch, _layout.Font, position);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_caret.SafeDispose();
		}
	}
}