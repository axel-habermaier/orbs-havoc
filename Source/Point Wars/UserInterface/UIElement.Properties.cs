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
	using System.Diagnostics;
	using System.Numerics;
	using Controls;
	using Input;
	using Rendering;
	using Utilities;

	public partial class UIElement
	{
		private Action<UIElement> _activeStyle;
		private float _bottom = Single.NaN;
		private Dock _dock = Dock.Left;
		private Font _font;
		private Color _foreground = Colors.Transparent;
		private float _height = Single.NaN;
		private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Stretch;
		private Action<UIElement> _hoveredStyle;
		private float _left = Single.NaN;
		private Thickness _margin;
		private float _maxHeight = Single.PositiveInfinity;
		private float _maxWidth = Single.PositiveInfinity;
		private float _minHeight;
		private float _minWidth;
		private Action<UIElement> _normalStyle;
		private float _right = Single.NaN;
		private float _top = Single.NaN;
		private VerticalAlignment _verticalAlignment = VerticalAlignment.Stretch;
		private Visibility _visibility = Visibility.Visible;
		private float _width = Single.NaN;
		private int _zIndex;

		/// <summary>
		///   Gets or sets the UI element's normal style when it is neither hovered nor active.
		/// </summary>
		public Action<UIElement> NormalStyle
		{
			get { return _normalStyle; }
			set
			{
				_normalStyle = value;
				_normalStyle?.Invoke(this);
			}
		}

		/// <summary>
		///   Gets or sets the UI element's hovered style.
		/// </summary>
		public Action<UIElement> HoveredStyle
		{
			get { return _hoveredStyle; }
			set
			{
				_hoveredStyle = value;

				if (IsMouseOver)
					_hoveredStyle?.Invoke(this);
			}
		}

		/// <summary>
		///   Gets or sets the UI element's active style.
		/// </summary>
		public Action<UIElement> ActiveStyle
		{
			get { return _activeStyle; }
			set
			{
				_activeStyle = value;

				if (IsActive)
					_activeStyle?.Invoke(this);
			}
		}

		/// <summary>
		///   Gets or sets the background color of the UI element.
		/// </summary>
		public Color Background { get; set; } = Colors.Transparent;

		/// <summary>
		///   Gets or sets the font used for text rendering by the UI element. If no font is set, the font is inherited from the UI
		///   element's parent.
		/// </summary>
		public Font Font
		{
			get { return _font ?? (_font = Parent.Font); }
			set
			{
				if (_font == value)
					return;

				HasInheritedFont = value == null;
				_font = value;

				SetDirtyState(measure: true, arrange: true);

				foreach (var child in GetChildren())
				{
					if (child.HasInheritedFont)
						child._font = null;
				}
			}
		}

		/// <summary>
		///   Gets or sets the foreground color of the UI element.
		/// </summary>
		public Color Foreground
		{
			get { return _foreground == Colors.Transparent ? (_foreground = Parent.Foreground) : _foreground; }
			set
			{
				if (_foreground == value)
					return;

				HasInheritedFont = value == Colors.Transparent;
				_foreground = value;

				SetDirtyState(measure: true, arrange: true);

				foreach (var child in GetChildren())
				{
					if (child.HasInheritedForeground)
						child._foreground = Colors.Transparent;
				}
			}
		}

		/// <summary>
		///   Gets or sets the width of the UI element, measured in pixels.
		/// </summary>
		public float Width
		{
			get { return _width; }
			set
			{
				ValidateWidthHeight(value);

				if (MathUtils.Equals(_width, value))
					return;

				_width = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the height of the UI element, measured in pixels.
		/// </summary>
		public float Height
		{
			get { return _height; }
			set
			{
				ValidateWidthHeight(value);

				if (MathUtils.Equals(_height, value))
					return;

				_height = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the minimum width constraint of the UI element, measured in pixels.
		/// </summary>
		public float MinWidth
		{
			get { return _minWidth; }
			set
			{
				ValidateMinWidthHeight(value);

				if (MathUtils.Equals(_minWidth, value))
					return;

				_minWidth = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the minimum height constraint of the UI element, measured in pixels.
		/// </summary>
		public float MinHeight
		{
			get { return _minHeight; }
			set
			{
				ValidateMinWidthHeight(value);

				if (MathUtils.Equals(_minHeight, value))
					return;

				_minHeight = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the maximum width constraint of the UI element, measured in pixels.
		/// </summary>
		public float MaxWidth
		{
			get { return _maxWidth; }
			set
			{
				ValidateMaxWidthHeight(value);

				if (MathUtils.Equals(_maxWidth, value))
					return;

				_maxWidth = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the maximum height constraint of the UI element, measured in pixels.
		/// </summary>
		public float MaxHeight
		{
			get { return _maxHeight; }
			set
			{
				ValidateMaxWidthHeight(value);

				if (MathUtils.Equals(_maxHeight, value))
					return;

				_maxHeight = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets  the actual width of the UI element, measured in pixels, as determined by the layouting system.
		/// </summary>
		public float ActualWidth { get; private set; }

		/// <summary>
		///   Gets the actual height of the UI element, measured in pixels, as determined by the layouting system.
		/// </summary>
		public float ActualHeight { get; private set; }

		/// <summary>
		///   Gets or sets the outer margin of the UI element.
		/// </summary>
		public Thickness Margin
		{
			get { return _margin; }
			set
			{
				if (_margin == value)
					return;

				_margin = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   The horizontal alignment characteristics of the UI element.
		/// </summary>
		public HorizontalAlignment HorizontalAlignment
		{
			get { return _horizontalAlignment; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));

				if (_horizontalAlignment == value)
					return;

				_horizontalAlignment = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   The vertical alignment characteristics of the UI element.
		/// </summary>
		public VerticalAlignment VerticalAlignment
		{
			get { return _verticalAlignment; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));

				if (_verticalAlignment == value)
					return;

				_verticalAlignment = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the visibility of the UI element.
		/// </summary>
		public Visibility Visibility
		{
			get { return _visibility; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));

				if (_visibility == value)
					return;

				_visibility = value;
				SetDirtyState(measure: true, arrange: true);
				UpdateVisibleState();
			}
		}

		/// <summary>
		///   Gets a value indicating whether the UI element is actually visible in the user interface.
		/// </summary>
		public bool IsVisible { get; protected set; }

		/// <summary>
		///   Gets or sets a value indicating whether the UI element can receive the keyboard focus.
		/// </summary>
		public bool Focusable { get; set; }

		/// <summary>
		///   Gets a value indicating whether the UI element currently has the keyboard focus.
		/// </summary>
		public bool IsFocused { get; private set; }

		/// <summary>
		///   Gets or sets a value indicating whether the UI element participates in hit testing.
		/// </summary>
		public bool IsHitTestVisible { get; set; } = true;

		/// <summary>
		///   Gets the size of the UI element that has been computed by the last measure pass of the layout engine.
		/// </summary>
		public Size DesiredSize { get; private set; }

		/// <summary>
		///   Gets the parent of the UI element.
		/// </summary>
		public UIElement Parent { get; private set; }

		/// <summary>
		///   Gets the final render size of the UI element.
		/// </summary>
		public Size RenderSize { get; internal set; }

		/// <summary>
		///   Gets the absolute visual offset of the UI element for drawing.
		/// </summary>
		protected internal Vector2 VisualOffset { get; private set; }

		/// <summary>
		///   Gets the number of children for this UI element.
		/// </summary>
		protected internal virtual int ChildrenCount => 0;

		/// <summary>
		///   Gets or sets a value indicating whether the UI element is attached to the visual tree's root element.
		/// </summary>
		protected internal bool IsAttachedToRoot
		{
			get { return (_state & State.AttachedToRoot) == State.AttachedToRoot; }
			set
			{
				if (IsAttachedToRoot == value)
					return;

				if (value)
					_state |= State.AttachedToRoot;
				else
					_state &= ~State.AttachedToRoot;

				foreach (var child in GetChildren())
					child.IsAttachedToRoot = value;
			}
		}

		/// <summary>
		///   Gets or sets the distance between the left side of the UI element and its parent Canvas.
		/// </summary>
		public float Left
		{
			get { return _left; }
			set
			{
				Assert.That(!Single.IsInfinity(value), "Invalid value.");

				if (MathUtils.Equals(_left, value))
					return;

				_left = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the distance between the top of the UI element and its parent Canvas.
		/// </summary>
		public float Top
		{
			get { return _top; }
			set
			{
				Assert.That(!Single.IsInfinity(value), "Invalid value.");

				if (MathUtils.Equals(_top, value))
					return;

				_top = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the distance between the right side of the UI element and its parent Canvas.
		/// </summary>
		public float Right
		{
			get { return _right; }
			set
			{
				Assert.That(!Single.IsInfinity(value), "Invalid value.");

				if (MathUtils.Equals(_right, value))
					return;

				_right = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the distance between the bottom of the UI element and its parent Canvas.
		/// </summary>
		public float Bottom
		{
			get { return _bottom; }
			set
			{
				Assert.That(!Single.IsInfinity(value), "Invalid value.");

				if (MathUtils.Equals(_bottom, value))
					return;

				_bottom = value;
				SetDirtyState(measure: false, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the position of the UI element within a dock panel.
		/// </summary>
		public Dock Dock
		{
			get { return _dock; }
			set
			{
				Assert.ArgumentInRange(value, nameof(value));

				if (_dock == value)
					return;

				_dock = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the z-index of the UI element.
		/// </summary>
		public int ZIndex
		{
			get { return _zIndex; }
			set
			{
				if (_zIndex == value)
					return;

				_zIndex = value;

				var panel = Parent as Panel;
				panel?.ZIndicesChanged();
			}
		}

		/// <summary>
		///   Gets a value indicating whether the UI element can receive the keyboard focus.
		/// </summary>
		internal bool CanBeFocused => IsAttachedToRoot && Focusable && IsVisible;

		/// <summary>
		///   Gets the list of input bindings associated with this UI element.
		/// </summary>
		public List<InputBinding> InputBindings
			=> _inputBindings ?? (_inputBindings = new List<InputBinding>());

		/// <summary>
		///   Gets the root element the UI element is contained in or null if it isn't contained in any root element.
		/// </summary>
		internal RootUIElement RootElement => GetRootElement(this);

		/// <summary>
		///   A value indicating whether the UI element's cached measured layout is dirty and needs to be updated.
		/// </summary>
		private bool IsMeasureDataDirty
		{
			get { return (_state & State.MeasureDirty) == State.MeasureDirty; }
			set
			{
				if (value)
					_state |= State.MeasureDirty;
				else
					_state &= ~State.MeasureDirty;
			}
		}

		/// <summary>
		///   A value indicating whether the UI element's cached arranged layout is dirty and needs to be updated.
		/// </summary>
		private bool IsArrangeDataDirty
		{
			get { return (_state & State.ArrangeDirty) == State.ArrangeDirty; }
			set
			{
				if (value)
					_state |= State.ArrangeDirty;
				else
					_state &= ~State.ArrangeDirty;
			}
		}

		/// <summary>
		///   A value indicating whether the UI element's cached visual offset is dirty and needs to be updated.
		/// </summary>
		private bool IsVisualOffsetDirty
		{
			get { return (_state & State.VisualOffsetDirty) == State.VisualOffsetDirty; }
			set
			{
				if (value)
					_state |= State.VisualOffsetDirty;
				else
					_state &= ~State.VisualOffsetDirty;
			}
		}

		/// <summary>
		///   A value indicating whether the UI element inherits its font from its parent.
		/// </summary>
		private bool HasInheritedFont
		{
			get { return (_state & State.InheritsFont) == State.InheritsFont; }
			set
			{
				if (value)
					_state |= State.InheritsFont;
				else
					_state &= ~State.InheritsFont;
			}
		}

		/// <summary>
		///   A value indicating whether the UI element inherits its foreground color from its parent.
		/// </summary>
		private bool HasInheritedForeground
		{
			get { return (_state & State.InheritsForeground) == State.InheritsForeground; }
			set
			{
				if (value)
					_state |= State.InheritsForeground;
				else
					_state &= ~State.InheritsForeground;
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the mouse is currently hovering the UI element or any of its children.
		/// </summary>
		private bool IsMouseOver
		{
			get { return (_state & State.IsMouseOver) == State.IsMouseOver; }
			set
			{
				if (value)
					_state |= State.IsMouseOver;
				else
					_state &= ~State.IsMouseOver;
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the mouse is currently pressed down on the UI element or any of its children.
		/// </summary>
		private bool IsActive
		{
			get { return (_state & State.IsActive) == State.IsActive; }
			set
			{
				if (value)
					_state |= State.IsActive;
				else
					_state &= ~State.IsActive;
			}
		}

		/// <summary>
		///   Gets the rectangle encompassing the UI element's visual area.
		/// </summary>
		protected Rectangle VisualArea
		{
			get
			{
				var x = MathUtils.Round(VisualOffset.X);
				var y = MathUtils.Round(VisualOffset.Y);
				var width = MathUtils.Round(ActualWidth);
				var height = MathUtils.Round(ActualHeight);

				return new Rectangle(x, y, width, height);
			}
		}

		/// <summary>
		///   Checks whether the given value is a valid width or height value.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		private static void ValidateWidthHeight(float value)
		{
			// NaN is used to represent automatic width or height
			Assert.That(Single.IsNaN(value) || (value >= 0.0 && !Single.IsPositiveInfinity(value)), "Invalid value");
		}

		/// <summary>
		///   Checks whether the given value is a valid minimum width or height value.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		private static void ValidateMinWidthHeight(float value)
		{
			Assert.That(!Single.IsNaN(value) && value >= 0.0 && !Single.IsPositiveInfinity(value), "Invalid value");
		}

		/// <summary>
		///   Checks whether the given value is a valid maximum width or height value.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		private static void ValidateMaxWidthHeight(float value)
		{
			Assert.That(!Single.IsNaN(value) && value >= 0.0, "Invalid value");
		}
	}
}