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
	using System;
	using Assets;
	using Network;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class StartGameMenuUI : Border
	{
		public StartGameMenuUI()
		{
			CapturesInput = true;
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
						Text = "Start Game",
						Font = AssetBundle.Moonhouse80,
						Margin = new Thickness(0, 0, 0, 30)
					},
					new Grid(columns: 2, rows: 4)
					{
						HorizontalAlignment = HorizontalAlignment.Center,
						Children =
						{
							new Label
							{
								Width = 120,
								Row = 0,
								Column = 0,
								VerticalAlignment = VerticalAlignment.Center,
								Text = "Server Name:"
							},
							(Name = new TextBox
							{
								Row = 0,
								Column = 1,
								Margin = new Thickness(5, 0, 0, 5),
								Width = 200,
								MaxLength = NetworkProtocol.ServerNameLength,
							}),
							(InvalidName = new Label
							{
								Row = 1,
								Column = 1,
								Text = $"Expected a non-empty string with a maximum length of {NetworkProtocol.ServerNameLength} characters.",
								Margin = new Thickness(5, 0, 0, 5),
								Foreground = Colors.Red,
								VerticalAlignment = VerticalAlignment.Center,
								Visibility = Visibility.Collapsed,
								Width = 200,
								TextWrapping = TextWrapping.Wrap
							}),
							new Label
							{
								Row = 2,
								Column = 0,
								Width = 120,
								VerticalAlignment = VerticalAlignment.Center,
								Text = "Server Port:"
							},
							(Port = new TextBox
							{
								Row = 2,
								Column = 1,
								Margin = new Thickness(5, 0, 0, 5),
								Width = 200,
								MaxLength = NetworkProtocol.ServerNameLength,
							}),
							(InvalidPort = new Label
							{
								Width = 200,
								Row = 3,
								Column = 1,
								Text = $"Expected a value of type {TypeRegistry.GetDescription<ushort>()} (e.g., " +
									   $"{String.Join(", ", TypeRegistry.GetExamples<ushort>())})",
								Margin = new Thickness(5, 0, 0, 5),
								Foreground = Colors.Red,
								TextWrapping = TextWrapping.Wrap,
								VerticalAlignment = VerticalAlignment.Center,
								Visibility = Visibility.Collapsed
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
							(StartGame = new Button
							{
								Content = "Start Game",
								Margin = new Thickness(0, 0, 10, 0)
							}),
							(Return = new Button { Content = "Return" })
						}
					}
				}
			};
		}

		public UIElement InvalidName { get; }
		public UIElement InvalidPort { get; }
		public TextBox Name { get; }
		public TextBox Port { get; }
		public Button StartGame { get; }
		public Button Return { get; }
	}
}