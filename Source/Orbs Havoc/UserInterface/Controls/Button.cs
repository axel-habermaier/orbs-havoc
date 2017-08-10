namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using Input;
	using Rendering;

	/// <summary>
	///   Represents a button control.
	/// </summary>
	internal class Button : Control
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
				ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7),
				Padding = new Thickness(7, 8, 7, 7)
			};
		};

		/// <summary>
		///   Raised when the button has been clicked.
		/// </summary>
		public Action Click;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Button()
			: base(_defaultTemplate)
		{
		}

		/// <summary>
		///   Invoked when a mouse button has been released while hovering the UI element.
		/// </summary>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (e.Button != MouseButton.Left || e.Handled)
				return;

			e.Handled = true;
			Click?.Invoke();
			Focus();
		}
	}
}