﻿// The MIT License (MIT)
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
	using Platform.Input;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	public sealed class MessageBoxUI : Border
	{
		public Action Button1Clicked;
		public Action Button2Clicked;

		public MessageBoxUI(string title, string message, string button1Label, string button2Label = null)
		{
			Assert.ArgumentNotNullOrWhitespace(title, nameof(title));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			Assert.ArgumentNotNullOrWhitespace(button1Label, nameof(button1Label));
			Assert.ArgumentSatisfies(button2Label == null || !String.IsNullOrWhiteSpace(button2Label), nameof(button2Label), "Invalid label.");

			IsFocusable = true;
			AutoFocus = true;
			CapturesInput = true;
			Background = new Color(0xAA000000);

			InputBindings.Add(new KeyBinding(OnButton1Clicked, Key.Enter));
			InputBindings.Add(new KeyBinding(OnButton1Clicked, Key.Space));
			InputBindings.Add(new KeyBinding(OnButton1Clicked, Key.NumpadEnter));
			InputBindings.Add(new KeyBinding(OnButton2Clicked, Key.Escape));

			var button1 = CreateButton(button1Label, OnButton1Clicked);
			button1.Padding = new Thickness(0, 0, 3, 0);

			var button2 = CreateButton(button2Label ?? String.Empty, OnButton2Clicked);
			if (button2Label == null)
				button2.Visibility = Visibility.Collapsed;
			
			Child = new Border
			{
				Font = AssetBundle.Roboto14,
				MaxWidth = 600,
				MinWidth = 200,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				BorderColor = new Color(0xFF055674),
				Background = new Color(0xFF002033),
				Child = new StackPanel
				{
					Children =
					{
						new Border
						{
							Background = new Color(0x33A1DDFF),
							Child = new Label { Text = title, Margin = new Thickness(15, 7, 15, 7) }
						},
						new Label
						{
							Text = message,
							TextAlignment = TextAlignment.Left,
							HorizontalAlignment = HorizontalAlignment.Stretch,
							Margin = new Thickness(15),
							TextWrapping = TextWrapping.Wrap
						},
						new StackPanel
						{
							Margin = new Thickness(0, 0, 0, 5),
							HorizontalAlignment = HorizontalAlignment.Center,
							Orientation = Orientation.Horizontal,
							Children = { button1, button2 }
						}
					}
				}
			};
		}

		private void OnButton1Clicked()
		{
			Button1Clicked?.Invoke();
			Close();
		}

		private void OnButton2Clicked()
		{
			Button2Clicked?.Invoke();
			Close();
		}

		private void Close()
		{
			var panel = Parent as Panel;
			panel?.Remove(this);
		}

		private static Button CreateButton(string label, Action clickHandler)
		{
			var button = new Button
			{
				Width = 70,
				Margin = new Thickness(0, 0, 0, 3),
				HorizontalAlignment = HorizontalAlignment.Center,
				Content = label
			};

			button.Click += clickHandler;
			return button;
		}
	}
}