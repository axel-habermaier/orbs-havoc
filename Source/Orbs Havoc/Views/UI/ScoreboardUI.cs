namespace OrbsHavoc.Views.UI
{
	using System.Collections.Generic;
	using Assets;
	using Gameplay;
	using Network;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	internal class ScoreboardUI : Border
	{
		private readonly List<RowDefinition> _gridRows;
		private readonly PlayerRow[] _playerRows = new PlayerRow[NetworkProtocol.MaxPlayers];

		public ScoreboardUI()
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

			IsHitTestVisible = false;
			Background = new Color(0x5F00588B);
			Font = AssetBundle.Roboto14;
			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;
			Padding = new Thickness(4);
			Child = grid;

			_gridRows = grid.Rows;
		}

		public void Update(PlayerCollection players)
		{
			Assert.ArgumentNotNull(players, nameof(players));

			foreach (var row in _playerRows)
				row.SetVisibility(Visibility.Collapsed);

			foreach (var row in _gridRows)
				row.Background = Colors.Transparent;

			foreach (var player in players)
			{
				if (player.IsServerPlayer || player.Rank == 0)
					continue;

				_playerRows[player.Rank - 1].SetVisibility(Visibility.Visible);
				_playerRows[player.Rank - 1].PlayerName.Text = player.Name;
				_playerRows[player.Rank - 1].Kills.Text = player.Kills.ToString();
				_playerRows[player.Rank - 1].Deaths.Text = player.Deaths.ToString();
				_playerRows[player.Rank - 1].Ping.Text = player.Ping.ToString();

				if (player.IsLocalPlayer)
					_gridRows[player.Rank].Background = new Color(0x33A1DDFF);
			}
		}

		private static UIElement CreateHeader(string header, int column, TextAlignment alignment, int minWidth = 0)
		{
			return new Border
			{
				BorderColor = new Color(0xFF055674),
				BorderThickness = new Thickness(0, 0, 0, 1),
				Row = 0,
				Column = column,
				Padding = new Thickness(0, 0, 0, 2),
				Margin = new Thickness(0, 0, 0, 2),
				Child = new Label
				{
					Text = header,
					TextAlignment = alignment,
					Margin = new Thickness(4),
					MinWidth = minWidth
				}
			};
		}

		private struct PlayerRow
		{
			public readonly Label PlayerName;
			public readonly Label Kills;
			public readonly Label Deaths;
			public readonly Label Ping;

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

				SetVisibility(Visibility.Collapsed);
			}

			public void SetVisibility(Visibility visibility)
			{
				PlayerName.Visibility = visibility;
				Kills.Visibility = visibility;
				Deaths.Visibility = visibility;
				Ping.Visibility = visibility;
			}
		}
	}
}