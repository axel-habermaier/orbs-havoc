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
	using Platform.Input;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;

	/// <summary>
	///   Represents the application's main menu when no game session is active.
	/// </summary>
	internal sealed class MainMenuView : View
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public MainMenuView()
			: base(InputLayer.Menu)
		{
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			IsActive = true;

			var stackPanel = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center
			};

			stackPanel.Add(new Label(Application.Name)
			{
				Font = Assets.Moonhouse80,
				Margin = new Thickness(0, 0, 0, 30),
			});

			stackPanel.Add(CreateButton("Start Game", () => { }));
			stackPanel.Add(CreateButton("Join Game", () => { }));
			stackPanel.Add(CreateButton("Options", () => { }));
			stackPanel.Add(CreateButton("Exit", OnExit));

			RootElement.Child = stackPanel;
		}

		/// <summary>
		///   Creates a menu button.
		/// </summary>
		private static UIElement CreateButton(string label, Action onClick)
		{
			var button = new Button
			{
				Font = Assets.Moonhouse24,
				Width = 200,
				Child = new Border
				{
					Child = new Label(label) { TextAlignment = TextAlignment.Center },
					BorderThickness = new Thickness(1),
					BorderColor = new Color(0xFF055674),
					NormalStyle = element => ((Border)element).Background = new Color(0x5F00588B),
					HoveredStyle = element => ((Border)element).Background = new Color(0x5F0082CE),
					ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7),
					Padding = new Thickness(7, 6, 7, 7)
				},
				Margin = new Thickness(4),
			};

			button.Click += onClick;
			return button;
		}

		/// <summary>
		///   Handles clicks on the Exit button.
		/// </summary>
		private void OnExit()
		{
			Views.MessageBoxes.Show(MessageBox.ShowConfimation("Are you sure?", "Do you really want to quit the application?", Commands.Exit));
		}
	}
}