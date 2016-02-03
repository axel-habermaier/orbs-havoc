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
	using System;
	using System.Net;
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
	///   Lets the user start a new game.
	/// </summary>
	internal sealed class StartGameMenu : View
	{
		private UIElement _invalidName;
		private UIElement _invalidPort;
		private TextBox _name;
		private TextBox _port;

		/// <summary>
		///   Gets the server name entered by the user or null if the name is invalid.
		/// </summary>
		private string ServerName => Encoding.UTF8.GetByteCount(_name.Text) > NetworkProtocol.ServerNameLength ? null : _name.Text;

		/// <summary>
		///   Gets the server port entered by the user or null if the port is invalid.
		/// </summary>
		private ushort? ServerPort
		{
			get
			{
				ushort port;
				return UInt16.TryParse(_port.Text, out port) ? port : (ushort?)null;
			}
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				CapturesInput = true,
				IsFocusable = true,
				Font = AssetBundle.Roboto14,
				AutoFocus = true,
				InputBindings =
				{
					new KeyBinding(() =>
					{
						Hide();
						Views.MainMenu.Show();
					}, Key.Escape)
				},
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
							Margin = new Thickness(0, 0, 0, 30),
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
								(_name = new TextBox
								{
									Row = 0,
									Column = 1,
									Margin = new Thickness(5, 0, 0, 5),
									Width = 200,
									MaxLength = NetworkProtocol.ServerNameLength,
									TextChanged = OnAddressChanged
								}),
								(_invalidName = new Label
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
								(_port = new TextBox
								{
									Row = 2,
									Column = 1,
									Margin = new Thickness(5, 0, 0, 5),
									Width = 200,
									MaxLength = NetworkProtocol.ServerNameLength,
									TextChanged = OnPortChanged
								}),
								(_invalidPort = new Label
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
								new Button
								{
									Content = "Start Game",
									Margin = new Thickness(0, 0, 10, 0),
									Click = StartGame
								},
								new Button
								{
									Content = "Return",
									Click = () =>
									{
										Hide();
										Views.MainMenu.Show();
									}
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
			_name.Text = $"{Environment.UserName}'s Server";
			_port.Text = NetworkProtocol.DefaultServerPort.ToString();
		}

		/// <summary>
		///   Invoked when the user entered another address.
		/// </summary>
		private void OnAddressChanged(string address)
		{
			_invalidName.Visibility = ServerName == null ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Invoked when the user entered another port.
		/// </summary>
		private void OnPortChanged(string port)
		{
			_invalidPort.Visibility = ServerPort == null ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///   Starts a local game server.
		/// </summary>
		private void StartGame()
		{
			if (String.IsNullOrWhiteSpace(ServerName) || ServerPort == null)
				return;

			if (Views.TryStartHost(ServerName, ServerPort.Value))
				Commands.Connect(IPAddress.IPv6Loopback, ServerPort.Value);
		}
	}
}