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
	using System.Numerics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a button.
	/// </summary>
	public sealed class Button : UIElement
	{
		private readonly Label _label;
		private Rectangle _area;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="label">The text shown on the button.</param>
		public Button(string label = "")
		{
			Assert.ArgumentNotNull(label, nameof(label));
			_label = new Label(label) { TextAlignment = TextAlignment.Centered | TextAlignment.Middle };
		}

		/// <summary>
		///   Gets or sets the font used to draw the button's label.
		/// </summary>
		public Font Font
		{
			get { return _label.Font; }
			set { _label.Font = value; }
		}

		/// <summary>
		///   Gets or sets the button's label.
		/// </summary>
		public string Label
		{
			get { return _label.Text; }
			set { _label.Text = value; }
		}

		/// <summary>
		///   Gets or sets the button's background color.
		/// </summary>
		public Color Background { get; set; } = new Color(0x5F00588B);

		/// <summary>
		///   Gets or sets the button's foreground color.
		/// </summary>
		public Color Foreground { get; set; } = Colors.White;

		/// <summary>
		///   Gets or sets the button's border color.
		/// </summary>
		public Color BorderColor { get; set; } = new Color(0xFF055674);

		/// <summary>
		///   Gets or sets the button's border thickness.
		/// </summary>
		public float BorderThickness { get; set; } = 1;

		/// <summary>
		///   Gets or sets the button's padding, i.e., the margin between its label and its border.
		/// </summary>
		public Thickness Padding { get; set; } = new Thickness(7, 3, 7, 3);

		/// <summary>
		///   Gets or sets the UI element's desired area before layouting.
		/// </summary>
		public override Rectangle Area
		{
			get
			{
				var area = _label.Area;
				var size = new Size(
					Math.Max(area.Width + Padding.Width + BorderThickness, _area.Width),
					Math.Max(area.Height + Padding.Height + BorderThickness, _area.Height));

				return new Rectangle(_area.Position, size);
			}
			set
			{
				_area = value;

				var position = value.Position + new Vector2(Padding.Left, Padding.Top);
				var size = new Size(value.Size.Width - Padding.Width - BorderThickness, value.Size.Height - Padding.Height - BorderThickness);
				_label.Area = new Rectangle(position, size);
			}
		}

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Background != Colors.Transparent)
				spriteBatch.Draw(Area, Background);

			if (BorderThickness > 0 && BorderColor != Colors.Transparent)
				spriteBatch.DrawOutline(Area, BorderColor, BorderThickness);

			++spriteBatch.Layer;
			_label.Draw(spriteBatch);
			--spriteBatch.Layer;
		}
	}
}