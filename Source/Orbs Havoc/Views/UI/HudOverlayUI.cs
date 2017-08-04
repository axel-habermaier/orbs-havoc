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