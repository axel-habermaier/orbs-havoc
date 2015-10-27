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
	using System.Numerics;
	using Platform.Input;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents an UI element.
	/// </summary>
	public abstract class UIElement : DisposableObject
	{
		private Rectangle _area;
		private State _state = State.Normal;

		/// <summary>
		///   The UI elements's style when it is active.
		/// </summary>
		public Style? ActiveStyle = null;

		/// <summary>
		///   The UI elements's style when it is hovered.
		/// </summary>
		public Style? HoveredStyle = null;

		/// <summary>
		///   The UI elements's style when it is neither hovered nor active.
		/// </summary>
		public Style NormalStyle;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		protected UIElement()
		{
			NormalStyle = new Style
			{
				Background = Colors.Transparent,
				Foreground = Colors.White,
				BorderColor = Colors.Transparent,
				BorderThickness = 0
			};
		}

		/// <summary>
		///   Gets or sets a value indicating whether the UI element is visible.
		/// </summary>
		public bool IsVisible { get; set; } = true;

		/// <summary>
		///   Gets or sets a value indicating whether the UI element should be sized to its content instead of having a guaranteed
		///   minimum size.
		/// </summary>
		public bool SizeToContent { get; set; }

		/// <summary>
		///   Gets the UI element's area.
		/// </summary>
		public Rectangle Area
		{
			get
			{
				Assert.NotDisposed(this);

				if (!IsVisible)
					return Rectangle.Empty;

				var area = ContentArea;
				var style = Style;

				var size = new Size(
					area.Width + Margin.Width + Padding.Width + 2 * style.BorderThickness,
					area.Height + Margin.Height + Padding.Height + 2 * style.BorderThickness);

				if (!SizeToContent)
					size = new Size(Math.Max(_area.Width, size.Width), Math.Max(_area.Height, size.Height));

				return new Rectangle(_area.Position, size);
			}
			set
			{
				var style = Style;
				var positionOffset = new Vector2(
					Margin.Left + Padding.Left + style.BorderThickness,
					Margin.Top + Padding.Top + style.BorderThickness);

				var size = new Size(
					value.Size.Width - Margin.Width - Padding.Width - 2 * style.BorderThickness,
					value.Size.Height - Margin.Height - Padding.Height - 2 * style.BorderThickness);

				_area = value;
				ContentArea = new Rectangle(value.Position + positionOffset, size);
			}
		}

		/// <summary>
		///   Gets or sets the UI element's margin.
		/// </summary>
		public Thickness Margin { get; set; }

		/// <summary>
		///   Gets or sets the UI element's padding, i.e., the margin between its contents and its border.
		/// </summary>
		public Thickness Padding { get; set; }

		/// <summary>
		///   Gets the currently active style.
		/// </summary>
		public Style Style
		{
			get
			{
				switch (_state)
				{
					case State.Normal:
						return NormalStyle;
					case State.Hovered:
						return HoveredStyle ?? NormalStyle;
					case State.Active:
						return ActiveStyle ?? NormalStyle;
					default:
						throw new InvalidOperationException("Unknown state.");
				}
			}
		}

		/// <summary>
		///   Gets or sets the area occupied by the UI element's contents.
		/// </summary>
		public abstract Rectangle ContentArea { get; set; }

		/// <summary>
		///   Changes the UI element's current state.
		/// </summary>
		private void ChangeState(State state)
		{
			if (_state == state)
				return;

			_state = state;
			ApplyStyle(Style);
		}

		/// <summary>
		///   Centers the UI element horizontally within the given area.
		/// </summary>
		/// <param name="area">The area the UI element should be centered horizontally in.</param>
		public void CenterHorizontally(Rectangle area)
		{
			var x = area.Left + Margin.Left - Margin.Right + MathUtils.Round((area.Width - Area.Width) / 2);
			Area = Area.Offset(new Vector2(x, 0));
		}

		/// <summary>
		///   Centers the UI element horizontally within the given area.
		/// </summary>
		/// <param name="area">The area the UI element should be centered horizontally in.</param>
		public void CenterVertically(Rectangle area)
		{
			var offset = area.Left + Margin.Left - Margin.Right + MathUtils.Round((area.Width - Area.Width) / 2);
			Area = Area.Offset(offset, 0);
		}

		/// <summary>
		///   Aligns the UI element to the bottom of the given area.
		/// </summary>
		/// <param name="area">The area the UI element should be aligned in.</param>
		public void AlignBottom(Rectangle area)
		{
			var offset = area.Height - Area.Height;
			Area = _area.MoveTo(_area.Left, offset);
		}

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			if (!IsVisible)
				return;

			var style = Style;
			if (style.Background != Colors.Transparent)
				spriteBatch.Draw(Area, style.Background);

			if (style.BorderThickness > 0 && style.BorderColor != Colors.Transparent)
				spriteBatch.DrawOutline(Area, style.BorderColor, style.BorderThickness);

			++spriteBatch.Layer;
			DrawCore(spriteBatch);
			--spriteBatch.Layer;
		}

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected abstract void DrawCore(SpriteBatch spriteBatch);

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
		}

		/// <summary>
		///   Applies the given style to the UI element.
		/// </summary>
		/// <param name="style">The style that should be applied.</param>
		protected virtual void ApplyStyle(Style style)
		{
		}

		/// <summary>
		///   Invoked when a mouse button was pressed while the UI element was hovered.
		/// </summary>
		/// <param name="button">The button that has been pressed.</param>
		/// <param name="doubleClicked">Indicates whether the UI element has been double clicked.</param>
		public virtual void OnMouseDown(MouseButton button, bool doubleClicked)
		{
			if (button == MouseButton.Left)
				ChangeState(State.Active);
		}

		/// <summary>
		///   Invoked when a mouse button was released while the UI element was hovered.
		/// </summary>
		/// <param name="button">The button that has been released.</param>
		public virtual void OnMouseUp(MouseButton button)
		{
			ChangeState(State.Hovered);
		}

		/// <summary>
		///   Invoked when the mouse starts hovering the UI element.
		/// </summary>
		public virtual void OnMouseEntered()
		{
			ChangeState(State.Hovered);
		}

		/// <summary>
		///   Invoked when the mouse no longer hovers the UI element.
		/// </summary>
		public virtual void OnMouseLeft()
		{
			ChangeState(State.Normal);
		}

		private enum State
		{
			Normal,
			Hovered,
			Active
		}
	}
}