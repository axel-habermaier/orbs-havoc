﻿namespace OrbsHavoc.UserInterface.Controls
{
	using Utilities;

	/// <summary>
	///   Displays arbitrary content in the UI.
	/// </summary>
	internal sealed class ContentPresenter : UIElement
	{
		private object _content;
		private Label _label;
		private UIElement _presentedElement;

		/// <summary>
		///   Gets or sets the content of the content control.
		/// </summary>
		public object Content
		{
			get => _content;
			set
			{
				if (_content == value)
					return;

				_content = value;
				SetDirtyState(measure: true, arrange: true);

				var previousElement = _presentedElement;

				if (value is UIElement element)
					_presentedElement = element;
				else if (value == null)
					_presentedElement = null;
				else if (_label == null)
					_presentedElement = _label = new Label { Text = value.ToString() };
				else
				{
					// Reuse the previous label instance
					_label.Text = value.ToString();
					previousElement = null; // No need to remove the label from the tree
				}

				previousElement?.ChangeParent(null);
				_presentedElement?.ChangeParent(this);
			}
		}

		/// <summary>
		///   Gets the number of children for this visual.
		/// </summary>
		protected override int ChildrenCount => _presentedElement == null ? 0 : 1;

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the UI element.
		/// </summary>
		protected override UIElementEnumerator GetChildren() => UIElementEnumerator.FromElement(_presentedElement);

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected override UIElement GetChild(int index)
		{
			Assert.NotNull(_presentedElement);
			Assert.ArgumentSatisfies(index == 0, nameof(index), "The UI element has only one child.");

			return _presentedElement;
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
			if (_presentedElement == null)
				return new Size();

			_presentedElement.Measure(availableSize);
			return _presentedElement.DesiredSize;
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
			if (_presentedElement == null)
				return new Size();

			_presentedElement.Arrange(new Rectangle(0, 0, finalSize));
			return _presentedElement.RenderSize;
		}
	}
}