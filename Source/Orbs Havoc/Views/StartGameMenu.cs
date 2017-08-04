namespace OrbsHavoc.Views
{
	using System;
	using System.Text;
	using Gameplay.Server;
	using Network;
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal sealed class StartGameMenu : View<StartGameMenuUI>
	{
		private string ServerName => Encoding.UTF8.GetByteCount(UI.Name.Text) > NetworkProtocol.ServerNameLength ? null : UI.Name.Text;
		private ushort? ServerPort => UInt16.TryParse(UI.Port.Text, out var port) ? port : (ushort?)null;

		public override void InitializeUI()
		{
			UI.InputBindings.AddRange(
				new KeyBinding(() =>
				{
					Hide();
					Views.MainMenu.Show();
				}, Key.Escape),
				new KeyBinding(StartGame, Key.Enter),
				new KeyBinding(StartGame, Key.NumpadEnter)
			);

			UI.Name.TextChanged = OnNameChanged;
			UI.Port.TextChanged = OnPortChanged;
			UI.StartGame.Click = StartGame;
			UI.Return.Click = () =>
			{
				Hide();
				Views.MainMenu.Show();
			};
		}

		protected override void Activate()
		{
			UI.Name.Text = GameSessionHost.DefaultServerName;
			UI.Port.Text = NetworkProtocol.DefaultServerPort.ToString();
		}

		private void OnNameChanged(string address)
		{
			UI.InvalidName.Visibility = TextString.IsNullOrWhiteSpace(ServerName) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OnPortChanged(string port)
		{
			UI.InvalidPort.Visibility = ServerPort == null ? Visibility.Visible : Visibility.Collapsed;
		}

		private void StartGame()
		{
			if (TextString.IsNullOrWhiteSpace(ServerName) || !(ServerPort is ushort port))
				return;

			if (Views.TryStartHost(ServerName, port))
				Commands.Connect("::1", port);
		}
	}
}