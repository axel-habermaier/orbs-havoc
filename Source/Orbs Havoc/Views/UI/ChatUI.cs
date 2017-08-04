namespace OrbsHavoc.Views.UI
{
	using System;
	using Assets;
	using Network;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class ChatUI : Border
	{
		public ChatUI()
		{
			CapturesInput = true;
			Font = AssetBundle.Roboto14;
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
								(ChatMessage = new TextBox
								{
									AutoFocus = true,
									MaxLength = NetworkProtocol.ChatMessageLength,
									Row = 0,
									Column = 1,
									HorizontalAlignment = HorizontalAlignment.Stretch
								}),
								(ValidationLabel = new Label
								{
									Text = "The message exceeds the maximum allowed length of a chat message and cannot be sent.",
									Margin = new Thickness(0, 10, 0, 0),
									Foreground = Colors.Red,
									Row = 1,
									Column = 1
								})
							}
						}
					}
				}
			};
		}

		public TextBox ChatMessage { get; }
		public Label ValidationLabel { get; }
	}
}