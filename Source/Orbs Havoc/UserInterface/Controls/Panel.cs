namespace OrbsHavoc.UserInterface.Controls
{
	using Rendering;
	using Utilities;

	/// <summary>
	///   A base class for all panel elements that position and arrange child UI elements.
	/// </summary>
	internal abstract class Panel : UIElement
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		protected Panel()
		{
			Children = new UIElementCollection(this);
		}

		/// <summary>
		///   Gets or sets the collection of layouted children.
		/// </summary>
		public UIElementCollection Children { get; }

		/// <summary>
		///   Gets the number of children for this visual.
		/// </summary>
		protected sealed override int ChildrenCount => Children?.Count ?? 0;

		/// <summary>
		///   Gets or sets the scroll viewer the panel is contained in.
		/// </summary>
		public ScrollViewer ScollViewer { get; set; }

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the panel.
		/// </summary>
		protected sealed override UIElementEnumerator GetChildren() => Children?.GetEnumerator() ?? UIElementEnumerator.Empty;

		/// <summary>
		///   Adds the given UI element to the panel.
		/// </summary>
		/// <param name="element">The UI element that should be added.</param>
		public void Add(UIElement element)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			Children.Add(element);
		}

		/// <summary>
		///   Removes the given UI element from the panel.
		/// </summary>
		/// <param name="element">The UI element that should be removed.</param>
		public bool Remove(UIElement element)
		{
			Assert.ArgumentNotNull(element, nameof(element));
			return Children.Remove(element);
		}

		/// <summary>
		///   Removes all child UI elements from the panel.
		/// </summary>
		public void Clear()
		{
			Children.Clear();
		}

		/// <summary>
		///   Gets the child at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the child that should be returned.</param>
		protected sealed override UIElement GetChild(int index)
		{
			Assert.NotNull(Children);
			Assert.ArgumentInRange(index, Children, nameof(index));

			return Children[index];
		}

		/// <summary>
		///   Draws the child UI elements of the current UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element's children.</param>
		protected override void DrawChildren(SpriteBatch spriteBatch)
		{
			if (ScollViewer != null)
			{
				// We draw the children on a higher layer to avoid draw ordering issues.
				spriteBatch.RenderState.Layer += 1;

				// Only draw the children that are actually visible
				var area = ScollViewer.ScrollArea;
				foreach (var child in Children)
				{
					var topIsInside = child.VisualOffset.Y <= area.Bottom;
					var bottomIsInside = child.VisualOffset.Y + child.ActualHeight >= area.Top;
					var leftIsInside = child.VisualOffset.X <= area.Right;
					var rightIsInside = child.VisualOffset.X + child.ActualWidth >= area.Left;

					if (topIsInside && bottomIsInside && leftIsInside && rightIsInside)
						child.Draw(spriteBatch);
				}

				spriteBatch.RenderState.Layer -= 1;
			}
			else
				base.DrawChildren(spriteBatch);
		}
	}
}