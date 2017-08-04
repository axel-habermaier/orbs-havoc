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
	using Network;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class OptionsMenuUI : Border
	{
		public OptionsMenuUI()
		{
			CapturesInput = true;
			Background = new Color(0xAA000000);
			IsFocusable = true;
			Font = AssetBundle.Roboto14;
			AutoFocus = true;

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
					new Grid(columns: 2, rows: 5)
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
							(Name = new TextBox
							{
								MaxLength = NetworkProtocol.PlayerNameLength,
								Text = Cvars.PlayerName,
								Row = 0,
								Column = 1,
								Width = 200,
							}),
							(InvalidName = new Label
							{
								Row = 1,
								Column = 1,
								Foreground = Colors.Red,
								Margin = new Thickness(0, 10, 0, 10),
								TextWrapping = TextWrapping.Wrap,
								Width = 200,
								Visibility = Visibility.Collapsed,
								Text = $"Expected a non-empty string with a maximum length of {NetworkProtocol.PlayerNameLength} characters."
							}),
							new Label
							{
								Text = "VSync:",
								Margin = new Thickness(0, 4, 15, 0),
								Row = 2,
								Column = 0
							},
							(Vsync = new CheckBox
							{
								Row = 2,
								Column = 1,
								HorizontalAlignment = HorizontalAlignment.Left,
								Margin = new Thickness(0, 1, 0, 1)
							}),
							new Label
							{
								Text = "Debug Overlay:",
								Margin = new Thickness(0, 4, 15, 0),
								Row = 3,
								Column = 0
							},
							(DebugOverlay = new CheckBox
							{
								Row = 3,
								Column = 1,
								HorizontalAlignment = HorizontalAlignment.Left,
								Margin = new Thickness(0, 1, 0, 1)
							}),
							new Label
							{
								Text = "Bloom:",
								Margin = new Thickness(0, 4, 15, 0),
								Row = 4,
								Column = 0
							},
							(Bloom = new CheckBox
							{
								Row = 4,
								Column = 1,
								HorizontalAlignment = HorizontalAlignment.Left,
								Margin = new Thickness(0, 1, 0, 1)
							}),
						}
					},
					new StackPanel
					{
						Orientation = Orientation.Horizontal,
						Margin = new Thickness(0, 20, 0, 0),
						HorizontalAlignment = HorizontalAlignment.Center,
						Children =
						{
							(Save = new Button
							{
								Content = "Save",
								Margin = new Thickness(0, 0, 10, 0),
							}),
							(Return = new Button
							{
								Content = "Return",
							})
						}
					}
				}
			};
		}

		public CheckBox Bloom { get; }
		public CheckBox DebugOverlay { get; }
		public UIElement InvalidName { get; }
		public TextBox Name { get; }
		public CheckBox Vsync { get; }
		public Button Save { get; }
		public Button Return { get; }
	}
}