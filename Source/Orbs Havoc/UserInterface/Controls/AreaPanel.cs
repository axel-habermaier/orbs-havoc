﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System.Numerics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Allows each child to take up the entire area of the panel.
	/// </summary>
	internal class AreaPanel : Panel
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
			foreach (var child in Children)
				child.Measure(availableSize);

			return availableSize;
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
				child.Arrange(new Rectangle(Vector2.Zero, finalSize));

			return finalSize;
		}

		/// <summary>
		///   Draws the child UI elements of the current UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element's children.</param>
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			foreach (var child in GetChildren())
			{
				// We draw each child on its own range of layers, as the children of an area panel are typically stacked
				// along the Z axis and therefore overlap each other.
				spriteBatch.RenderState.Layer += 100;
				child.Draw(spriteBatch);
			}
		}
	}
}