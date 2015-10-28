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
	using Utilities;

	/// <summary>
	///   Represents a base class for templated UI elements.
	/// </summary>
	public class Control : UIElement
	{
		private UIElement _child;

		private Thickness _padding;

		/// <summary>
		///   Gets or sets the UI element that is decorated.
		/// </summary>
		public UIElement Child
		{
			get { return _child; }
			set
			{
				Assert.ArgumentNotNull(value, nameof(value));

				if (_child == value)
					return;

				_child?.ChangeParent(null);
				_child = value;
				_child?.ChangeParent(this);

				SetDirtyState(measure: true, arrange: true);
			}
		}

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
		///   Gets the number of children for this visual.
		/// </summary>
		protected internal override int ChildrenCount => _child == null ? 0 : 1;

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all logical children of the UI element.
		/// </summary>
		protected override sealed Enumerator<UIElement> GetChildren() => Enumerator<UIElement>.FromItemOrEmpty(_child);

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected internal override sealed UIElement GetChild(int index)
		{
			Assert.NotNull(_child);
			Assert.ArgumentSatisfies(index == 0, nameof(index), "The UI element has only one child.");

			return _child;
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
			if (_child == null)
				return new Size();

			availableSize = new Size(availableSize.Width - Padding.Width, availableSize.Height - Padding.Height);
			_child.Measure(availableSize);

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
			if (_child == null)
				return new Size();

			finalSize = new Size(finalSize.Width - Padding.Width, finalSize.Height - Padding.Height);
			_child.Arrange(new Rectangle(0, 0, finalSize));

			return new Size(_child.RenderSize.Width + Padding.Width, _child.RenderSize.Height + Padding.Height);
		}
	}
}