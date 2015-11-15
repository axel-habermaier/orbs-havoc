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

namespace OrbsHavoc.Views
{
	using Assets;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Represents waiting-for-server overlay during a game session.
	/// </summary>
	internal sealed class RespawnOverlay : View
	{
		private readonly Label _label = new Label { HorizontalAlignment = HorizontalAlignment.Center };

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				IsHitTestVisible = false,
				Background = new Color(0x44000000),
				Font = AssetBundle.Moonhouse24,
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
				}
			};
		}

		/// <summary>
		///   Updates the continuation label when the input key has changed.
		/// </summary>
		public override void Update()
		{
			var remainingTime = StringCache.GetString(MathUtils.RoundIntegral(Views.Game.GameSession.Players.LocalPlayer.RemainingRespawnDelay));
			_label.Text = $"Respawning in {remainingTime}...";
		}
	}
}