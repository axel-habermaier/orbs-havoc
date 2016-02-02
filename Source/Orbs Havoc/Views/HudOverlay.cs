// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using Assets;
	using Gameplay.Client;
	using Gameplay.SceneNodes.Entities;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Shows the HUD while a game is active and the local player is alive.
	/// </summary>
	internal class HudOverlay : View
	{
		private const int HealthBlinkingFrequency = 2;
		private readonly Label _ammoLabel = new Label { MinWidth = 200 };
		private readonly Image _healthIcon = new Image { Texture = AssetBundle.HudHealthIcon, Margin = new Thickness(0, 0, 20, 0) };
		private readonly Label _healthLabel = new Label { MinWidth = 200 };
		private readonly Image _powerUpIcon = new Image { Texture = AssetBundle.RoundParticle, Margin = new Thickness(0, 0, 20, 0) };
		private readonly Label _powerUpLabel = new Label { MinWidth = 200 };
		private readonly Image _weaponIcon = new Image { Texture = AssetBundle.RoundParticle, Margin = new Thickness(0, 0, 20, 0) };
		private StackPanel _layoutRoot;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = _layoutRoot = new StackPanel
			{
				IsHitTestVisible = false,
				Orientation = Orientation.Horizontal,
				Font = AssetBundle.HudFont,
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Left,
				MinHeight = 70,
				Margin = new Thickness(30, 0, 0, 20)
			};

			_layoutRoot.Add(_healthIcon);
			_layoutRoot.Add(_healthLabel);

			_layoutRoot.Add(_weaponIcon);
			_layoutRoot.Add(_ammoLabel);

			_layoutRoot.Add(_powerUpIcon);
			_layoutRoot.Add(_powerUpLabel);

			foreach (var child in _layoutRoot.Children)
				child.VerticalAlignment = VerticalAlignment.Center;
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			var orb = Views.Game.GameSession.Players.LocalPlayer?.Orb;
			if (orb == null)
				return;

			_healthLabel.Text = StringCache.GetString(MathUtils.RoundIntegral(orb.Health));
			_ammoLabel.Text = StringCache.GetString(orb.WeaponEnergyLevels[orb.PrimaryWeapon.GetWeaponSlot()]);
			_powerUpLabel.Text = StringCache.GetString(MathUtils.RoundIntegral(orb.RemainingPowerUpTime));

			_weaponIcon.Texture = orb.PrimaryWeapon.GetTexture();
			_weaponIcon.Foreground = orb.PrimaryWeapon.GetColor();

			var healthColor = new Color(0, 255, 0, 255);
			if (orb.HasCriticalHealth)
				healthColor = Colors.Red;

			_healthIcon.Foreground = healthColor;
			_healthLabel.Foreground = healthColor;

			_healthIcon.Visibility = orb.HasCriticalHealth && MathUtils.RoundIntegral((float)Clock.GetTime() * HealthBlinkingFrequency) % 2 != 0
				? Visibility.Hidden
				: Visibility.Visible;

			var powerUpVisibility = orb.PowerUp == EntityType.None ? Visibility.Hidden : Visibility.Visible;
			_powerUpLabel.Visibility = powerUpVisibility;
			_powerUpIcon.Visibility = powerUpVisibility;

			var ammoVisibility = orb.PrimaryWeapon == EntityType.MiniGun ? Visibility.Hidden : Visibility.Visible;
			_weaponIcon.Visibility = ammoVisibility;
			_ammoLabel.Visibility = ammoVisibility;

			if (orb.PowerUp != EntityType.None)
			{
				_powerUpIcon.Texture = orb.PowerUp.GetTexture();
				_powerUpIcon.Foreground = orb.PowerUp.GetColor();
			}
		}
	}
}