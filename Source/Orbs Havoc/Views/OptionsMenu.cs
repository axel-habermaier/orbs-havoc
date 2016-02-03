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
	using System.Text;
	using Assets;
	using Network;
	using Platform.Input;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;

	/// <summary>
	///   Lets the player set some options.
	/// </summary>
	internal sealed class OptionsMenu : View
	{
		private UIElement _invalidName;
		private TextBox _name;

		/// <summary>
		///   Gets the player name entered by the user or null if the name is invalid.
		/// </summary>
		private string PlayerName => Encoding.UTF8.GetByteCount(_name.Text) > NetworkProtocol.PlayerNameLength ? null : _name.Text;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				CapturesInput = true,
				Background = new Color(0xAA000000),
				IsFocusable = true,
				Font = AssetBundle.Roboto14,
				AutoFocus = true,
				InputBindings =
				{
					new KeyBinding(ShowPreviousMenu, Key.Escape)
				},
				Child = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Children =
					{
						new Label
						{
							Text = "Options",
							Font = AssetBundle.Moonhouse80,
							Margin = new Thickness(0, 0, 0, 30),
						},
						new Grid(columns: 2, rows: 2)
						{
							HorizontalAlignment = HorizontalAlignment.Center,
							Children =
							{
								new Label
								{
									Text = "Player Name:",
									Margin = new Thickness(0, 4, 15, 0),
									Row = 0,
									Column = 0
								},
								(_name = new TextBox
								{
									MaxLength = NetworkProtocol.PlayerNameLength,
									Text = Cvars.PlayerName,
									Row = 0,
									Column = 1,
									Width = 200,
									TextChanged = OnNameChanged
								}),
								(_invalidName = new Label
								{
									Row = 1,
									Column = 1,
									Foreground = Colors.Red,
									Margin = new Thickness(0, 10, 0, 10),
									TextWrapping = TextWrapping.Wrap,
									Width = 200,
									Visibility = Visibility.Collapsed,
									Text = $"Expected a non-empty string with a maximum length of {NetworkProtocol.PlayerNameLength} characters."
								})
							}
						},
						new StackPanel
						{
							Orientation = Orientation.Horizontal,
							Margin = new Thickness(0, 20, 0, 0),
							HorizontalAlignment = HorizontalAlignment.Center,
							Children =
							{
								new Button
								{
									Content = "Save",
									Margin = new Thickness(0, 0, 10, 0),
									Click = Save
								},
								new Button
								{
									Content = "Return",
									Click = ShowPreviousMenu
								}
							}
						}
					}
				}
			};
		}

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected override void Activate()
		{
			_name.Text = Cvars.PlayerName;
		}

		/// <summary>
		///   Invoked when the user entered another name.
		/// </summary>
		private void OnNameChanged(string name)
		{
			_invalidName.Visibility = PlayerName == null ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Saves the changes and returns.
		/// </summary>
		private void Save()
		{
			if (PlayerName == null)
				return;

			Cvars.PlayerName = PlayerName;
			ShowPreviousMenu();
		}

		/// <summary>
		///   Shows the previously active menu.
		/// </summary>
		private void ShowPreviousMenu()
		{
			Hide();

			if (Views.Game.IsShown)
				Views.InGameMenu.Show();
			else
				Views.MainMenu.Show();
		}
	}
}