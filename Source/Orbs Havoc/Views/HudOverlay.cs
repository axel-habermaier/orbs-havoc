namespace OrbsHavoc.Views
{
	using Gameplay.Client;
	using Gameplay.SceneNodes.Entities;
	using Rendering;
	using UI;
	using UserInterface;
	using Utilities;

	internal class HudOverlay : View<HudOverlayUI>
	{
		private const int HealthBlinkingFrequency = 2;

		public override void Update()
		{
			var orb = Views.Game.GameSession.Players.LocalPlayer?.Orb;
			if (orb == null)
				return;

			UpdateHealth(orb);
			UpdateWeapon(orb);
			UpdatePowerUp(orb);
		}

		private void UpdateHealth(Orb orb)
		{
			UI.HealthLabel.Text = StringCache.GetString(MathUtils.RoundIntegral(orb.Health));

			var healthColor = new Color(0, 255, 0, 255);
			if (orb.HasCriticalHealth)
				healthColor = Colors.Red;

			UI.HealthIcon.Foreground = healthColor;
			UI.HealthLabel.Foreground = healthColor;

			UI.HealthIcon.Visibility = orb.HasCriticalHealth && MathUtils.RoundIntegral((float)Clock.GetTime() * HealthBlinkingFrequency) % 2 != 0
				? Visibility.Hidden
				: Visibility.Visible;
		}

		private void UpdateWeapon(Orb orb)
		{
			UI.AmmoLabel.Text = StringCache.GetString(orb.WeaponEnergyLevels[orb.PrimaryWeapon.GetWeaponSlot()]);

			UI.WeaponIcon.Texture = orb.PrimaryWeapon.GetTexture();
			UI.WeaponIcon.Foreground = orb.PrimaryWeapon.GetColor();

			var ammoVisibility = orb.PrimaryWeapon == EntityType.MiniGun ? Visibility.Hidden : Visibility.Visible;
			UI.WeaponIcon.Visibility = ammoVisibility;
			UI.AmmoLabel.Visibility = ammoVisibility;
		}

		private void UpdatePowerUp(Orb orb)
		{
			var powerUpVisibility = orb.PowerUp == EntityType.None ? Visibility.Hidden : Visibility.Visible;
			UI.PowerUpLabel.Visibility = powerUpVisibility;
			UI.PowerUpIcon.Visibility = powerUpVisibility;

			if (orb.PowerUp == EntityType.None)
				return;

			UI.PowerUpLabel.Text = StringCache.GetString(MathUtils.RoundIntegral(orb.RemainingPowerUpTime));
			UI.PowerUpIcon.Texture = orb.PowerUp.GetTexture();
			UI.PowerUpIcon.Foreground = orb.PowerUp.GetColor();
		}
	}
}