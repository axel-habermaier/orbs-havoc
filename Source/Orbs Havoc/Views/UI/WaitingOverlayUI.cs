namespace OrbsHavoc.Views.UI
{
	using System;
	using Assets;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class WaitingOverlayUI : Border
	{
		private readonly Label _label = new Label();
		private int _timeout;

		public WaitingOverlayUI()
		{
			IsHitTestVisible = false;
			Background = new Color(0xAA000000);
			Font = AssetBundle.Roboto14;
			Child = new Border
			{
				Margin = new Thickness(0, 0, 0, 400),
				Background = new Color(0x5F00588B),
				Padding = new Thickness(10),
				BorderColor = new Color(0xFF055674),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Child = _label
			};
		}

		public void Update(double secondsLeft)
		{
			var timeout = (int)Math.Round(secondsLeft);
			if (_timeout == timeout)
				return;

			_timeout = timeout;
			_label.Text = $"Waiting for server ({timeout} seconds left)...";
		}
	}
}