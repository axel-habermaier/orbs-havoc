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
	using Assets;
	using Input;
	using Platform.Input;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a message box.
	/// </summary>
	public sealed class MessageBox : Control
	{
		private Action _continuation1;
		private Action _continuation2;

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

			IsFocusable = true;

			InputBindings.Add(new KeyBinding(Button1Clicked, Key.Enter, triggerMode: TriggerMode.OnDeactivation));
			InputBindings.Add(new KeyBinding(Button1Clicked, Key.Space, triggerMode: TriggerMode.OnDeactivation));
			InputBindings.Add(new KeyBinding(Button1Clicked, Key.NumpadEnter, triggerMode: TriggerMode.OnDeactivation));
			InputBindings.Add(new KeyBinding(Button2Clicked, Key.Escape, triggerMode: TriggerMode.OnDeactivation));

			button1.Padding = new Thickness(0, 0, 3, 0);
			Content = new Border
			{
				Background = new Color(0xAA000000),
				Content = new Border
				{
					Font = Assets.Roboto14,
					MaxWidth = 600,
					MinWidth = 200,
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					BorderColor = new Color(0xFF055674),
					Background = new Color(0xFF002033),
					Content = new StackPanel
					{
						Children =
						{
							new Border
							{
								Background = new Color(0x33A1DDFF),
								Content = new Label(title) { Margin = new Thickness(15, 7, 15, 7) }
							},
							new Label(message)
							{
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
				}
			};
		}

		/// <summary>
		///   Creates a confirmation message box with an OK and a Cancel button.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="onConfirmed">The continuation that should be executed when the user confirmed.</param>
		public static MessageBox ShowOkCancel(string title, string message, Action onConfirmed)
		{
			var messageBox = new MessageBox { _continuation1 = onConfirmed };
			var okButton = CreateButton("OK", messageBox.Button1Clicked);
			var cancelButton = CreateButton("Cancel", messageBox.Button2Clicked);

			messageBox.Initialize(title, message, okButton, cancelButton);
			return messageBox;
		}

		/// <summary>
		///   Creates a confirmation message box with a Yes and a No button.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="yesContinuation">The continuation that should be executed when the user pressed the Yes button.</param>
		/// <param name="noContinuation">The continuation that should be executed when the user pressed the No button.</param>
		public static MessageBox ShowYesNo(string title, string message, Action yesContinuation, Action noContinuation = null)
		{
			var messageBox = new MessageBox { _continuation1 = yesContinuation, _continuation2 = noContinuation };
			var yesButton = CreateButton("Yes", messageBox.Button1Clicked);
			var noButton = CreateButton("No", messageBox.Button2Clicked);

			messageBox.Initialize(title, message, yesButton, noButton);
			return messageBox;
		}

		/// <summary>
		///   Creates a error message box.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		public static MessageBox ShowError(string title, string message)
		{
			var messageBox = new MessageBox();
			var okButton = CreateButton("OK", messageBox.Button1Clicked);
			var hiddenButton = new Button { Visibility = Visibility.Collapsed };

			messageBox.Initialize(title, message, okButton, hiddenButton);
			return messageBox;
		}

		/// <summary>
		///   Creates a message box button.
		/// </summary>
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

		/// <summary>
		///   Handles a click on the first button.
		/// </summary>
		private void Button1Clicked()
		{
			_continuation1?.Invoke();
			Close();
		}

		/// <summary>
		///   Handles a click on the second button.
		/// </summary>
		private void Button2Clicked()
		{
			_continuation2?.Invoke();
			Close();
		}

		/// <summary>
		///   Closes the message box.
		/// </summary>
		private void Close()
		{
			var panel = Parent as Panel;
			Assert.NotNull(panel, "Expected to message box's parent to be a panel.");

			panel.Remove(this);
		}
	}
}