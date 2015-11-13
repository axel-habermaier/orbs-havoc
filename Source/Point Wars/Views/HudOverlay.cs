// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Views
{
	using Assets;
	using Gameplay.SceneNodes.Entities;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Shows the HUD while a game is active and the local player is alive.
	/// </summary>
	internal class HudOverlay : View
	{
		private Label _healthLabel;
		private Grid _layoutRoot;
		private Label _weaponEnergyLabel;
		private Label _weaponTypeLabel;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = _layoutRoot = new Grid(columns: 2, rows: 10)
			{
				IsHitTestVisible = false,
				Font = AssetBundle.Moonhouse24,
				VerticalAlignment = VerticalAlignment.Bottom,
				HorizontalAlignment = HorizontalAlignment.Center
			};

			_layoutRoot.Columns[0].Width = 150;
			_layoutRoot.Columns[1].Width = 300;

			CreateRow(0, "Health", out _healthLabel);
			CreateRow(1, "Weapon", out _weaponTypeLabel);
			CreateRow(2, "Ammo", out _weaponEnergyLabel);
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			var avatar = Views.Game.GameSession.Players.LocalPlayer?.Avatar;
			if (avatar == null)
				return;

			_healthLabel.Text = StringCache.GetString(MathUtils.RoundIntegral(avatar.Health));
			_weaponTypeLabel.Text = avatar.PrimaryWeapon.ToString(); // TODO: Friendly names, no string alloc, retrieve from weapon template array
			_weaponEnergyLabel.Text = StringCache.GetString(avatar.WeaponEnergyLevels[avatar.PrimaryWeapon.GetWeaponSlot()]);
		}

		private void CreateRow(int row, string label, out Label value)
		{
			_layoutRoot.Add(new Label { Text = label, Row = row, Column = 0 });
			_layoutRoot.Add(value = new Label { Row = row, Column = 1 });
		}
	}
}