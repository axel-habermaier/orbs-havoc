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
	using System.Numerics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Draws a border around another UI element.
	/// </summary>
	public class Border : UIElement
	{
		private Thickness _borderThickness = new Thickness(1);
		private UIElement _child;
		private Thickness _padding;

		/// <summary>
		///   Gets or sets the padding inside the border.
		/// </summary>
		public Thickness Padding
		{
			get { return _padding; }
			set
			{
				if (_padding == value)
					return;

				_padding = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the child UI element the border is drawn around.
		/// </summary>
		public UIElement Child
		{
			get { return _child; }
			set
			{
				if (_child == value)
					return;

				_child?.ChangeParent(null);
				_child = value;

				SetDirtyState(measure: true, arrange: true);
				_child?.ChangeParent(this);
			}
		}

		/// <summary>
		///   Gets or sets the color of the border.
		/// </summary>
		public Color BorderColor { get; set; } = Colors.Transparent;

		/// <summary>
		///   Gets or sets the thickness of the border.
		/// </summary>
		public Thickness BorderThickness
		{
			get { return _borderThickness; }
			set
			{
				if (_borderThickness == value)
					return;

				_borderThickness = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets the number of children for this UI element.
		/// </summary>
		protected internal override int ChildrenCount => Child == null ? 0 : 1;

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected internal override UIElement GetChild(int index)
		{
			Assert.NotNull(Child);
			Assert.ArgumentSatisfies(index == 0, nameof(index), "The UI element has only one child.");

			return Child;
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
			if (Child == null)
				return new Size(Padding.Width, Padding.Height);

			availableSize = new Size(availableSize.Width - Padding.Width, availableSize.Height - Padding.Height);
			Child.Measure(availableSize);

			return new Size(Child.DesiredSize.Width + Padding.Width, Child.DesiredSize.Height + Padding.Height);
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
			if (Child == null)
				return new Size(Padding.Width, Padding.Height);

			finalSize = new Size(finalSize.Width - Padding.Width, finalSize.Height - Padding.Height);
			Child.Arrange(new Rectangle(0, 0, finalSize));

			return new Size(Child.RenderSize.Width + Padding.Width, Child.RenderSize.Height + Padding.Height);
		}

		/// <summary>
		///   Gets the additional offset that should be applied to the visual offset of the UI element's children.
		/// </summary>
		protected override Vector2 GetAdditionalChildrenOffset()
		{
			var padding = Padding;
			return new Vector2(padding.Left, padding.Top);
		}

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the UI element.
		/// </summary>
		protected override Enumerator<UIElement> GetChildren() => Enumerator<UIElement>.FromItemOrEmpty(Child);

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			if (BorderColor == Colors.Transparent)
				return;

			var area = VisualArea;

			// Make sure there is no overdraw at the corners
			++spriteBatch.Layer;
			spriteBatch.DrawLine(area.TopLeft, area.TopRight, BorderColor, BorderThickness.Top);
			spriteBatch.DrawLine(area.BottomLeft + new Vector2(1, -2), area.TopLeft + new Vector2(1, 0), BorderColor, BorderThickness.Left);
			spriteBatch.DrawLine(area.TopRight, area.BottomRight - new Vector2(0, 1), BorderColor, BorderThickness.Right);
			spriteBatch.DrawLine(area.BottomLeft - new Vector2(0, 1), area.BottomRight - new Vector2(0, 1), BorderColor, BorderThickness.Bottom);
			--spriteBatch.Layer;
		}
	}
}