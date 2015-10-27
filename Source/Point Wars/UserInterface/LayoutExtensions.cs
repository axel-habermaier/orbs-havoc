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
	using System.Numerics;
	using Utilities;

	/// <summary>
	///   Provides layouting extension methods for UI elements.
	/// </summary>
	public static class LayoutExtensions
	{
		/// <summary>
		///   Centers the UI element horizontally within the given area.
		/// </summary>
		/// <param name="element">The UI element that should be centered horizontally.</param>
		/// <param name="area">The area the UI element should be centered horizontally in.</param>
		public static void CenterHorizontally(this UIElement element, Rectangle area)
		{
			var x = area.Left + element.Margin.Left - element.Margin.Right + MathUtils.Round((area.Width - element.Area.Width) / 2);
            element.Area = element.Area.Offset(new Vector2(x, 0));
		}

		/// <summary>
		///   Stacks the elements vertically.
		/// </summary>
		/// <param name="elements">The elements that should be stacked.</param>
		/// <param name="margin">The margin between two consecutive elements.</param>
		public static void StackVertically(this UIElement[] elements, float margin = 0)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			var offset = 0.0f;
			foreach (var element in elements)
			{
				offset += element.Margin.Top;
				element.Area = element.Area.Offset(new Vector2(0, offset));
				offset += margin + element.Area.Height + element.Margin.Bottom;
			}
		}

		/// <summary>
		///   Centers the UI elements vertically within the given area.
		/// </summary>
		/// <param name="elements">The elements that should be centered.</param>
		/// <param name="area">The area the UI elements should be centered vertically in.</param>
		public static void CenterVertically(this UIElement[] elements, Rectangle area)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			if (elements.Length == 0)
				return;

			var bottom = elements[elements.Length - 1].Area.Bottom;
			var top = elements[0].Area.Top;
			var height = bottom - top;

			foreach (var element in elements)
				element.Area = element.Area.Offset(new Vector2(0, area.Top + MathUtils.Round((area.Height - height) / 2)));
		}
	}
}