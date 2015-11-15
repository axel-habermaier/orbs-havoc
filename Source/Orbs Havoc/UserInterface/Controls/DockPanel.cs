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
	using Utilities;

	/// <summary>
	///   A panel that arranges its children both vertically and horizontally.
	/// </summary>
	public class DockPanel : Panel
	{
		private bool _lastChildFill = true;

		/// <summary>
		///   Gets or sets a value indicating whether the last child element within the dock panel fills the remaining available
		///   space.
		/// </summary>
		public bool LastChildFill
		{
			get { return _lastChildFill; }
			set
			{
				if (_lastChildFill == value)
					return;

				_lastChildFill = true;
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
			// The desired size of the dock panel itself
			var dockSize = new Size();

			// The accumulated sizes of the children in the direction they actually take up space
			var accumulatedSize = new Size();

			foreach (var child in Children)
			{
				// The available size for the child is what we've got left after allocating some of the available space to
				// the previously enumerated children
				var availableChildSize = new Size(
					Math.Max(0, availableSize.Width - accumulatedSize.Width),
					Math.Max(0, availableSize.Height - accumulatedSize.Height));

				child.Measure(availableChildSize);
				var desiredChildSize = child.DesiredSize;

				// We now update the dock size and the accumulated size depending on the child's Dock value
				switch (child.Dock)
				{
					case Dock.Top:
					case Dock.Bottom:
						dockSize.Width = Math.Max(dockSize.Width, accumulatedSize.Width + desiredChildSize.Width);
						accumulatedSize.Height += desiredChildSize.Height;
						break;
					case Dock.Left:
					case Dock.Right:
						dockSize.Height = Math.Max(dockSize.Height, accumulatedSize.Height + desiredChildSize.Height);
						accumulatedSize.Width += desiredChildSize.Width;
						break;
					default:
						Assert.NotReached("Unknown dock type.");
						break;
				}
			}

			return new Size(
				Math.Max(dockSize.Width, accumulatedSize.Width),
				Math.Max(dockSize.Height, accumulatedSize.Height));
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
			var accumulatedLeft = 0.0f;
			var accumulatedTop = 0.0f;
			var accumulatedRight = 0.0f;
			var accumulatedBottom = 0.0f;

			var count = ChildrenCount;
			var i = 0;
			foreach (var child in Children)
			{
				var desiredChildSize = child.DesiredSize;
				var childArea = new Rectangle(
					accumulatedLeft,
					accumulatedTop,
					Math.Max(0.0f, finalSize.Width - (accumulatedLeft + accumulatedRight)),
					Math.Max(0.0f, finalSize.Height - (accumulatedTop + accumulatedBottom)));

				if (i != count - 1 || LastChildFill)
				{
					switch (child.Dock)
					{
						case Dock.Top:
							accumulatedTop += desiredChildSize.Height;
							childArea.Height = desiredChildSize.Height;
							break;
						case Dock.Bottom:
							accumulatedBottom += desiredChildSize.Height;
							childArea.Top = Math.Max(0.0f, finalSize.Height - accumulatedBottom);
							childArea.Height = desiredChildSize.Height;
							break;
						case Dock.Left:
							accumulatedLeft += desiredChildSize.Width;
							childArea.Width = desiredChildSize.Width;
							break;
						case Dock.Right:
							accumulatedRight += desiredChildSize.Width;
							childArea.Left = Math.Max(0.0f, finalSize.Width - accumulatedRight);
							childArea.Width = desiredChildSize.Width;
							break;
					}
				}

				child.Arrange(childArea);
				++i;
			}

			return finalSize;
		}
	}
}