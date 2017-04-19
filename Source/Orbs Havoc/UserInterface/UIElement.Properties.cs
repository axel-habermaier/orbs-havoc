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
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Controls;
	using Input;
	using Rendering;
	using Utilities;

	public partial class UIElement
	{
		private Action<UIElement> _activeStyle;
		private float _bottom = Single.NaN;
		private int _column;
		private Dock _dock = Dock.Left;
		private Font _font;
		private Color _foreground = Colors.White;
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
		private int _row;
		private float _top = Single.NaN;
		private VerticalAlignment _verticalAlignment = VerticalAlignment.Stretch;
		private Visibility _visibility = Visibility.Visible;
		private float _width = Single.NaN;

		/// <summary>
		///   Gets or sets the UI element's normal style when it is neither hovered nor active.
		/// </summary>
		public Action<UIElement> NormalStyle
		{
			get => _normalStyle;
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
			get => _hoveredStyle;
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
			get => _activeStyle;
			set
			{
				_activeStyle = value;

				if (IsActive)
					_activeStyle?.Invoke(this);
			}
		}

		/// <summary>
		///   Gets or sets the cursor that is displayed when the mouse hovers an UI element or any of its children.
		///   If null, the displayed cursor is determined by the hovered child element or the default cursor is displayed.
		/// </summary>
		public Cursor Cursor { get; set; }

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
			get => _font ?? (_font = Parent.Font);
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
		///   Gets or sets the foreground color of the UI element. If no foreground color is set, i.e., the foreground color is
		///   transparent, it is inherited from the UI element's parent.
		/// </summary>
		public Color Foreground
		{
			get => _foreground == Colors.Transparent ? (_foreground = Parent.Foreground) : _foreground;
			set
			{
				if (_foreground == value)
					return;

				HasInheritedForeground = value == Colors.Transparent;
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
			get => _width;
			set
			{
				// NaN is used to represent automatic width or height
				Assert.That(Single.IsNaN(value) || (value >= 0.0 && !Single.IsPositiveInfinity(value)), "Invalid value");

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
			get => _height;
			set
			{
				// NaN is used to represent automatic width or height
				Assert.That(Single.IsNaN(value) || (value >= 0.0 && !Single.IsPositiveInfinity(value)), "Invalid value");

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
			get => _minWidth;
			set
			{
				Assert.That(!Single.IsNaN(value) && value >= 0.0 && !Single.IsPositiveInfinity(value), "Invalid value");

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
			get => _minHeight;
			set
			{
				Assert.That(!Single.IsNaN(value) && value >= 0.0 && !Single.IsPositiveInfinity(value), "Invalid value");

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
			get => _maxWidth;
			set
			{
				Assert.That(!Single.IsNaN(value) && value >= 0.0, "Invalid value");

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
			get => _maxHeight;
			set
			{
				Assert.That(!Single.IsNaN(value) && value >= 0.0, "Invalid value");

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
			get => _margin;
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
			get => _horizontalAlignment;
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
			get => _verticalAlignment;
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
			get => _visibility;
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
		public bool IsVisible
		{
			get => (_state & State.IsVisible) == State.IsVisible;
			protected set
			{
				if (value)
					_state |= State.IsVisible;
				else
					_state &= ~State.IsVisible;

				if (value && AutoFocus)
					Focus();
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the UI element can receive the keyboard focus.
		/// </summary>
		public bool IsFocusable
		{
			get { return (_state & State.IsFocusable) == State.IsFocusable; }
			set
			{
				if (value)
					_state |= State.IsFocusable;
				else
					_state &= ~State.IsFocusable;
			}
		}

		/// <summary>
		///   Gets a value indicating whether the UI element currently has the keyboard focus.
		/// </summary>
		public bool IsFocused
		{
			get => (_state & State.IsFocused) == State.IsFocused;
			set
			{
				if (IsFocused == value)
					return;

				if (value)
					_state |= State.IsFocused;
				else
					_state &= ~State.IsFocused;

				OnFocusChanged();
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the UI element participates in hit testing.
		/// </summary>
		public bool IsHitTestVisible
		{
			get { return (_state & State.IsHitTestVisible) == State.IsHitTestVisible; }
			set
			{
				if (value)
					_state |= State.IsHitTestVisible;
				else
					_state &= ~State.IsHitTestVisible;
			}
		}

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
		protected virtual int ChildrenCount => 0;

		/// <summary>
		///   Gets or sets a value indicating whether the UI element is attached to the visual tree's root element.
		/// </summary>
		protected internal bool IsAttachedToRoot
		{
			get => (_state & State.AttachedToRoot) == State.AttachedToRoot;
			set
			{
				if (IsAttachedToRoot == value)
					return;

				if (value)
					_state |= State.AttachedToRoot;
				else
					_state &= ~State.AttachedToRoot;

				UpdateVisibleState();

				foreach (var child in GetChildren())
					child.IsAttachedToRoot = value;

				if (value)
					OnAttached();
				else
					OnDetached();
			}
		}

		/// <summary>
		///   Gets or sets the distance between the left side of the UI element and its parent Canvas.
		/// </summary>
		public float Left
		{
			get => _left;
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
			get => _top;
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
			get => _right;
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
			get => _bottom;
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
			get => _dock;
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
		///   Gets a value indicating which column of a grid layout, identified by its zero-based index, the UI element should appear
		///   in.
		/// </summary>
		public int Column
		{
			get => _column;
			set
			{
				if (_column == value)
					return;

				_column = value;

				var panel = Parent as Panel;
				panel?.SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets a value indicating which row of a grid layout, identified by its zero-based index, the UI element should appear in.
		/// </summary>
		public int Row
		{
			get => _row;
			set
			{
				if (_row == value)
					return;

				_row = value;

				var panel = Parent as Panel;
				panel?.SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets a value indicating whether the UI element can receive the keyboard focus.
		/// </summary>
		internal bool CanBeFocused => IsAttachedToRoot && IsFocusable && IsVisible;

		/// <summary>
		///   Gets the list of input bindings associated with this UI element.
		/// </summary>
		public List<InputBinding> InputBindings => _inputBindings ?? (_inputBindings = new List<InputBinding>());

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
		///   Gets or sets a value indicating whether the UI element captures all keyboard and mouse input.
		/// </summary>
		public bool CapturesInput
		{
			get { return (_state & State.CapturesInput) == State.CapturesInput; }
			set
			{
				if (value)
					_state |= State.CapturesInput;
				else
					_state &= ~State.CapturesInput;
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether the UI element automatically receives the keyboard focus when it becomes visible.
		/// </summary>
		public bool AutoFocus
		{
			get => (_state & State.AutoFocus) == State.AutoFocus;
			set
			{
				if (value)
					IsFocusable = true;

				if (value)
					_state |= State.AutoFocus;
				else
					_state &= ~State.AutoFocus;
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
	}
}