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
	using Input;

	// Contains input handling method that would better fir into RootUIElement; however, that would require making
	// certain methods and properties public, which we don't want to do. The reason is that RootUIElement is not
	// allowed to call protected members of other UIElement instances... annoying
	partial class UIElement
	{
		/// <summary>
		///   Handles a mouse entered event.
		/// </summary>
		protected static void OnMouseEntered(UIElement hoveredElement, MouseEventArgs e)
		{
			while (hoveredElement != null)
			{
				if (!hoveredElement.IsMouseOver)
				{
					hoveredElement.IsMouseOver = true;
					hoveredElement.HoveredStyle?.Invoke(hoveredElement);
					hoveredElement.OnMouseEntered(e);
				}

				hoveredElement = hoveredElement.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse left event.
		/// </summary>
		protected static void OnMouseLeft(UIElement unhoveredElement, MouseEventArgs e)
		{
			while (unhoveredElement != null)
			{
				if (unhoveredElement.IsMouseOver)
				{
					unhoveredElement.IsMouseOver = false;
					unhoveredElement.IsActive = false;
					unhoveredElement.NormalStyle?.Invoke(unhoveredElement);
					unhoveredElement.OnMouseLeft(e);
				}

				unhoveredElement = unhoveredElement.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse pressed event.
		/// </summary>
		protected static void OnMousePressed(UIElement element, MouseButtonEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				if (element.CanBeFocused)
				{
					element.Focus();
					e.Handled = true;
				}

				element.HandleInputEvent(e);
				element.ActiveStyle?.Invoke(element);
				element.OnMousePressed(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		protected static void OnMouseReleased(UIElement element, MouseButtonEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e);
				element.HoveredStyle?.Invoke(element);
				element.OnMouseReleased(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a mouse wheel event.
		/// </summary>
		protected static void OnMouseWheel(UIElement element, MouseWheelEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e);
				element.OnMouseWheel(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a key pressed event.
		/// </summary>
		protected static void OnKeyPressed(UIElement element, KeyEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e);
				element.OnKeyPressed(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a key released event.
		/// </summary>
		protected static void OnKeyReleased(UIElement element, KeyEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				element.HandleInputEvent(e);
				element.OnKeyReleased(e);

				element = element.Parent;
			}
		}

		/// <summary>
		///   Handles a text entered event.
		/// </summary>
		protected static void OnTextEntered(UIElement element, TextInputEventArgs e)
		{
			while (element != null && !e.Handled)
			{
				element.OnTextEntered(e);
				element = element.Parent;
			}
		}
	}
}