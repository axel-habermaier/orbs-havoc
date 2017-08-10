﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents a panel that stacks its children vertically or horizontally.
	/// </summary>
	internal class StackPanel : Panel
	{
		private Orientation _orientation = Orientation.Vertical;

		/// <summary>
		///   Gets or sets a value indicating the dimension in which the child elements are stacked.
		/// </summary>
		public Orientation Orientation
		{
			get => _orientation;
			set
			{
				Assert.ArgumentInRange(value, nameof(value));

				if (_orientation == value)
					return;

				_orientation = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

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
			var size = new Size();
			var isHorizontallyOriented = Orientation == Orientation.Horizontal;

			// The stack panel's size is not limited in the direction that it stacks
			if (isHorizontallyOriented)
				availableSize.Width = Single.PositiveInfinity;
			else
				availableSize.Height = Single.PositiveInfinity;

			// Compute the stack panel's actual size, depending on its orientation
			foreach (var child in Children)
			{
				child.Measure(availableSize);
				var desiredSize = child.DesiredSize;

				if (isHorizontallyOriented)
				{
					size.Width += desiredSize.Width;
					size.Height = Math.Max(size.Height, desiredSize.Height);
				}
				else
				{
					size.Width = Math.Max(size.Width, desiredSize.Width);
					size.Height += desiredSize.Height;
				}
			}

			return size;
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
			var isHorizontallyOriented = Orientation == Orientation.Horizontal;
			var offset = 0.0f;

			// Arrange the children such that they're stacked either horizontally or vertically
			foreach (var child in Children)
			{
				if (isHorizontallyOriented)
				{
					child.Arrange(new Rectangle(offset, 0, child.DesiredSize.Width, finalSize.Height));
					offset += child.DesiredSize.Width;
				}
				else
				{
					child.Arrange(new Rectangle(0, offset, finalSize.Width, child.DesiredSize.Height));
					offset += child.DesiredSize.Height;
				}
			}

			return finalSize;
		}
	}
}