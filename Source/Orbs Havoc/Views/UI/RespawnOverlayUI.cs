namespace OrbsHavoc.Views.UI
{
	using Assets;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	internal sealed class RespawnOverlayUI : Border
	{
		private readonly Label _label = new Label { HorizontalAlignment = HorizontalAlignment.Center };

		public RespawnOverlayUI()
		{
			IsHitTestVisible = false;
			Background = new Color(0x44000000);
			Font = AssetBundle.Moonhouse24;

			Child = new StackPanel
			{
				Margin = new Thickness(0, 0, 0, 400),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Children =
				{
					new Label { Text = "You got killed!", Font = AssetBundle.Moonhouse80 },
					_label
				}
			};
		}

		public void Update(float remainingDelayInSeconds)
		{
			var remainingTime = StringCache.GetString(MathUtils.RoundIntegral(remainingDelayInSeconds));
			_label.Text = $"Respawning in {remainingTime}...";
		}
	}
}