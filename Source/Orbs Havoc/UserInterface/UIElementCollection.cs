namespace OrbsHavoc.UserInterface
{
	using System.Collections.ObjectModel;
	using Utilities;

	/// <summary>
	///     Represents a collection of UI elements that belongs to an UI element. When an UI element is added to or removed from the
	///     collection, its parent is updated accordingly.
	/// </summary>
	internal class UIElementCollection : Collection<UIElement>
	{
		/// <summary>
		///     The parent of the UI elements contained in the collection.
		/// </summary>
		private readonly UIElement _parent;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="parent"> The visual parent of the UI elements contained in the collection.</param>
		public UIElementCollection(UIElement parent)
		{
			Assert.ArgumentNotNull(parent, nameof(parent));
			_parent = parent;
		}

		/// <summary>
		///     Gets the version of the collection. Each modification of the collection increments the version number by one.
		/// </summary>
		internal int Version { get; private set; }

		/// <summary>
		///     Removes all elements from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var element in this)
				element.ChangeParent(null);

			_parent.OnChildrenChanged();
			base.ClearItems();

			++Version;
		}

		public void AddRange(params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			foreach (var element in elements)
				Add(element);
		}

		/// <summary>
		///     Inserts an element into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the item that should be inserted.</param>
		/// <param name="item">The item that should be inserted.</param>
		protected override void InsertItem(int index, UIElement item)
		{
			Assert.ArgumentNotNull(item, nameof(item));

			base.InsertItem(index, item);

			item.ChangeParent(_parent);
			_parent.OnChildrenChanged();

			++Version;
		}

		/// <summary>
		///     Removes the element at the specified index of the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the element that should be removed.</param>
		protected override void RemoveItem(int index)
		{
			this[index].ChangeParent(null);
			base.RemoveItem(index);

			_parent.OnChildrenChanged();

			++Version;
		}

		/// <summary>
		///     Replaces the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element that should be replaced.</param>
		/// <param name="item">The new value for the element at the specified index.</param>
		protected override void SetItem(int index, UIElement item)
		{
			Assert.ArgumentNotNull(item, nameof(item));

			base.SetItem(index, item);

			this[index].ChangeParent(null);
			item.ChangeParent(_parent);

			_parent.OnChildrenChanged();

			++Version;
		}

		/// <summary>
		///     Gets an enumerator for the collection.
		/// </summary>
		/// <Remarks>This method returns a custom enumerator in order to avoid heap allocations.</Remarks>
		public new UIElementEnumerator GetEnumerator()
		{
			return Count == 0 ? UIElementEnumerator.Empty : UIElementEnumerator.FromElements(this);
		}
	}
}