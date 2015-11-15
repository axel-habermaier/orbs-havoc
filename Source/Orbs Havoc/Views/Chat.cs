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
	using System;
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
	///   Represents the in-game chat input.
	/// </summary>
	internal class Chat : View
	{
		private readonly TextBox _input = new TextBox
		{
			AutoFocus = true,
			MaxLength = NetworkProtocol.ChatMessageLength,
			Row = 0,
			Column = 1,
			HorizontalAlignment = HorizontalAlignment.Stretch
		};

		private readonly Label _validationLabel = new Label
		{
			Text = "The message exceeds the maximum allowed length of a chat message and cannot be sent.",
			Margin = new Thickness(0, 10, 0, 0),
			Foreground = Colors.Red,
			Row = 1,
			Column = 1
		};

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			_input.TextChanged += _ => CheckLength();

			RootElement = new Border
			{
				CapturesInput = true,
				Font = AssetBundle.Roboto14,
				InputBindings =
				{
					new KeyBinding(Send, Key.Enter),
					new KeyBinding(Send, Key.NumpadEnter),
					new KeyBinding(Hide, Key.Escape)
				},
				Child = new StackPanel
				{
					Height = 200,
					VerticalAlignment = VerticalAlignment.Bottom,
					HorizontalAlignment = HorizontalAlignment.Center,
					Children =
					{
						new Border
						{
							Background = new Color(0x5F00588B),
							BorderColor = new Color(0xFF055674),
							Padding = new Thickness(5),
							Child = new Grid
							{
								Margin = new Thickness(5),
								VerticalAlignment = VerticalAlignment.Top,
								Columns = { new ColumnDefinition { Width = 40 }, new ColumnDefinition { Width = 600 } },
								Rows = { new RowDefinition { Height = Single.NaN }, new RowDefinition { Height = Single.NaN } },
								Children =
								{
									new Label
									{
										Text = "Say:",
										Column = 0,
										Row = 0,
										Margin = new Thickness(0, 4, 0, 0)
									},
									_input,
									_validationLabel
								}
							}
						}
					}
				}
			};
		}

		/// <summary>
		///   Checks whether the chat message entered by the user exceeds the maximum allowed length.
		/// </summary>
		private bool CheckLength()
		{
			var tooLong = Encoding.UTF8.GetByteCount(_input.Text) > NetworkProtocol.ChatMessageLength;
			_validationLabel.Visibility = tooLong ? Visibility.Visible : Visibility.Collapsed;

			return !tooLong;
		}

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected override void Activate()
		{
			_input.Text = String.Empty;
			_validationLabel.Visibility = Visibility.Collapsed;
		}


		/// <summary>
		///   Sends the message, if any, and closes the chat input.
		/// </summary>
		private void Send()
		{
			if (!CheckLength())
				return;

			if (!String.IsNullOrWhiteSpace(_input.Text))
				Commands.Say(_input.Text.Trim());

			Hide();
		}
	}
}