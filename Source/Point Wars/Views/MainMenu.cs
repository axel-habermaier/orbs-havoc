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
	using System;
	using Assets;
	using UserInterface;
	using UserInterface.Controls;

	/// <summary>
	///   Represents the application's main menu when no game session is active.
	/// </summary>
	internal sealed class MainMenu : View
	{
		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			IsActive = true;

			RootElement = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Children =
				{
					new Label
					{
						Text = Application.Name,
						Font = AssetBundle.Moonhouse80,
						Margin = new Thickness(0, 0, 0, 30),
					},
					CreateButton("Start Game", () => { }),
					CreateButton("Join Game", () =>
					{
						Views.JoinGameMenu.IsActive = true;
						IsActive = false;
					}),
					CreateButton("Options", () => { }),
					CreateButton("Exit", Views.Exit)
				}
			};
		}

		/// <summary>
		///   Creates a menu button.
		/// </summary>
		private static UIElement CreateButton(string label, Action onClick)
		{
			var button = new Button
			{
				Font = AssetBundle.Moonhouse24,
				Width = 200,
				Content = label,
				Margin = new Thickness(4),
			};

			button.Click += onClick;
			return button;
		}
	}
}