﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using System.Numerics;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a scrollable area.
	/// </summary>
	internal class ScrollViewer : Control
	{
		private Size _arrangedContentSize;
		private Camera _camera;
		private Size _measuredContentSize;
		private Panel _panel;
		private UIElement _scrolledChild;
		private Vector2 _scrollOffset;

		/// <summary>
		///   Gets or sets the horizontal scroll speed.
		/// </summary>
		public float HorizontalScrollStep { get; set; } = 10;

		/// <summary>
		///   Gets or sets the vertical scroll speed.
		/// </summary>
		public float VerticalScrollStep { get; set; } = 10;

		/// <summary>
		///   Gets or sets the scroll offset, taking the alignment of the scroll child into account.
		/// </summary>
		private Vector2 ScrollOffset
		{
			get
			{
				var offset = _scrollOffset;

				if (_scrolledChild != null && _scrolledChild.HorizontalAlignment == HorizontalAlignment.Right)
					offset.X = _scrolledChild.ActualWidth - ActualWidth - offset.X;

				if (_scrolledChild != null && _scrolledChild.VerticalAlignment == VerticalAlignment.Bottom)
					offset.Y = _scrolledChild.ActualHeight - ActualHeight - offset.Y;

				return offset;
			}
		}

		/// <summary>
		///   Gets the area the scrolled content is presented in.
		/// </summary>
		public Rectangle ScrollArea => new Rectangle(VisualOffset + ScrollOffset, ActualWidth, ActualHeight);

		/// <summary>
		///   Ensures that the scroll offset is still valid after a size changed. When the scroll viewer's size is increased, more
		///   content fits into its scroll area, possibly resulting in the scroll viewer displaying nothing if the scroll offset
		///   is not reclamped to the new size.
		/// </summary>
		protected override void OnActualSizeChanged()
		{
			Scroll(Vector2.Zero);
		}

		/// <summary>
		///   Invoked when the content has been changed.
		/// </summary>
		protected override void OnContentChanged()
		{
			if (_panel != null)
				_panel.ScollViewer = null;

			_scrolledChild = Content as UIElement;
			_panel = ContentPresenter.Content as Panel;

			if (_panel != null)
				_panel.ScollViewer = this;
		}

		/// <summary>
		///   Scrolls the content by the given delta.
		/// </summary>
		/// <param name="delta">The delta in the scrolling position.</param>
		private void Scroll(Vector2 delta)
		{
			if (_scrolledChild != null && _scrolledChild.VerticalAlignment == VerticalAlignment.Bottom)
				delta.Y *= -1;

			var offset = _scrollOffset + delta;
			var width = _arrangedContentSize.Width;
			var height = _arrangedContentSize.Height;

			// Clamp to the range
			offset.X = MathUtils.Clamp(offset.X, 0, Math.Max(0, width - HorizontalScrollStep));
			offset.Y = MathUtils.Clamp(offset.Y, 0, Math.Max(0, height - VerticalScrollStep));

			_scrollOffset = offset;
		}

		/// <summary>
		///   Performs a detailed hit test for the given position. The position is guaranteed to lie within the UI element's
		///   bounds. This method should be overridden to implement special hit testing logic that is more precise than a
		///   simple bounding box check.
		/// </summary>
		/// <param name="position">The position that should be checked for a hit.</param>
		/// <returns>Returns true if the UI element is hit; false, otherwise.</returns>
		protected override bool HitTestCore(Vector2 position)
		{
			return true;
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
			_measuredContentSize = base.MeasureCore(availableSize);
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
			_arrangedContentSize = base.ArrangeCore(_measuredContentSize);
			return finalSize;
		}

		/// <summary>
		///   Draws the child UI elements of the current UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element's children.</param>
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			var width = MathUtils.Round(ActualWidth);
			var height = MathUtils.Round(ActualHeight);
			var x = MathUtils.Round(VisualOffset.X);
			var y = MathUtils.Round(VisualOffset.Y);

			var scissorArea = spriteBatch.RenderState.ScissorArea;
			var camera = spriteBatch.RenderState.Camera;

			spriteBatch.RenderState.ScissorArea = new Rectangle(x, y, width, height);
			spriteBatch.RenderState.Camera = _camera;

			_camera.Position = new Vector2(-MathUtils.RoundIntegral(ScrollOffset.X), -MathUtils.RoundIntegral(ScrollOffset.Y));

			base.DrawChildren(spriteBatch);

			spriteBatch.RenderState.ScissorArea = scissorArea;
			spriteBatch.RenderState.Camera = camera;
		}

		/// <summary>
		///   Invoked when the UI element is now (transitively) attached to the root of a tree.
		/// </summary>
		protected override void OnAttached()
		{
			_camera = new Camera();
		}

		/// <summary>
		///   Invoked when the UI element is no longer (transitively) attached to the root of a tree.
		/// </summary>
		protected override void OnDetached()
		{
			_camera.SafeDispose();
			_camera = null;
		}

		/// <summary>
		///   Scrolls up by a step.
		/// </summary>
		public void ScrollUp()
		{
			Scroll(new Vector2(0, -VerticalScrollStep));
		}

		/// <summary>
		///   Scrolls down by a step.
		/// </summary>
		public void ScrollDown()
		{
			Scroll(new Vector2(0, VerticalScrollStep));
		}

		/// <summary>
		///   Scrolls left by a step.
		/// </summary>
		public void ScrollLeft()
		{
			Scroll(new Vector2(-HorizontalScrollStep, 0));
		}

		/// <summary>
		///   Scrolls right by a step.
		/// </summary>
		public void ScrollRight()
		{
			Scroll(new Vector2(HorizontalScrollStep, 0));
		}

		/// <summary>
		///   Scrolls to the top of the content area.
		/// </summary>
		public void ScrollToTop()
		{
			Scroll(new Vector2(0, Single.NegativeInfinity));
		}

		/// <summary>
		///   Scrolls to the bottom of the content area.
		/// </summary>
		public void ScrollToBottom()
		{
			Scroll(new Vector2(0, Single.PositiveInfinity));
		}

		/// <summary>
		///   Scrolls to the left of the content area.
		/// </summary>
		public void ScrollToLeft()
		{
			Scroll(new Vector2(Single.NegativeInfinity, 0));
		}

		/// <summary>
		///   Scrolls to the right of the content area.
		/// </summary>
		public void ScrollToRight()
		{
			Scroll(new Vector2(Single.PositiveInfinity, 0));
		}
	}
}