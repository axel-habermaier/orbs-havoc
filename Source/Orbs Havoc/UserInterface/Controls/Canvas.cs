﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents a layout where all child elements can position themselves with coordinates relative to the canvas.
	/// </summary>
	internal class Canvas : Panel
	{
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
			// The canvas does not restrict the size its children
			var size = new Size(Single.PositiveInfinity, Single.PositiveInfinity);

			foreach (var child in Children)
				child.Measure(size);

			// The canvas itself consumes no space
			return new Size();
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
			// Arrange all children by checking the values of their Left, Right, Top, and Bottom properties
			foreach (var child in Children)
			{
				var x = 0.0f;
				var y = 0.0f;

				// Check if Left is set to a valid value (and if so, ignore Right)
				if (!Single.IsNaN(child.Left))
					x = child.Left;
				else
				{
					// Else, check Right; if Right is also invalid, just position the element to the left of the canvas
					if (!Single.IsNaN(child.Right))
						x = finalSize.Width - child.DesiredSize.Width - child.Right;
				}

				// Check if Top is set to a valid value (and if so, ignore Bottom)
				if (!Single.IsNaN(child.Top))
					y = child.Top;
				else
				{
					// Else, check Bottom; if Bottom is also invalid, just position the element to the top of the canvas
					if (!Single.IsNaN(child.Bottom))
						y = finalSize.Height - child.DesiredSize.Height - child.Bottom;
				}

				child.Arrange(new Rectangle(x, y, child.DesiredSize));
			}

			return finalSize;
		}
	}
}