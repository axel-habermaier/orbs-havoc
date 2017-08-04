namespace OrbsHavoc.Views.UI
{
	using System;
	using System.Net;
	using Assets;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class LoadingOverlayUI : Border
	{
		private readonly Label _infoLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };

		public LoadingOverlayUI()
		{
			CapturesInput = true;
			IsFocusable = true;
			Font = AssetBundle.Roboto14;
			AutoFocus = true;

			Child = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Children =
				{
					new Label
					{
						Text = "Loading",
						Font = AssetBundle.Moonhouse80,
						Margin = new Thickness(0, 0, 0, 30),
					},
					_infoLabel
				}
			};
		}

		public void Update(IPEndPoint serverEndPoint, double remainingSeconds)
		{
			_infoLabel.Text = $"Connecting to {serverEndPoint} ({Math.Round(remainingSeconds)} seconds)...";
		}
	}
}