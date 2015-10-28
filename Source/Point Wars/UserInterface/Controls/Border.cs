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
	using System.Numerics;
	using Rendering;

	/// <summary>
	///   Draws a border, a background, or both around another element.
	/// </summary>
	public class Border : Control
	{
		private Thickness _borderThickness = new Thickness(1);

		/// <summary>
		///   Gets or sets the color of the border.
		/// </summary>
		public Color BorderColor { get; set; } = Colors.Transparent;

		/// <summary>
		///   Gets or sets the thickness of the border.
		/// </summary>
		public Thickness BorderThickness
		{
			get { return _borderThickness; }
			set
			{
				if (_borderThickness == value)
					return;

				_borderThickness = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets the additional offset that should be applied to the visual offset of the UI element's children.
		/// </summary>
		protected override Vector2 GetAdditionalChildrenOffset()
		{
			var padding = Padding;
			return new Vector2(padding.Left, padding.Top);
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			if (BorderColor == Colors.Transparent)
				return;

			var area = VisualArea;

			// Make sure there is no overdraw at the corners
			spriteBatch.DrawLine(area.TopLeft, area.TopRight, BorderColor, BorderThickness.Top);
			spriteBatch.DrawLine(area.BottomLeft + new Vector2(1, -1), area.TopLeft + new Vector2(1, 1), BorderColor, BorderThickness.Left);
			spriteBatch.DrawLine(area.TopRight + new Vector2(0, 1), area.BottomRight - new Vector2(0, 1), BorderColor, BorderThickness.Right);
			spriteBatch.DrawLine(area.BottomLeft - new Vector2(0, 1), area.BottomRight - new Vector2(0, 1), BorderColor, BorderThickness.Bottom);
		}
	}
}