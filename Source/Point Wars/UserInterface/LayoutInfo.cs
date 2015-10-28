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
	using Utilities;

	/// <summary>
	///   Caches layouting information of an UI element.
	/// </summary>
	internal struct LayoutInfo
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="element">The UI element the layout info should be cached for.</param>
		public LayoutInfo(UIElement element)
			: this()
		{
			Assert.ArgumentNotNull(element, nameof(element));

			HorizontalAlignment = element.HorizontalAlignment;
			VerticalAlignment = element.VerticalAlignment;
			Width = element.Width;
			Height = element.Height;
			MinWidth = element.MinWidth;
			MinHeight = element.MinHeight;
			MaxWidth = element.MaxWidth;
			MaxHeight = element.MaxHeight;
			Margin = element.Margin;
			HasExplicitWidth = !Single.IsNaN(Width);
			HasExplicitHeight = !Single.IsNaN(Height);
		}

		/// <summary>
		///   Gets a value indicating whether the UI element has an explicit width set.
		/// </summary>
		public bool HasExplicitWidth { get; }

		/// <summary>
		///   Gets a value indicating whether the UI element has an explicit height set.
		/// </summary>
		public bool HasExplicitHeight { get; }

		/// <summary>
		///   Gets the horizontal alignment of the UI element.
		/// </summary>
		public HorizontalAlignment HorizontalAlignment { get; }

		/// <summary>
		///   Gets the vertical alignment of the UI element.
		/// </summary>
		public VerticalAlignment VerticalAlignment { get; }

		/// <summary>
		///   Gets the width of the UI element.
		/// </summary>
		public float Width { get; }

		/// <summary>
		///   Gets the minimum width of the UI element.
		/// </summary>
		public float MinWidth { get; }

		/// <summary>
		///   Gets the maximum width of the UI element.
		/// </summary>
		public float MaxWidth { get; }

		/// <summary>
		///   Gets the height of the UI element.
		/// </summary>
		public float Height { get; }

		/// <summary>
		///   Gets the minimum height of the UI element.
		/// </summary>
		public float MinHeight { get; }

		/// <summary>
		///   Gets the maximum height of the UI element.
		/// </summary>
		public float MaxHeight { get; }

		/// <summary>
		///   Gets the margin of the UI element.
		/// </summary>
		public Thickness Margin { get; }
	}
}