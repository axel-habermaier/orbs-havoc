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
	using Platform.Input;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a button.
	/// </summary>
	public sealed class Button : UIElement
	{
		private readonly Label _label;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="label">The text shown on the button.</param>
		public Button(string label = "")
		{
			Assert.ArgumentNotNull(label, nameof(label));
			_label = new Label(label) { TextAlignment = TextAlignment.Centered | TextAlignment.Middle };

			NormalStyle = new Style
			{
				Background = new Color(0x5F00588B),
				Foreground = Colors.White,
				BorderColor = new Color(0xFF055674),
				BorderThickness = 1
			};

			HoveredStyle = new Style
			{
				Background = new Color(0x5F0082CE),
				Foreground = Colors.White,
				BorderColor = new Color(0xFF055674),
				BorderThickness = 1
			};

			ActiveStyle = new Style
			{
				Background = new Color(0x5F009CF7),
				Foreground = Colors.White,
				BorderColor = new Color(0xFF055674),
				BorderThickness = 1
			};

			Padding = new Thickness(7, 5, 7, 5);
		}

		/// <summary>
		///   Gets or sets the font used to draw the button's label.
		/// </summary>
		public Font Font
		{
			get { return _label.Font; }
			set { _label.Font = value; }
		}

		/// <summary>
		///   Gets or sets the button's label.
		/// </summary>
		public string Label
		{
			get { return _label.Text; }
			set { _label.Text = value; }
		}

		/// <summary>
		///   Gets or sets the callback that is invoked when the button has been clicked.
		/// </summary>
		public Action Clicked { get; set; }

		/// <summary>
		///   Gets the area occupied by the UI element's contents.
		/// </summary>
		public override Rectangle ContentArea
		{
			get { return _label.ContentArea; }
			set { _label.ContentArea = value; }
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_label.SafeDispose();
		}

		/// <summary>
		///   Invoked when a mouse button was released while the UI element was hovered.
		/// </summary>
		/// <param name="button">The button that has been released.</param>
		public override void OnMouseUp(MouseButton button)
		{
			base.OnMouseUp(button);
			Clicked?.Invoke();
		}

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			_label.Draw(spriteBatch);
		}
	}
}