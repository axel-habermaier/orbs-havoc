﻿namespace OrbsHavoc.UserInterface.Controls
{
	using Platform.Graphics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Draws a texture into the UI.
	/// </summary>
	internal class Image : UIElement
	{
		/// <summary>
		///   Gets or sets the texture that should be drawn.
		/// </summary>
		public Texture Texture { get; set; }

		/// <summary>
		///   Gets the number of children for this UI element.
		/// </summary>
		protected override int ChildrenCount => 0;

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected override UIElement GetChild(int index)
		{
			Assert.NotReached("Image has no children.");
			return null;
		}

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the UI element.
		/// </summary>
		protected override UIElementEnumerator GetChildren() => UIElementEnumerator.Empty;

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
			Assert.NotNull(Texture, "No texture has been set.");
			return Texture.Size;
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
			Assert.NotNull(Texture, "No texture has been set.");
			return Texture.Size;
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			Assert.NotNull(Texture, "No texture has been set.");
			spriteBatch.Draw(Texture, VisualOffset + Texture.Size / 2, Foreground);
		}
	}
}