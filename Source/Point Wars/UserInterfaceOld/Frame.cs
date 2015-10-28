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

namespace PointWars.UserInterfaceOld
{
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a simple frame with a background and a border.
	/// </summary>
	public sealed class Frame : UIElement
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Frame()
		{
			NormalStyle = new Style
			{
				BorderThickness = 1,
				Background = new Color(0xFF002033),
				BorderColor = new Color(0xFF055674),
				Foreground = Colors.White
			};
		}

		/// <summary>
		///   Gets or sets the area occupied by the UI element's contents.
		/// </summary>
		public override Rectangle ContentArea { get; set; }

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			// Nothing to do here
		}
	}
}