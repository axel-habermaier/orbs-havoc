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
	using Input;
	using Platform.Input;
	using Rendering;

	/// <summary>
	///   Represents a button control.
	/// </summary>
	public class Button : Control
	{
		private static readonly ControlTemplate DefaultTemplate = (out UIElement templateRoot, out ContentPresenter contentPresenter) =>
		{
			contentPresenter = new ContentPresenter
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			templateRoot = new Border
			{
				Child = contentPresenter,
				BorderThickness = new Thickness(1),
				BorderColor = new Color(0xFF055674),
				NormalStyle = element => ((Border)element).Background = new Color(0x5F00588B),
				HoveredStyle = element => ((Border)element).Background = new Color(0x5F0082CE),
				ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7),
				Padding = new Thickness(7, 6, 7, 7)
			};
		};

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Button()
			: base(DefaultTemplate)
		{
		}

		/// <summary>
		///   Raised when the button has been clicked.
		/// </summary>
		public Action Click;

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.Button != MouseButton.Left || e.Handled)
				return;

			e.Handled = true;
			Click?.Invoke();
			Focus();
		}
	}
}