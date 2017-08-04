namespace OrbsHavoc.Views.UI
{
	using Assets;
	using UserInterface;
	using UserInterface.Controls;

	internal class HudOverlayUI : StackPanel
	{
		public HudOverlayUI()
		{
			IsHitTestVisible = false;
			Orientation = Orientation.Horizontal;
			Font = AssetBundle.HudFont;
			VerticalAlignment = VerticalAlignment.Bottom;
			HorizontalAlignment = HorizontalAlignment.Left;
			MinHeight = 70;
			Margin = new Thickness(30, 0, 0, 20);

			Children.AddRange(
				HealthIcon, HealthLabel,
				WeaponIcon, AmmoLabel,
				PowerUpIcon, PowerUpLabel
			);

			foreach (var child in Children)
				child.VerticalAlignment = VerticalAlignment.Center;
		}

		public Image HealthIcon { get; } = new Image { Texture = AssetBundle.HudHealthIcon, Margin = new Thickness(0, 0, 20, 0) };
		public Image PowerUpIcon { get; } = new Image { Texture = AssetBundle.RoundParticle, Margin = new Thickness(0, 0, 20, 0) };
		public Image WeaponIcon { get; } = new Image { Texture = AssetBundle.RoundParticle, Margin = new Thickness(0, 0, 20, 0) };

		public Label AmmoLabel { get; } = new Label { MinWidth = 200 };
		public Label HealthLabel { get; } = new Label { MinWidth = 200 };
		public Label PowerUpLabel { get; } = new Label { MinWidth = 200 };
	}
}