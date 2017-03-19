// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	///   Represents a row of a Grid layout.
	/// </summary>
	public class RowDefinition
	{
		/// <summary>
		///   Gets or sets the row's background color.
		/// </summary>
		public Color Background { get; set; } = Colors.Transparent;

		/// <summary>
		///   Gets or sets the vertical offset of the row.
		/// </summary>
		internal float Offset { get; set; }

		/// <summary>
		///   Gets the effective maximum height of the row, depending on whether a height has explicitly been set.
		/// </summary>
		internal float EffectiveMaxHeight
		{
			get
			{
				if (Single.IsNaN(Height))
					return MaxHeight;

				return Height;
			}
		}

		/// <summary>
		///   Gets or sets the height of the row. Can be NaN to indicate that the row should automatically size itself to the
		///   height of its children.
		/// </summary>
		public float Height { get; set; } = Single.NaN;

		/// <summary>
		///   Gets or sets the minimum height of the row.
		/// </summary>
		public float MinHeight { get; set; }

		/// <summary>
		///   Gets or sets the maximum height of the row.
		/// </summary>
		public float MaxHeight { get; set; } = Single.PositiveInfinity;

		/// <summary>
		///   Gets the actual height of the row.
		/// </summary>
		internal float ActualHeight { get; private set; }

		/// <summary>
		///   Resets the actual height to the default value as if the row contained no children.
		/// </summary>
		internal void ResetActualHeight()
		{
			Assert.That(MinHeight >= 0, "Row has invalid negative minimum height.");
			Assert.That(MaxHeight >= 0, "Row has invalid negative maximum height.");
			Assert.That(Height >= 0 || Single.IsNaN(Height), "Row has invalid negative height.");

			if (Single.IsNaN(Height))
				ActualHeight = MinHeight;
			else
				ActualHeight = Height;
		}

		/// <summary>
		///   Registers the child height on the row. If possible, the row resizes itself to the accommodate the child.
		/// </summary>
		/// <param name="height">The height of the child UI element that should be taken into account.</param>
		internal void RegisterChildHeight(float height)
		{
			if (!Single.IsNaN(Height))
				return;

			if (height > ActualHeight)
				ActualHeight = Math.Min(height, MaxHeight);
		}
	}
}