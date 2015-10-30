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
	using System.Collections.Generic;
	using System.Numerics;
	using Controls;
	using Input;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Provides layouting, input, hit testing, rendering, and other base functionality for all UI elements.
	/// </summary>
	public abstract partial class UIElement
	{
		/// <summary>
		///   The list of input bindings associated with this UI element.
		/// </summary>
		private List<InputBinding> _inputBindings;

		/// <summary>
		///   The available size allocated for the UI element during the last measure phase.
		/// </summary>
		private Size _previousAvailableSize;

		/// <summary>
		///   The final rectangle allocated for the UI element during the last arrange phase.
		/// </summary>
		private Rectangle _previousFinalRect;

		/// <summary>
		///   The visual offset that has previously been applied to this UI element.
		/// </summary>
		private Vector2 _previousVisualOffset;

		/// <summary>
		///   The relative visual offset of the UI element for drawing.
		/// </summary>
		private Vector2 _relativeVisualOffset;

		/// <summary>
		///   The state of the UI element;
		/// </summary>
		private State _state;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		protected UIElement()
		{
			IsHitTestVisible = true;
		}

		/// <summary>
		///   Sets the corresponding UI element state to dirty.
		/// </summary>
		/// <param name="measure">Indicates whether the measure state should be set to dirty.</param>
		/// <param name="arrange">Indicates whether the arrange state should be set to dirty.</param>
		protected void SetDirtyState(bool measure, bool arrange)
		{
			if (measure)
			{
				IsMeasureDataDirty = true;
				var element = Parent;

				while (element != null && element.Visibility != Visibility.Collapsed)
				{
					element.IsMeasureDataDirty = true;
					element = element.Parent;
				}
			}
			else if (arrange)
			{
				IsArrangeDataDirty = true;
				var element = Parent;

				while (element != null && element.Visibility != Visibility.Collapsed)
				{
					element.IsArrangeDataDirty = true;
					element = element.Parent;
				}
			}
		}

		/// <summary>
		///   Updates the UI element's visible state.
		/// </summary>
		private void UpdateVisibleState()
		{
			var isVisible = Visibility == Visibility.Visible && IsAttachedToRoot &&
							((Parent != null && Parent.IsVisible) || this is RootUIElement);

			if (IsVisible == isVisible)
				return;

			IsVisible = isVisible;

			foreach (var child in GetChildren())
				child.UpdateVisibleState();
		}

		/// <summary>
		///   Gets the root element the UI element is contained in or null if it isn't contained in any root element.
		/// </summary>
		/// <param name="element">The UI element the parent root element should be returned for.</param>
		private static RootUIElement GetRootElement(UIElement element)
		{
			while (element != null)
			{
				var rootElement = element as RootUIElement;
				if (rootElement != null)
					return rootElement;

				element = element.Parent;
			}

			return null;
		}

		/// <summary>
		///   Sets the keyboard focus to this UI element, provided that it is focusable.
		/// </summary>
		public void Focus()
		{
			if (!CanBeFocused)
				return;

			var rootElement = GetRootElement(this);
			if (rootElement != null)
				rootElement.FocusedElement = this;
		}

		/// <summary>
		///   Removes the keyboard focus from this UI element, provided that it is focused.
		/// </summary>
		public void Unfocus()
		{
			if (!IsFocused)
				return;

			var rootElement = GetRootElement(this);
			if (rootElement != null && rootElement.FocusedElement == this)
				rootElement.FocusedElement = null;
		}

		/// <summary>
		///   Returns the child UI element of the current UI element that lies at the given position.
		/// </summary>
		/// <param name="position">The position that should be checked for a hit.</param>
		/// <param name="boundsTestOnly">A value indicating whether only the bounds of the UI elements should be checked.</param>
		internal UIElement HitTest(Vector2 position, bool boundsTestOnly)
		{
			// If the element isn't visible or hit test visible, there can be no hit.
			if (!IsVisible || !IsHitTestVisible)
				return null;

			// Check if the position lies within the element's bounding box. If not, there can be no hit.
			var horizontalHit = position.X >= VisualOffset.X && position.X <= VisualOffset.X + RenderSize.Width - Margin.Width;
			var verticalHit = position.Y >= VisualOffset.Y && position.Y <= VisualOffset.Y + RenderSize.Height - Margin.Height;

			if (!horizontalHit || !verticalHit)
				return null;

			// Find and return the first visual child that is hit.
			// We have to iterate the visual children in reverse order, as they are enumerated from
			// bottom to top (optimized for drawing), whereas we want to check the topmost children first.
			var count = ChildrenCount;
			for (var i = count; i > 0; --i)
			{
				var child = GetChild(i - 1);
				var hitTestResult = child.HitTest(position, boundsTestOnly);

				if (hitTestResult != null)
					return hitTestResult;
			}

			// If the UI element captures all input, return it, even though there might not have been a hit otherwise
			if (CapturesInput)
				return this;

			// Otherwise, check if we should return this UI element instead.
			if (boundsTestOnly || HitTestCore(position))
				return this;

			return null;
		}

		/// <summary>
		///   Performs a detailed hit test for the given position. The position is guaranteed to lie within the UI element's
		///   bounds. This method should be overridden to implement special hit testing logic that is more precise than a
		///   simple bounding box check.
		/// </summary>
		/// <param name="position">The position that should be checked for a hit.</param>
		/// <returns>Returns true if the UI element is hit; false, otherwise.</returns>
		protected virtual bool HitTestCore(Vector2 position)
		{
			return Background != Colors.Transparent;
		}

		/// <summary>
		///   Updates the UI element's desired size. This method should be called from a parent UI element's MeasureCore method to
		///   perform a the first pass of a recursive layout update.
		/// </summary>
		/// <param name="availableSize">
		///   The available space that the parent UI element can allocate to this UI element. Can be infinity if the parent wants
		///   to size itself to its contents. The computed desired size is allowed to exceed the available space; the parent UI
		///   element might be able to use scrolling in this case.
		/// </param>
		public void Measure(Size availableSize)
		{
			if (!IsMeasureDataDirty && _previousAvailableSize == availableSize)
				return;

			if (Visibility == Visibility.Collapsed)
			{
				IsMeasureDataDirty = false;
				DesiredSize = new Size();
				return;
			}

			_previousAvailableSize = availableSize;
			availableSize = RestrictSize(availableSize);

			DesiredSize = MeasureCore(DecreaseByMargin(availableSize));

			Assert.That(!Single.IsInfinity(DesiredSize.Width) && !Single.IsNaN(DesiredSize.Width), "MeasureCore returned invalid width.");
			Assert.That(!Single.IsInfinity(DesiredSize.Height) && !Single.IsNaN(DesiredSize.Height), "MeasureCore returned invalid height.");

			DesiredSize = RestrictSize(DesiredSize);
			DesiredSize = IncreaseByMargin(DesiredSize);

			IsMeasureDataDirty = false;
			IsArrangeDataDirty = true;
		}

		/// <summary>
		///   Restricts the given size, taking the explicit size as well as the minimum and maximum size of the UI element into
		///   account.
		/// </summary>
		/// <param name="size">The size that should be restricted.</param>
		private Size RestrictSize(Size size)
		{
			if (!Single.IsNaN(Width))
				size.Width = Width;

			if (!Single.IsNaN(Height))
				size.Height = Height;

			size.Width = MathUtils.Clamp(size.Width, MinWidth, MaxWidth);
			size.Height = MathUtils.Clamp(size.Height, MinHeight, MaxHeight);

			return size;
		}

		/// <summary>
		///   Computes and returns the desired size of the element given the available space allocated by the parent UI element.
		/// </summary>
		/// <param name="availableSize">
		///   The available space that the parent UI element can allocate to this UI element. Can be infinity if the parent wants
		///   to size itself to its contents. The computed desired size is allowed to exceed the available space; the parent UI
		///   element might be able to use scrolling in this case.
		/// </param>
		protected abstract Size MeasureCore(Size availableSize);

		/// <summary>
		///   Determines the size and position of the UI element and all of its children. This method should be called from a
		///   parent UI element's ArrangeCore method to perform the second pass of a recursive layout update.
		/// </summary>
		/// <param name="finalRect">The final size and position of the UI element.</param>
		/// <remarks>
		///   The first time a UI element is layouted, Measure is always called before Arrange. Later layout passes
		///   triggered by some changes to the UI element's state might only call Arrange if the UI element's measurement remained
		///   unaffected by the state change.
		/// </remarks>
		public void Arrange(Rectangle finalRect)
		{
			if (!IsArrangeDataDirty && _previousFinalRect == finalRect)
				return;

			if (Visibility == Visibility.Collapsed)
			{
				IsArrangeDataDirty = false;
				VisualOffset = Vector2.Zero;
				RenderSize = new Size();
				ActualWidth = 0;
				ActualHeight = 0;

				return;
			}

			_previousFinalRect = finalRect;
			var availableSize = DecreaseByMargin(finalRect.Size);
			var desiredSize = DecreaseByMargin(DesiredSize);

			var width = Math.Min(desiredSize.Width, availableSize.Width);
			var height = Math.Min(desiredSize.Height, availableSize.Height);

			var finalSize = new Size(width, height);
			AdaptSize(ref finalSize, availableSize);

			var size = ArrangeCore(finalSize);
			AdaptSize(ref size, availableSize);

			ActualWidth = size.Width;
			ActualHeight = size.Height;

			OnActualSizeChanged();
			RenderSize = size;

			_relativeVisualOffset = finalRect.Position + ComputeAlignmentOffset(finalRect.Size);
			RenderSize = IncreaseByMargin(size);

			IsArrangeDataDirty = false;
			IsVisualOffsetDirty = true;
		}

		/// <summary>
		///   Invoked when the actual size of the UI element has changed after a layouting pass.
		/// </summary>
		protected virtual void OnActualSizeChanged()
		{
		}

		/// <summary>
		///   Changes the parent of the UI element.
		/// </summary>
		/// <param name="parent">The new parent of the UI element.</param>
		public void ChangeParent(UIElement parent)
		{
			if (Parent == parent)
				return;

			Assert.That(Parent != this, "Detected a loop in the visual tree.");
			Assert.That(parent == null || Parent == null, "The element is already attached to the visual tree.");

			// The old parent's layout must be invalidated
			Parent?.SetDirtyState(true, true);

			if (parent != null)
			{
				// Add parent first, then attach
				Parent = parent;
				IsAttachedToRoot = parent.IsAttachedToRoot;
			}
			else
			{
				// Detach first, then remove parent
				IsAttachedToRoot = false;
				Parent = null;
			}

			// The new parent's layout must be invalidated and the visibility state must be recomputed
			Parent?.SetDirtyState(true, true);
			UpdateVisibleState();

			// Changing the parent possibly invalidates the inherited properties of this UI element and its children
			if (HasInheritedFont)
				Font = null;

			if (HasInheritedForeground)
				Foreground = Colors.Transparent;
		}

		/// <summary>
		///   Updates the visual offsets of this UI element and all of its children.
		/// </summary>
		/// <param name="visualOffset">The visual offset that should be applied to the UI element.</param>
		internal void UpdateVisualOffsets(Vector2 visualOffset)
		{
			if (!IsVisualOffsetDirty && visualOffset == _previousVisualOffset)
				return;

			_previousVisualOffset = visualOffset;
			VisualOffset = _relativeVisualOffset + visualOffset;

			foreach (var child in GetChildren())
				child.UpdateVisualOffsets(VisualOffset + GetAdditionalChildrenOffset());

			IsVisualOffsetDirty = false;
		}

		/// <summary>
		///   Gets the additional offset that should be applied to the visual offset of the UI element's children.
		/// </summary>
		protected virtual Vector2 GetAdditionalChildrenOffset()
		{
			return Vector2.Zero;
		}

		/// <summary>
		///   Adapts the size of the UI element according to the layouting information.
		/// </summary>
		/// <param name="size">The size that should be adapted.</param>
		/// <param name="availableSize">The size that is available to this UI element.</param>
		private void AdaptSize(ref Size size, Size availableSize)
		{
			// When stretching horizontally, fill all available width
			if (HorizontalAlignment == HorizontalAlignment.Stretch)
				size.Width = availableSize.Width;

			// When stretching vertically, fill all available height
			if (VerticalAlignment == VerticalAlignment.Stretch)
				size.Height = availableSize.Height;

			// Use the requested width if one is set
			if (!Single.IsNaN(Width))
				size.Width = Width;

			// Use the requested height if one is set
			if (!Single.IsNaN(Height))
				size.Height = Height;

			// Clamp the width and the height to the minimum and maximum values
			size.Width = MathUtils.Clamp(size.Width, MinWidth, MaxWidth);
			size.Height = MathUtils.Clamp(size.Height, MinHeight, MaxHeight);
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
		protected abstract Size ArrangeCore(Size finalSize);

		/// <summary>
		///   Computes the alignment offset based on the available size and the actual size of the UI element.
		/// </summary>
		/// <param name="availableSize">The available size the UI element should be aligned in.</param>
		private Vector2 ComputeAlignmentOffset(Size availableSize)
		{
			var offset = Vector2.Zero;

			switch (HorizontalAlignment)
			{
				case HorizontalAlignment.Stretch:
				case HorizontalAlignment.Center:
					offset.X = (availableSize.Width - RenderSize.Width + Margin.Left - Margin.Right) / 2;
					break;
				case HorizontalAlignment.Left:
					offset.X = Margin.Left;
					break;
				case HorizontalAlignment.Right:
					offset.X = availableSize.Width - RenderSize.Width - Margin.Right;
					break;
				default:
					throw new InvalidOperationException("Unexpected alignment.");
			}

			switch (VerticalAlignment)
			{
				case VerticalAlignment.Stretch:
				case VerticalAlignment.Center:
					offset.Y = (availableSize.Height - RenderSize.Height + Margin.Top - Margin.Bottom) / 2;
					break;
				case VerticalAlignment.Top:
					offset.Y = Margin.Top;
					break;
				case VerticalAlignment.Bottom:
					offset.Y = availableSize.Height - RenderSize.Height - Margin.Bottom;
					break;
				default:
					throw new InvalidOperationException("Unexpected alignment.");
			}

			offset.X = Math.Max(0, offset.X);
			offset.Y = Math.Max(0, offset.Y);
			return offset;
		}

		/// <summary>
		///   Increases the size to encompass the margin. For instance, if the width is 10 and the left and right margins are 2 and
		///   3, the returned size has a width of 10 + 2 + 3 = 15.
		/// </summary>
		/// <param name="size">The size the margin should be added to.</param>
		private Size IncreaseByMargin(Size size)
		{
			return new Size(size.Width + Margin.Left + Margin.Right, size.Height + Margin.Top + Margin.Bottom);
		}

		/// <summary>
		///   Decreases the size to encompass the margin. For instance, if the width is 10 and the left and right margins are 2 and
		///   3, the returned size has a width of 10 - 2 - 3 = 5.
		/// </summary>
		/// <param name="size">The size the margin should be added to.</param>
		private Size DecreaseByMargin(Size size)
		{
			return new Size(size.Width - Margin.Left - Margin.Right, size.Height - Margin.Top - Margin.Bottom);
		}

		/// <summary>
		///   Invoked when the children of the UI element have changed.
		/// </summary>
		protected internal virtual void OnChildrenChanged()
		{
		}

		/// <summary>
		///   Invoked when the IsFocus property of the UI element has changed.
		/// </summary>
		protected internal virtual void OnFocusChanged()
		{
		}

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected internal virtual UIElement GetChild(int index)
		{
			Assert.That(false, "This visual does not have any children.");
			return null;
		}

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the UI element.
		/// </summary>
		protected abstract Enumerator<UIElement> GetChildren();

		/// <summary>
		///   Invoked when the UI element is now (transitively) attached to the root of a tree.
		/// </summary>
		protected virtual void OnAttached()
		{
		}

		/// <summary>
		///   Invoked when the UI element is no longer (transitively) attached to the root of a tree.
		/// </summary>
		protected virtual void OnDetached()
		{
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		internal void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			if (Visibility != Visibility.Visible)
				return;

			DrawCore(spriteBatch);
			DrawChildren(spriteBatch);
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected virtual void DrawCore(SpriteBatch spriteBatch)
		{
			if (Background != Colors.Transparent)
				spriteBatch.Draw(VisualArea, Background);
		}

		/// <summary>
		///   Draws the child UI elements of the current UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element's children.</param>
		protected virtual void DrawChildren(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			// We draw the children on a higher layer to avoid draw ordering issues.
			++spriteBatch.Layer;

			foreach (var child in GetChildren())
				child.Draw(spriteBatch);

			--spriteBatch.Layer;
		}

		/// <summary>
		///   Provides state information for an UI element.
		/// </summary>
		[Flags]
		private enum State
		{
			AttachedToRoot = 1,
			MeasureDirty = 2,
			ArrangeDirty = 4,
			VisualOffsetDirty = 8,
			InheritsFont = 16,
			InheritsForeground = 32,
			IsMouseOver = 64,
			IsActive = 128,
			IsVisible = 256,
			IsFocusable = 512,
			IsFocused = 1024,
			IsHitTestVisible = 2048,
			CapturesInput = 4096,
			AutoFocus = 8192
		}
	}
}