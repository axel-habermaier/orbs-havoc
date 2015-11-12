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
	using System;
	using System.Collections.Generic;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Provides a flexible grid layout that consists of rows and columns.
	/// </summary>
	public class Grid : Panel
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="columns">The number of auto-sized columns that should be created.</param>
		/// <param name="rows">The number of auto-sized rows that should be created.</param>
		public Grid(int columns = 0, int rows = 0)
		{
			for (var i = 0; i < columns; ++i)
				Columns.Add(new ColumnDefinition());

			for (var i = 0; i < rows; ++i)
				Rows.Add(new RowDefinition());
		}

		/// <summary>
		///   Gets the columns of this grid.
		/// </summary>
		public List<ColumnDefinition> Columns { get; } = new List<ColumnDefinition>();

		/// <summary>
		///   Gets the rows of this grid.
		/// </summary>
		public List<RowDefinition> Rows { get; } = new List<RowDefinition>();

		/// <summary>
		///   Computes and returns the desired size of the element given the available space allocated by the parent UI element.
		/// </summary>
		/// <param name="availableSize">
		///   The available space that the parent UI element can allocate to this UI element. Can be infinity if the parent wants
		///   to size itself to its contents. The computed desired size is allowed to exceed the available space; the parent UI
		///   element might be able to use scrolling in this case.
		/// </param>
		protected override Size MeasureCore(Size availableSize)
		{
			// Reset all rows and columns
			foreach (var column in Columns)
				column.ResetActualWidth();

			foreach (var row in Rows)
				row.ResetActualHeight();

			// Register all children on the rows and columns
			foreach (var child in Children)
			{
				Assert.InRange(child.Column, Columns);
				Assert.InRange(child.Row, Rows);

				var column = Columns[child.Column];
				var row = Rows[child.Row];

				var width = Math.Min(column.EffectiveMaxWidth, availableSize.Width);
				if (Single.IsNaN(column.Width))
				{
					for (var c = child.Column; c > 0; --c)
						width -= Columns[c - 1].ActualWidth;
				}

				var height = Math.Min(row.EffectiveMaxHeight, availableSize.Height);
				if (Single.IsNaN(row.Height))
				{
					for (var r = child.Row; r > 0; --r)
						height -= Rows[r - 1].ActualHeight;
				}

				child.Measure(new Size(width, height));
				var desiredSize = child.DesiredSize;

				column.RegisterChildWidth(desiredSize.Width);
				row.RegisterChildHeight(desiredSize.Height);
			}

			// Compute the desired size of the grid and update the row and column offsets
			var gridSize = new Size();
			foreach (var column in Columns)
			{
				column.Offset = gridSize.Width;
				gridSize.Width += column.ActualWidth;
			}

			foreach (var row in Rows)
			{
				row.Offset = gridSize.Height;
				gridSize.Height += row.ActualHeight;
			}

			return gridSize;
		}

		/// <summary>
		///   Determines the size of the UI element and positions all of its children. Returns the actual size used by the UI
		///   element. If this value is smaller than the given size, the UI element's alignment properties position it
		///   appropriately.
		/// </summary>
		/// <param name="finalSize">
		///   The final area allocated by the UI element's parent that the UI element should use to arrange
		///   itself and its children.
		/// </param>
		protected override Size ArrangeCore(Size finalSize)
		{
			foreach (var child in Children)
			{
				var column = Columns[child.Column];
				var row = Rows[child.Row];

				child.Arrange(new Rectangle(column.Offset, row.Offset, column.ActualWidth, row.ActualHeight));
			}

			return finalSize;
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			spriteBatch.RenderState.Layer += 1;

			foreach (var column in Columns)
			{
				if (column.Background != Colors.Transparent)
					spriteBatch.Draw(new Rectangle(VisualOffset.X + column.Offset, VisualOffset.Y, column.ActualWidth, ActualHeight), column.Background);
			}

			foreach (var row in Rows)
			{
				if (row.Background != Colors.Transparent)
					spriteBatch.Draw(new Rectangle(VisualOffset.X, VisualOffset.Y + row.Offset, ActualWidth, row.ActualHeight), row.Background);
			}

			spriteBatch.RenderState.Layer -= 1;
		}

		/// <summary>
		///   Draws the child UI elements of the current UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element's children.</param>
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			spriteBatch.RenderState.Layer += 1;
			base.DrawChildren(spriteBatch);
			spriteBatch.RenderState.Layer -= 1;
		}
	}
}