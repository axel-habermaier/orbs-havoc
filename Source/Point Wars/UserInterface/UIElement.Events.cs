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
		//		/// <summary>
		//		///   Raised when a key is pressed while the UI element is focused.
		//		/// </summary>
		//		public event Action<KeyEventArgs> KeyDown;
		//
		//		/// <summary>
		//		///   Raised when a key is released while the UI element is focused.
		//		/// </summary>
		//		public event Action<KeyEventArgs> KeyUp;
		//
		//		/// <summary>
		//		///   Raised when the UI element gets text input.
		//		/// </summary>
		//		public event Action<TextInputEventArgs> TextInput;
		//
		//		/// <summary>
		//		///   Raised when a mouse button is pressed while the mouse is over the UI element.
		//		/// </summary>
		//		public event Action<MouseButtonEventArgs> MouseDown;
		//
		//		/// <summary>
		//		///   Raised when a mouse button is released while the mouse is over the UI element.
		//		/// </summary>
		//		public event Action<MouseButtonEventArgs> MouseUp;
		//
		//		/// <summary>
		//		///   Raised when the mouse wheel has changed while the mouse is over the UI element.
		//		/// </summary>
		//		public event Action<MouseWheelEventArgs> MouseWheel;
		//
		//		/// <summary>
		//		///   Raised when the mouse has moved while the mouse is over the UI element.
		//		/// </summary>
		//		public event Action<MouseEventArgs> MouseMove;
		//
		//		/// <summary>
		//		///   Raised when the mouse has entered the bounds of the UI element.
		//		/// </summary>
		//		public event Action<MouseEventArgs> MouseEnter;
		//
		//		/// <summary>
		//		///   Raised when the mouse has left the bounds of the UI element.
		//		/// </summary>
		//		public event Action<MouseEventArgs> MouseLeave;
		/// <summary>
		///   Invoked when the mouse has entered the UI element.
		/// </summary>
		protected virtual void OnMouseEntered(MouseEventArgs args)
		{
			IsMouseOver = true;
			HoveredStyle?.Invoke(this);
		}

		/// <summary>
		///   Invoked when the mouse has left the UI element.
		/// </summary>
		protected virtual void OnMouseLeft(MouseEventArgs args)
		{
			IsMouseOver = false;
			IsActive = false;
			NormalStyle?.Invoke(this);
		}

		/// <summary>
		///   Invoked when a mouse button has been pressed while hovering the UI element.
		/// </summary>
		protected virtual void OnMousePressed(MouseButtonEventArgs args)
		{
			HandleInputEvent(args);
			ActiveStyle?.Invoke(this);
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected virtual void OnMouseReleased(MouseButtonEventArgs args)
		{
			HandleInputEvent(args);
			HoveredStyle?.Invoke(this);
		}

		/// <summary>
		///   Invoked when a key has been pressed while the UI element is focused.
		/// </summary>
		protected virtual void OnKeyPressed(KeyEventArgs args)
		{
			HandleInputEvent(args);
		}

		/// <summary>
		///   Invoked when a key has been released while the UI element is focused.
		/// </summary>
		protected virtual void OnKeyReleased(KeyEventArgs args)
		{
			HandleInputEvent(args);
		}

		/// <summary>
		///   Invoked when a text has been entered while the UI element is focused.
		/// </summary>
		protected virtual void OnTextEntered(TextInputEventArgs args)
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