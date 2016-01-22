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

namespace OrbsHavoc.UserInterface
{
	using Utilities;

	/// <summary>
	///   Represents a collection of UI elements that belongs to an UI element. When an UI element is added to or removed from the
	///   collection, its parent is updated accordingly.
	/// </summary>
	public class UIElementCollection : CustomCollection<UIElement>
	{
		/// <summary>
		///   The parent of the UI elements contained in the collection.
		/// </summary>
		private readonly UIElement _parent;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="parent"> The visual parent of the UI elements contained in the collection.</param>
		public UIElementCollection(UIElement parent)
		{
			Assert.ArgumentNotNull(parent, nameof(parent));
			_parent = parent;
		}

		/// <summary>
		///   Removes all elements from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			foreach (var element in this)
				element.ChangeParent(null);

			_parent.OnChildrenChanged();
			base.ClearItems();
		}

		/// <summary>
		///   Inserts an element into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the item that should be inserted.</param>
		/// <param name="item">The item that should be inserted.</param>
		protected override void InsertItem(int index, UIElement item)
		{
			Assert.ArgumentNotNull(item, nameof(item));

			base.InsertItem(index, item);

			item.ChangeParent(_parent);
			_parent.OnChildrenChanged();
		}

		/// <summary>
		///   Removes the element at the specified index of the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the element that should be removed.</param>
		protected override void RemoveItem(int index)
		{
			this[index].ChangeParent(null);
			base.RemoveItem(index);

			_parent.OnChildrenChanged();
		}

		/// <summary>
		///   Replaces the element at the specified index.
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
		}
	}
}