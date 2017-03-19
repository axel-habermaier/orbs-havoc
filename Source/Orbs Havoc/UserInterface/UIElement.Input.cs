// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.UserInterface
{
	using Input;
	using Utilities;

	// Contains input handling method that would better fir into RootUIElement; however, that would require making
	// certain methods and properties public, which we don't want to do. The reason is that RootUIElement is not
	// allowed to call protected members of other UIElement instances... annoying
	partial class UIElement
	{
		/// <summary>
		///   Handles a mouse entered event.
		/// </summary>
		protected static void OnMouseEnter(UIElement hoveredElement, MouseEventArgs e)
		{
			Assert.ArgumentNotNull(e, nameof(e));

			while (hoveredElement != null)
			{
				if (!hoveredElement.IsMouseOver)
				{
					hoveredElement.IsMouseOver = true;
					hoveredElement.HoveredStyle?.Invoke(hoveredElement);
					hoveredElement.OnMouseEnter(e);
				}

				hoveredElement = hoveredElement.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse left event.
		/// </summary>
		protected static void OnMouseLeave(UIElement unhoveredElement, MouseEventArgs e)
		{
			Assert.ArgumentNotNull(e, nameof(e));

			while (unhoveredElement != null)
			{
				if (unhoveredElement.IsMouseOver)
				{
					unhoveredElement.IsMouseOver = false;
					unhoveredElement.IsActive = false;
					unhoveredElement.NormalStyle?.Invoke(unhoveredElement);
					unhoveredElement.OnMouseLeave(e);
				}

				unhoveredElement = unhoveredElement.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse pressed event.
		/// </summary>
		private static void OnPreviewMouseDown(UIElement element, MouseButtonEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewMouseDown(element.Parent, e);

			if (e.Handled)
				return;

			element.OnPreviewMouseDown(e);
			element.HandleInputEvent(e, isPreview: true);
		}

		/// <summary>
		///   Handles a mouse pressed event.
		/// </summary>
		protected static void OnMouseDown(UIElement element, MouseButtonEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewMouseDown(element, e);

			while (element != null && !e.Handled)
			{
				if (element.CanBeFocused)
				{
					element.Focus();
					e.Handled = true;
				}

				element.HandleInputEvent(e, isPreview: false);
				element.ActiveStyle?.Invoke(element);
				element.OnMouseDown(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private static void OnPreviewMouseUp(UIElement element, MouseButtonEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewMouseUp(element.Parent, e);

			if (e.Handled)
				return;

			element.OnPreviewMouseUp(e);
			element.HandleInputEvent(e, isPreview: true);
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		protected static void OnMouseUp(UIElement element, MouseButtonEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewMouseUp(element, e);

			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e, isPreview: false);
				element.HoveredStyle?.Invoke(element);
				element.OnMouseUp(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private static void OnPreviewMouseWheel(UIElement element, MouseWheelEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewMouseWheel(element.Parent, e);

			if (e.Handled)
				return;

			element.OnPreviewMouseWheel(e);
			element.HandleInputEvent(e, isPreview: true);
		}

		/// <summary>
		///   Handles a mouse wheel event.
		/// </summary>
		protected static void OnMouseWheel(UIElement element, MouseWheelEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewMouseWheel(element, e);

			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e, isPreview: false);
				element.OnMouseWheel(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private static void OnPreviewKeyDown(UIElement element, KeyEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewKeyDown(element.Parent, e);

			if (e.Handled)
				return;

			element.OnPreviewKeyDown(e);
			element.HandleInputEvent(e, isPreview: true);
		}

		/// <summary>
		///   Handles a key pressed event.
		/// </summary>
		protected static void OnKeyDown(UIElement element, KeyEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewKeyDown(element, e);

			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e, isPreview: false);
				element.OnKeyDown(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private static void OnPreviewKeyUp(UIElement element, KeyEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewKeyUp(element.Parent, e);

			if (e.Handled)
				return;

			element.OnPreviewKeyUp(e);
			element.HandleInputEvent(e, isPreview: true);
		}

		/// <summary>
		///   Handles a key released event.
		/// </summary>
		protected static void OnKeyUp(UIElement element, KeyEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewKeyUp(element, e);

			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e, isPreview: false);
				element.OnKeyUp(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private static void OnPreviewTextEntered(UIElement element, TextInputEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			if (element.Parent != null)
				OnPreviewTextEntered(element.Parent, e);

			if (!e.Handled)
				element.OnPreviewTextEntered(e);
		}

		/// <summary>
		///   Handles a text entered event.
		/// </summary>
		protected static void OnTextEntered(UIElement element, TextInputEventArgs e)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Assert.ArgumentNotNull(e, nameof(e));

			OnPreviewTextEntered(element, e);

			while (element != null && !e.Handled)
			{
				element.OnTextEntered(e);
				element = element.Parent;
			}
		}

		/// <summary>
		///   Invoked when the mouse has entered the UI element.
		/// </summary>
		protected virtual void OnMouseEnter(MouseEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when the mouse has left the UI element.
		/// </summary>
		protected virtual void OnMouseLeave(MouseEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been pressed while hovering the UI element.
		/// </summary>
		protected virtual void OnMouseDown(MouseButtonEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected virtual void OnMouseUp(MouseButtonEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when the mouse wheel has been moved.
		/// </summary>
		protected virtual void OnMouseWheel(MouseWheelEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a key has been pressed while the UI element is focused.
		/// </summary>
		protected virtual void OnKeyDown(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a key has been released while the UI element is focused.
		/// </summary>
		protected virtual void OnKeyUp(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a text has been entered while the UI element is focused.
		/// </summary>
		protected virtual void OnTextEntered(TextInputEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been pressed while hovering the UI element.
		/// </summary>
		protected virtual void OnPreviewMouseDown(MouseButtonEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected virtual void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when the mouse wheel has been moved.
		/// </summary>
		protected virtual void OnPreviewMouseWheel(MouseWheelEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a key has been pressed while the UI element is focused.
		/// </summary>
		protected virtual void OnPreviewKeyDown(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a key has been released while the UI element is focused.
		/// </summary>
		protected virtual void OnPreviewKeyUp(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a text has been entered while the UI element is focused.
		/// </summary>
		protected virtual void OnPreviewTextEntered(TextInputEventArgs e)
		{
		}

		/// <summary>
		///   Handles a non-preview input event.
		/// </summary>
		private void HandleInputEvent(InputEventArgs e, bool isPreview)
		{
			if (_inputBindings == null || !IsAttachedToRoot)
				return;

			foreach (var binding in _inputBindings)
				binding.HandleEvent(e, isPreview);
		}
	}
}