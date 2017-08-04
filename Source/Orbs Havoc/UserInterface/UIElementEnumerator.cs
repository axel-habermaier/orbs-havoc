﻿namespace OrbsHavoc.UserInterface
{
	using Utilities;

	/// <summary>
	///   Enumerates a custom collection.
	/// </summary>
	public struct UIElementEnumerator
	{
		/// <summary>
		///   Represents an enumerator that does not enumerate any items.
		/// </summary>
		public static readonly UIElementEnumerator Empty = new UIElementEnumerator();

		/// <summary>
		///   The collection that is enumerated.
		/// </summary>
		private UIElementCollection _collection;

		/// <summary>
		///   The index of the current enumerated item.
		/// </summary>
		private int _current;

		/// <summary>
		///   The single item that is enumerated.
		/// </summary>
		private UIElement _item;

		/// <summary>
		///   Indicates whether only a single item should be enumerated.
		/// </summary>
		private bool _singleItem;

		/// <summary>
		///   The version of the collection when the enumerator was created.
		/// </summary>
		private int _version;

		/// <summary>
		///   Gets the item at the current position of the enumerator.
		/// </summary>
		public UIElement Current { get; private set; }

		/// <summary>
		///   Creates an enumerator for a single item. If the item is null, an empty enumerator is returned.
		/// </summary>
		/// <param name="item">The item that should be enumerated.</param>
		public static UIElementEnumerator FromElement(UIElement item)
		{
			return item == null ? Empty : new UIElementEnumerator { _item = item, _singleItem = true };
		}

		/// <summary>
		///   Creates an enumerator for a collection with multiple items.
		/// </summary>
		/// <param name="collection">The collection that should be enumerated.</param>
		public static UIElementEnumerator FromElements(UIElementCollection collection)
		{
			Assert.ArgumentNotNull(collection, nameof(collection));
			return new UIElementEnumerator { _collection = collection, _version = collection.Version };
		}

		/// <summary>
		///   Advances the enumerator to the next item.
		/// </summary>
		public bool MoveNext()
		{
			// If we neither have a single item nor a collection, we're done
			if (!_singleItem && _collection == null)
				return false;

			// If we have a single item, enumerate it and make sure the next call to MoveNext returns false
			if (_singleItem)
			{
				Current = _item;
				_singleItem = false;

				return true;
			}

			Assert.That(_collection.Version == _version, "The collection has been modified while it is enumerated.");

			// If we've reached the end of the collection, we're done
			if (_current == _collection.Count)
			{
				_collection = null;
				return false;
			}

			// Otherwise, enumerate the next element
			Current = _collection[_current++];
			return true;
		}

		/// <summary>
		///   Gets the enumerator that can be used with C#'s foreach loops.
		/// </summary>
		/// <remarks>
		///   This method just returns the enumerator object. It is only required to enable foreach support.
		/// </remarks>
		public UIElementEnumerator GetEnumerator()
		{
			return this;
		}
	}
}