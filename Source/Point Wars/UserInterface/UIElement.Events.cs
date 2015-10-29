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

	public partial class UIElement
	{
		/// <summary>
		///   Invoked when the mouse has entered the UI element.
		/// </summary>
		protected virtual void OnMouseEntered(MouseEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when the mouse has left the UI element.
		/// </summary>
		protected virtual void OnMouseLeft(MouseEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been pressed while hovering the UI element.
		/// </summary>
		protected virtual void OnMousePressed(MouseButtonEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected virtual void OnMouseReleased(MouseButtonEventArgs e)
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
		protected virtual void OnKeyPressed(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a key has been released while the UI element is focused.
		/// </summary>
		protected virtual void OnKeyReleased(KeyEventArgs e)
		{
		}

		/// <summary>
		///   Invoked when a text has been entered while the UI element is focused.
		/// </summary>
		protected virtual void OnTextEntered(TextInputEventArgs e)
		{
		}

		/// <summary>
		///   Handles an input event.
		/// </summary>
		private void HandleInputEvent(InputEventArgs e)
		{
			if (_inputBindings == null || !IsAttachedToRoot)
				return;

			foreach (var binding in _inputBindings)
				binding.HandleEvent(e);
		}
	}
}