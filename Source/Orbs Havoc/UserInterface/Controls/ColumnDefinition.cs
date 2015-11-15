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

namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a column of a Grid layout.
	/// </summary>
	public class ColumnDefinition
	{
		/// <summary>
		///   Gets or sets the width of the column. Can be NaN to indicate that the column should automatically size itself to the
		///   width of its children.
		/// </summary>
		public float Width { get; set; } = Single.NaN;

		/// <summary>
		///   Gets or sets the minimum width of the column.
		/// </summary>
		public float MinWidth { get; set; }

		/// <summary>
		///   Gets or sets the maximum width of the column.
		/// </summary>
		public float MaxWidth { get; set; } = Single.PositiveInfinity;

		/// <summary>
		///   Gets the actual width of the column.
		/// </summary>
		internal float ActualWidth { get; private set; }

		/// <summary>
		///   Gets the effective maximum width of the column, depending on whether a width has explicitly been set.
		/// </summary>
		internal float EffectiveMaxWidth
		{
			get
			{
				if (Single.IsNaN(Width))
					return MaxWidth;

				return Width;
			}
		}

		/// <summary>
		///   Gets or sets the column's background color.
		/// </summary>
		public Color Background { get; set; } = Colors.Transparent;

		/// <summary>
		///   Gets or sets the horizontal offset of the column.
		/// </summary>
		internal float Offset { get; set; }

		/// <summary>
		///   Resets the actual width to the default value as if the column contained no children.
		/// </summary>
		internal void ResetActualWidth()
		{
			Assert.That(MinWidth >= 0, "Column has invalid negative minimum width.");
			Assert.That(MaxWidth >= 0, "Column has invalid negative maximum width.");
			Assert.That(Width >= 0 || Single.IsNaN(Width), "Column has invalid negative width.");

			if (Single.IsNaN(Width))
				ActualWidth = MinWidth;
			else
				ActualWidth = Width;
		}

		/// <summary>
		///   Registers the child width on the column. If possible, the column resizes itself to the accommodate the child.
		/// </summary>
		/// <param name="width">The width of the child UI element that should be taken into account.</param>
		internal void RegisterChildWidth(float width)
		{
			if (!Single.IsNaN(Width))
				return;

			if (width > ActualWidth)
				ActualWidth = Math.Min(width, MaxWidth);
		}
	}
}