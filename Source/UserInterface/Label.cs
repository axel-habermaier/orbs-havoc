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
	using Math;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents a text label.
	/// </summary>
	public sealed class Label : DisposableObject
	{
		/// <summary>
		///   The layout of the label's text.
		/// </summary>
		private readonly TextLayout _layout;

		/// <summary>
		///   The renderer for the label's text.
		/// </summary>
		private TextRenderer _textRenderer;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="font">The font that should be used for drawing the text.</param>
		public Label(Font font)
			: this(font, String.Empty)
		{
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="font">The font that should be used for drawing the text.</param>
		/// <param name="text">The text that should be displayed.</param>
		public Label(Font font, string text)
		{
			Assert.ArgumentNotNull(font, nameof(font));
			Assert.ArgumentNotNull(text, nameof(text));

			Commands.OnReloadAssets += OnReloadAssets;

			_layout = new TextLayout(font, text);
			_layout.LayoutChanged += () => _textRenderer.RebuildCache(Font, _layout.Text, _layout.LayoutData);

			Color = Colors.White;

			// Use a large value for the default area; however, don't use Int32.MaxValue because of
			// integer arithmetic overflowing problems
			Area = new Rectangle(0, 0, Int16.MaxValue, Int16.MaxValue);
		}

		/// <summary>
		///   Gets or sets the label's text.
		/// </summary>
		public string Text
		{
			get { return _layout.TextString; }
			set
			{
				Assert.NotDisposed(this);
				_layout.TextString = value;
			}
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
				_layout.DesiredArea = value;
			}
		}

		/// <summary>
		///   Gets or sets the amount of spacing between lines.
		/// </summary>
		public float LineSpacing
		{
			get { return _layout.LineSpacing; }
			set
			{
				Assert.NotDisposed(this);
				_layout.LineSpacing = value;
			}
		}

		/// <summary>
		///   Gets or sets the font that is used to draw the label's text.
		/// </summary>
		public Font Font
		{
			get { return _layout.Font; }
			set
			{
				Assert.NotDisposed(this);
				_layout.Font = value;
			}
		}

		/// <summary>
		///   Gets or sets the text color.
		/// </summary>
		public Color Color
		{
			get { return _textRenderer.Color; }
			set
			{
				Assert.NotDisposed(this);
				_textRenderer.Color = value;
			}
		}

		/// <summary>
		///   Gets or sets the alignment of the label's text.
		/// </summary>
		public TextAlignment Alignment
		{
			get { return _layout.Alignment; }
			set
			{
				Assert.NotDisposed(this);
				_layout.Alignment = value;
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
				// Ensure that the layout is up to date
				_layout.UpdateLayout();
				return _layout.ActualArea;
			}
		}

		/// <summary>
		///   Invoked when assets are reloaded. Relayouts the label's text just in case that the font has been changed.
		/// </summary>
		private void OnReloadAssets()
		{
			_layout.Relayout();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnReloadAssets -= OnReloadAssets;
			_layout.SafeDispose();
		}

		/// <summary>
		///   Draws the label using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the label.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));
			Assert.NotDisposed(this);

			// Ensure that the layout is up to date
			_layout.UpdateLayout();

			_layout.DrawDebugVisualizations(spriteBatch);
			_textRenderer.DrawCached(spriteBatch, _layout.Font.Texture);
		}
	}
}