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
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a text label.
	/// </summary>
	public sealed class Label : UIElement
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
		/// <param name="label">The label that should be displayed.</param>
		public Label(string label = "")
		{
			Assert.ArgumentNotNull(label, nameof(label));

			_layout = new TextLayout(Assets.DefaultFont, label);
			_layout.LayoutChanged += text => _textRenderer.RebuildCache(Font, text, _layout.LayoutData);

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
			get { return _layout.Text; }
			set
			{
				Assert.NotDisposed(this);
				_layout.Text = value;
			}
		}

		/// <summary>
		///   Gets or sets the label's area.
		/// </summary>
		public override Rectangle Area
		{
			get
			{
				Assert.NotDisposed(this);

				// Ensure that the layout is up to date
				_layout.UpdateLayout();
				return _layout.ActualArea;
			}
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
		public TextAlignment TextAlignment
		{
			get { return _layout.Alignment; }
			set
			{
				Assert.NotDisposed(this);
				_layout.Alignment = value;
			}
		}

		/// <summary>
		///   Draws the label using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the label.</param>
		public override void Draw(SpriteBatch spriteBatch)
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