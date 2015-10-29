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

namespace PointWars.UserInterface.Controls
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents a base class for templated UI elements with a single logical child of any type as its content.
	/// </summary>
	public class Control : UIElement
	{
		private static readonly Func<Control, UIElement> DefaultTemplate = control =>
		{
			var presenter = new ContentPresenter();
			control.TemplateBinding = () => presenter.Content = control.Content;
			return presenter;
		};

		private Thickness _padding;
		private Func<Control, UIElement> _template;
		private Action _templateBinding;
		private UIElement _templateRoot;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="template">The template that should be used by the control.</param>
		public Control(Func<Control, UIElement> template = null)
		{
			Template = template ?? DefaultTemplate;
		}

		/// <summary>
		///   Gets or sets the template binding that copies property values from the control to its template root.
		/// </summary>
		public Action TemplateBinding
		{
			get { return _templateBinding; }
			set
			{
				if (_templateBinding == value)
					return;

				_templateBinding = value;

				if (!IsAttachedToRoot)
					return;

				if (_templateBinding != null)
					RootElement.TemplateBindings.Add(_templateBinding);
				else
					RootElement.TemplateBindings.Remove(_templateBinding);
			}
		}

		/// <summary>
		///   Gets or sets the template that defines the control's appearance.
		/// </summary>
		public Func<Control, UIElement> Template
		{
			get
			{
				Assert.NotNull(_template, "No template has been set for the control.");
				return _template;
			}
			set
			{
				Assert.ArgumentNotNull(value, nameof(value));

				if (_template == value)
					return;

				_template = value;
				SetDirtyState(measure: true, arrange: true);

				_templateRoot?.ChangeParent(null);

				if (value == null)
					_templateRoot = null;
				else
					_templateRoot = value(this);

				_templateRoot?.ChangeParent(this);
			}
		}

		/// <summary>
		///   Gets or sets the UI element that is decorated.
		/// </summary>
		public object Content { get; set; }

		/// <summary>
		///   Gets or sets the padding inside the border.
		/// </summary>
		public Thickness Padding
		{
			get { return _padding; }
			set
			{
				if (_padding == value)
					return;

				_padding = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets the number of children for this visual.
		/// </summary>
		protected internal override int ChildrenCount => _templateRoot == null ? 0 : 1;

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all logical children of the UI element.
		/// </summary>
		protected override sealed Enumerator<UIElement> GetChildren() => Enumerator<UIElement>.FromItemOrEmpty(_templateRoot);

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected internal override sealed UIElement GetChild(int index)
		{
			Assert.NotNull(_templateRoot);
			Assert.ArgumentSatisfies(index == 0, nameof(index), "The UI element has only one child.");

			return _templateRoot;
		}

		/// <summary>
		///   Computes and returns the desired size of the element given the available space allocated by the parent UI element.
		/// </summary>
		/// <param name="availableSize">
		///   The available space that the parent UI element can allocate to this UI element. Can be infinity if the parent wants
		///   to size itself to its contents. The computed desired size is allowed to exceed the available space; the parent UI
		///   element might be able to use scrolling in this case.
		/// </param>
		protected override Size MeasureCore(Size availableSize)
		{
			if (_templateRoot == null)
				return new Size();

			availableSize = new Size(availableSize.Width - Padding.Width, availableSize.Height - Padding.Height);
			_templateRoot.Measure(availableSize);

			return new Size(_templateRoot.DesiredSize.Width + Padding.Width, _templateRoot.DesiredSize.Height + Padding.Height);
		}

		/// <summary>
		///   Determines the size of the UI element and positions all of its children. Returns the actual size used by the UI
		///   element. If this value is smaller than the given size, the UI element's alignment properties position it
		///   appropriately.
		/// </summary>
		/// <param name="finalSize">
		///   The final area allocated by the UI element's parent that the UI element should use to arrange
		///   itself and its children.
		/// </param>
		protected override Size ArrangeCore(Size finalSize)
		{
			if (_templateRoot == null)
				return new Size();

			finalSize = new Size(finalSize.Width - Padding.Width, finalSize.Height - Padding.Height);
			_templateRoot.Arrange(new Rectangle(0, 0, finalSize));

			return new Size(_templateRoot.RenderSize.Width + Padding.Width, _templateRoot.RenderSize.Height + Padding.Height);
		}

		/// <summary>
		///   Invoked when the UI element is now (transitively) attached to the root of a tree.
		/// </summary>
		protected override void OnAttached()
		{
			if (_templateBinding != null)
				RootElement.TemplateBindings.Add(_templateBinding);
		}

		/// <summary>
		///   Invoked when the UI element is no longer (transitively) attached to the root of a tree.
		/// </summary>
		protected override void OnDetached()
		{
			if (_templateBinding != null)
				RootElement.TemplateBindings.Remove(_templateBinding);
		}
	}
}