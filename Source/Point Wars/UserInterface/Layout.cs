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
	using Utilities;

	/// <summary>
	///   Provides methods for UI element layouting.
	/// </summary>
	public static class Layout
	{
		/// <summary>
		///   Gets the UI elements' area.
		/// </summary>
		/// <param name="elements">The UI elements the area should be computed for.</param>
		public static Rectangle GetArea(params UIElement[] elements)
		{
			var left = Single.MaxValue;
			var top = Single.MaxValue;
			var right = Single.MinValue;
			var bottom = Single.MinValue;

			foreach (var element in elements)
			{
				var area = element.Area;

				left = Math.Min(left, area.Left);
				right = Math.Max(right, area.Right);
				top = Math.Min(top, area.Top);
				bottom = Math.Max(bottom, area.Bottom);
			}

			return new Rectangle(left, top, right - left, bottom - top);
		}

		/// <summary>
		///   Stacks the elements vertically.
		/// </summary>
		/// <param name="margin">The margin between two consecutive elements.</param>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void StackVertically(float margin = 0, params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			var offset = 0.0f;
			foreach (var element in elements)
			{
				offset += element.Margin.Top + element.Style.BorderThickness;
				element.Area = element.Area.Offset(new Vector2(0, offset));
				offset += margin + element.Area.Height + element.Margin.Bottom + element.Style.BorderThickness;
			}
		}

		/// <summary>
		///   Stacks the elements vertically.
		/// </summary>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void StackVertically(params UIElement[] elements)
		{
			StackVertically(0, elements);
		}

		/// <summary>
		///   Resets the layouted positions.
		/// </summary>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void ResetLayoutedPositions(params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			foreach (var element in elements)
				element.Area = element.Area.MoveTo(Vector2.Zero);
		}

		/// <summary>
		///   Centers the UI elements vertically within the given area.
		/// </summary>
		/// <param name="area">The area the UI elements should be centered vertically in.</param>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void CenterVertically(Rectangle area, params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			if (elements.Length == 0)
				return;

			var height = GetArea(elements).Height;
			foreach (var element in elements)
				element.Area = element.Area.Offset(new Vector2(0, area.Top + MathUtils.Round((area.Height - height) / 2)));
		}

		/// <summary>
		///   Centers the UI elements horizontally within the given area.
		/// </summary>
		/// <param name="area">The area the UI elements should be centered horizontally in.</param>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void CenterHorizontally(Rectangle area, params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			if (elements.Length == 0)
				return;

			var width = GetArea(elements).Width;
			foreach (var element in elements)
				element.Area = element.Area.Offset(new Vector2(area.Left + MathUtils.Round((area.Width - width) / 2), 0));
		}

		/// <summary>
		///   Centers each UI elements horizontally within the given area.
		/// </summary>
		/// <param name="area">The area the UI elements should be centered horizontally in.</param>
		/// <param name="elements">The UI elements that should be layouted.</param>
		public static void CenterEachHorizontally(Rectangle area, params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			foreach (var element in elements)
				element.CenterHorizontally(area);
		}
	}
}