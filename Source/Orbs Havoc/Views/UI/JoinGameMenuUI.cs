namespace OrbsHavoc.Views.UI
{
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using Assets;
	using Network;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class JoinGameMenuUI : Border
	{
		private readonly Panel _serversPanel = new StackPanel();

		public JoinGameMenuUI()
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
						Text = "Join Game",
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
								Width = 120,
								Row = 0,
								Column = 0,
								VerticalAlignment = VerticalAlignment.Center,
								Text = "Server Address:"
							},
							(Address = new TextBox
							{
								Row = 0,
								Column = 1,
								Margin = new Thickness(5, 0, 0, 5),
								Width = 200,
								MaxLength = NetworkProtocol.ServerNameLength,
							}),
							(InvalidAddress = new Label
							{
								Row = 1,
								Column = 1,
								Text = "Expected a valid server name.",
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
								MaxLength = 5,
							}),
							(InvalidPort = new Label
							{
								Width = 200,
								Row = 3,
								Column = 1,
								Text = "Expected a valid server port.",
								Margin = new Thickness(5, 0, 0, 5),
								Foreground = Colors.Red,
								TextWrapping = TextWrapping.Wrap,
								VerticalAlignment = VerticalAlignment.Center,
								Visibility = Visibility.Collapsed
							}),
							(Connect = new Button
							{
								Row = 4,
								Column = 1,
								Content = "Connect",
								HorizontalAlignment = HorizontalAlignment.Left,
								Margin = new Thickness(5, 10, 0, 50),
							})
						}
					},
					new StackPanel
					{
						Width = 330,
						Children =
						{
							new Label
							{
								TextWrapping = TextWrapping.Wrap,
								Text = "Click on one of the following servers to join a game:"
							},
							_serversPanel
						}
					},
					(Return = new Button
					{
						Content = "Return",
						HorizontalAlignment = HorizontalAlignment.Center,
						Margin = new Thickness(0, 10, 0, 0)
					})
				}
			};
		}

		public TextBox Address { get; }
		public UIElement InvalidAddress { get; }
		public UIElement InvalidPort { get; }
		public TextBox Port { get; }

		public Button Return { get; }
		public Button Connect { get; }

		public void SetServers(IEnumerable<(string Name, IPEndPoint EndPoint)> servers)
		{
			_serversPanel.Clear();

			foreach (var server in servers.OrderBy(s => s.Name))
			{
				_serversPanel.Add(new Button
				{
					Content = new Label { Text = $"{server.Name}\\default @ {server.EndPoint}", TextWrapping = TextWrapping.Wrap },
					Margin = new Thickness(0, 2, 0, 2),
					Click = () => Commands.Connect(server.EndPoint.Address.ToString(), (ushort)server.EndPoint.Port)
				});
			}
		}
	}
}