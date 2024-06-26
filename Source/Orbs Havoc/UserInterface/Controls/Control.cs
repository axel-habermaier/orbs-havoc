﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System.Numerics;
	using Utilities;

	/// <summary>
	///   Represents a base class for templated UI elements with a single logical child of any type as its content.
	/// </summary>
	internal abstract class Control : UIElement
	{
		private static readonly ControlTemplate _defaultTemplate =
			(out UIElement templateRoot, out ContentPresenter contentPresenter) => templateRoot = contentPresenter = new ContentPresenter();

		private object _content;
		private ContentPresenter _contentPresenter;
		private Thickness _padding;
		private ControlTemplate _template;
		private UIElement _templateRoot;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="template">The template that should be used by the control.</param>
		protected Control(ControlTemplate template = null)
		{
			Template = template ?? _defaultTemplate;
		}

		/// <summary>
		///   Gets the content presenter that shows the control's contents.
		/// </summary>
		protected ContentPresenter ContentPresenter => _contentPresenter;

		/// <summary>
		///   Gets or sets the padding inside the border.
		/// </summary>
		public Thickness Padding
		{
			get => _padding;
			set
			{
				if (_padding == value)
					return;

				_padding = value;
				SetDirtyState(measure: true, arrange: true);
			}
		}

		/// <summary>
		///   Gets or sets the template that defines the control's appearance.
		/// </summary>
		public ControlTemplate Template
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
				var uiContent = _content as UIElement;
				uiContent?.ChangeParent(null);

				if (value == null)
				{
					_templateRoot = null;
					_contentPresenter = null;
				}
				else
				{
					value(out _templateRoot, out _contentPresenter);
					if (_contentPresenter != null)
						_contentPresenter.Content = _content;
				}

				_templateRoot?.ChangeParent(this);
				OnTemplateApplied(_templateRoot, _contentPresenter);
			}
		}

		/// <summary>
		///   Gets or sets the UI element that is decorated.
		/// </summary>
		public object Content
		{
			get => _content;
			set
			{
				if (_content == value)
					return;

				_content = value;

				if (ContentPresenter != null)
					ContentPresenter.Content = _content;

				OnContentChanged();
			}
		}

		/// <summary>
		///   Gets the number of children for this visual.
		/// </summary>
		protected override int ChildrenCount => _templateRoot == null ? 0 : 1;

		/// <summary>
		///   Invoked when the control's content has been changed.
		/// </summary>
		protected virtual void OnContentChanged()
		{
		}

		/// <summary>
		///   Invoked when the control's template has been applied.
		/// </summary>
		/// <param name="templateRoot">The UI element representing the root of the applied template.</param>
		/// <param name="contentPresenter">The content presenter that presents the control's content.</param>
		protected virtual void OnTemplateApplied(UIElement templateRoot, ContentPresenter contentPresenter)
		{
		}

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all logical children of the UI element.
		/// </summary>
		protected sealed override UIElementEnumerator GetChildren() => UIElementEnumerator.FromElement(_templateRoot);

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected sealed override UIElement GetChild(int index)
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
		///   Gets the additional offset that should be applied to the visual offset of the UI element's children.
		/// </summary>
		protected override Vector2 GetAdditionalChildrenOffset()
		{
			return new Vector2(_padding.Left, _padding.Top);
		}
	}
}