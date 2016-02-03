// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using System.Numerics;
	using Input;
	using Platform.Input;
	using Rendering;

	/// <summary>
	///   Represents a checkbox control that the user can select and clear.
	/// </summary>
	public class CheckBox : Control
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
				ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7)
			};
		};

		private bool _isChecked;

		/// <summary>
		///   Raised when the checkbox' selection state has changed.
		/// </summary>
		public Action Changed;

		/// <summary>
		///   Raised when the checkbox has been checked.
		/// </summary>
		public Action Checked;

		/// <summary>
		///   Raised when the checkbox has been unchecked.
		/// </summary>
		public Action Unchecked;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public CheckBox()
			: base(DefaultTemplate)
		{
			Width = 20;
			Height = 20;
			Foreground = new Color(0xFF055674);
		}

		/// <summary>
		///   Gets a value indicating whethre the checkbox is checked.
		/// </summary>
		public bool IsChecked
		{
			get { return _isChecked; }
			set
			{
				if (_isChecked == value)
					return;

				_isChecked = value;
				Changed?.Invoke();

				if (IsChecked)
					Checked?.Invoke();
				else
					Unchecked?.Invoke();
			}
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.Button != MouseButton.Left || e.Handled)
				return;

			e.Handled = true;
			Focus();

			IsChecked = !IsChecked;
		}

		/// <summary>
		///   Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			if (!IsChecked)
				return;

			var area = VisualArea;

			spriteBatch.RenderState.Layer += 1;
			spriteBatch.DrawLine(area.TopLeft + new Vector2(2, 2), area.BottomRight - new Vector2(2, 2), Foreground, 2);
			spriteBatch.DrawLine(area.TopRight + new Vector2(-2, 2), area.BottomLeft - new Vector2(-2, 2), Foreground, 2);
			spriteBatch.RenderState.Layer -= 1;
		}
	}
}