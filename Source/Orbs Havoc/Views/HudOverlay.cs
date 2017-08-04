// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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