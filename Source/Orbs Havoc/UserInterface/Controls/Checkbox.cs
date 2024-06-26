﻿namespace OrbsHavoc.UserInterface.Controls
{
	using System.Numerics;
	using Input;
	using Rendering;

	/// <summary>
	///     Represents a checkbox control that the user can select and clear.
	/// </summary>
	internal class CheckBox : Control
	{
		private static readonly ControlTemplate _defaultTemplate = (out UIElement templateRoot, out ContentPresenter contentPresenter) =>
		{
			contentPresenter = new ContentPresenter
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			templateRoot = new Border
			{
				Child = contentPresenter,
				BorderThickness = new Thickness(1),
				BorderColor = new Color(0xFF055674),
				NormalStyle = element => ((Border)element).Background = new Color(0x5F00588B),
				HoveredStyle = element => ((Border)element).Background = new Color(0x5F0082CE),
				ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7)
			};
		};

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		public CheckBox()
			: base(_defaultTemplate)
		{
			Width = 20;
			Height = 20;
			Foreground = new Color(0xFF055674);
		}

		/// <summary>
		///     Gets or sets a value indicating whether the checkbox is checked.
		/// </summary>
		public bool IsChecked { get; set; }

		/// <summary>
		///     Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.Button != MouseButton.Left || e.Handled)
				return;

			e.Handled = true;
			Focus();

			IsChecked = !IsChecked;
		}

		/// <summary>
		///     Draws the UI element using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		protected override void DrawCore(SpriteBatch spriteBatch)
		{
			base.DrawCore(spriteBatch);

			if (!IsChecked)
				return;

			var area = VisualArea;

			spriteBatch.RenderState.Layer += 1;
			spriteBatch.DrawLine(area.TopLeft + new Vector2(2, 2), area.BottomRight - new Vector2(2, 2), Foreground, 2);
			spriteBatch.DrawLine(area.TopRight + new Vector2(-2, 2), area.BottomLeft - new Vector2(-2, 2), Foreground, 2);
			spriteBatch.RenderState.Layer -= 1;
		}
	}
}