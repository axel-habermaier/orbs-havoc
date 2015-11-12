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

namespace PointWars.Views
{
	using System.Collections.Generic;
	using Assets;
	using Network;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Represents the in-game scoreboard.
	/// </summary>
	internal class Scoreboard : View
	{
		private readonly PlayerRow[] _playerRows = new PlayerRow[NetworkProtocol.MaxPlayers];
		private bool _dirty = true;
		private List<RowDefinition> _gridRows;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			var grid = new Grid(columns: 4, rows: NetworkProtocol.MaxPlayers + 1)
			{
				Children =
				{
					CreateHeader("Player", 0, TextAlignment.Left, minWidth: 150),
					CreateHeader("Kills", 1, TextAlignment.Right, minWidth: 40),
					CreateHeader("Deaths", 2, TextAlignment.Right, minWidth: 50),
					CreateHeader("Ping", 3, TextAlignment.Right, minWidth: 35)
				}
			};
			
			for (var i = 0; i < NetworkProtocol.MaxPlayers; ++i)
				_playerRows[i] = new PlayerRow(grid, i + 1);

			RootElement = new Border
			{
				IsHitTestVisible = false,
				Background = new Color(0x5F00588B),
				Font = AssetBundle.Roboto14,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Padding = new Thickness(4),
				Child = grid
			};

			_gridRows = grid.Rows;
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			if (Views.Console.IsShown || Views.Chat.IsShown || Views.InGameMenu.IsShown)
				Hide();

			if (!_dirty)
				return;

			Assert.NotNull(Views.Game.GameSession);

			foreach (var row in _playerRows)
				row.Hide();

			foreach (var row in _gridRows)
				row.Background = Colors.Transparent;

			foreach (var player in Views.Game.GameSession.Players)
			{
				if (player.IsServerPlayer || player.Rank == 0)
					continue;

				_playerRows[player.Rank - 1].Show();
				_playerRows[player.Rank - 1].PlayerName.Text = player.Name;
				_playerRows[player.Rank - 1].Kills.Text = player.Kills.ToString();
				_playerRows[player.Rank - 1].Deaths.Text = player.Deaths.ToString();
				_playerRows[player.Rank - 1].Ping.Text = player.Ping.ToString();

				if (player.IsLocalPlayer)
					_gridRows[player.Rank].Background = new Color(0x33A1DDFF);
			}

			_dirty = false;
		}

		/// <summary>
		///   Updates the UI to reflect changes to the connected players or their data.
		/// </summary>
		public void OnPlayersChanged()
		{
			_dirty = true;
		}

		/// <summary>
		///   Creates a column header.
		/// </summary>
		private static UIElement CreateHeader(string header, int column, TextAlignment alignment, int minWidth = 0)
		{
			return new Border
			{
				BorderColor = new Color(0xFF055674),
				BorderThickness = new Thickness(0, 0, 0, 1),
				Row = 0,
				Column = column,
				Padding = new Thickness(0, 0, 0, 2),
				Child = new Label
				{
					Text = header,
					TextAlignment = alignment,
					Margin = new Thickness(4),
					MinWidth = minWidth
				}
			};
		}

		/// <summary>
		///   Represents a set of labels that constitutes a row of the scoreboard.
		/// </summary>
		private struct PlayerRow
		{
			public readonly Label PlayerName;
			public readonly Label Kills;
			public readonly Label Deaths;
			public readonly Label Ping;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="grid">The grid the player row should be added to.</param>
			/// <param name="row">The row number.</param>
			public PlayerRow(Grid grid, int row)
			{
				PlayerName = new Label
				{
					HorizontalAlignment = HorizontalAlignment.Left,
					Row = row,
					Column = 0,
					Margin = new Thickness(4),
				};

				Kills = new Label
				{
					HorizontalAlignment = HorizontalAlignment.Right,
					Row = row,
					Column = 1,
					Margin = new Thickness(4),
				};

				Deaths = new Label
				{
					HorizontalAlignment = HorizontalAlignment.Right,
					Row = row,
					Column = 2,
					Margin = new Thickness(4),
				};

				Ping = new Label
				{
					HorizontalAlignment = HorizontalAlignment.Right,
					Row = row,
					Column = 3,
					Margin = new Thickness(4),
				};

				grid.Children.Add(PlayerName);
				grid.Children.Add(Kills);
				grid.Children.Add(Deaths);
				grid.Children.Add(Ping);

				Hide();
			}

			/// <summary>
			///   Shows the player row.
			/// </summary>
			public void Show()
			{
				PlayerName.Visibility = Visibility.Visible;
				Kills.Visibility = Visibility.Visible;
				Deaths.Visibility = Visibility.Visible;
				Ping.Visibility = Visibility.Visible;
			}

			/// <summary>
			///   Hides the player row.
			/// </summary>
			public void Hide()
			{
				PlayerName.Visibility = Visibility.Collapsed;
				Kills.Visibility = Visibility.Collapsed;
				Deaths.Visibility = Visibility.Collapsed;
				Ping.Visibility = Visibility.Collapsed;
			}
		}
	}
}