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

namespace PointWars.UserInterface.Controls
{
	using System;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a message box.
	/// </summary>
	public sealed class MessageBox : Control
	{
		/// <summary>
		///   Initializes the message box.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="button1">The first button shown in the message box.</param>
		/// <param name="button2">The second button shown in the message box.</param>
		private void Initialize(string title, string message, Button button1, Button button2)
		{
			Assert.ArgumentNotNullOrWhitespace(title, nameof(title));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			Assert.ArgumentNotNull(button1, nameof(button1));
			Assert.ArgumentNotNull(button2, nameof(button2));

			button1.Padding = new Thickness(0, 0, 3, 0);
			Child = new Border
			{
				Background = new Color(0xAA000000),
				Child = new Border
				{
					Font = Assets.LiberationSans16,
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
								Child = new Label(title) { Margin = new Thickness(7) }
							},
							new Label(message) { Margin = new Thickness(7), TextWrapping = TextWrapping.Wrap },
							new StackPanel
							{
								Margin = new Thickness(0, 0, 0, 5),
								HorizontalAlignment = HorizontalAlignment.Center,
								Orientation = Orientation.Horizontal,
								Children = { button1, button2 }
							}
						}
					}
				}
			};
		}

		/// <summary>
		///   Creates a confirmation message box with an OK and a Cancel button.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="onConfirmed">The continuation that should be executed when the user confirmed.</param>
		public static MessageBox ShowConfimation(string title, string message, Action onConfirmed)
		{
			Assert.ArgumentNotNull(onConfirmed, nameof(onConfirmed));

			var messageBox = new MessageBox();
			var okButton = messageBox.CreateButton("OK", onConfirmed);
			var cancelButton = messageBox.CreateButton("Cancel");

			messageBox.Initialize(title, message, okButton, cancelButton);
			return messageBox;
		}

		/// <summary>
		///   Creates a message box button.
		/// </summary>
		private Button CreateButton(string label, Action continuation = null)
		{
			var button = new Button
			{
				Width = 70,
				Margin = new Thickness(0, 20, 0, 3),
				HorizontalAlignment = HorizontalAlignment.Center,
				Child = new Border
				{
					Child = new Label(label) { TextAlignment = TextAlignment.Center },
					BorderThickness = new Thickness(1),
					BorderColor = new Color(0xFF055674),
					NormalStyle = element => ((Border)element).Background = new Color(0x5F00588B),
					HoveredStyle = element => ((Border)element).Background = new Color(0x5F0082CE),
					ActiveStyle = element => ((Border)element).Background = new Color(0x5F009CF7),
					Padding = new Thickness(7, 6, 7, 7)
				}
			};

			button.Click += OnButtonClicked(continuation);
			return button;
		}

		/// <summary>
		///   Generates the click handler for a message box button.
		/// </summary>
		private Action OnButtonClicked(Action continuation)
		{
			return () =>
			{
				continuation?.Invoke();

				var panel = Parent as Panel;
				Assert.NotNull(panel, "Expected to message box's parent to be a panel.");
				panel.Remove(this);
			};
		}
	}
}