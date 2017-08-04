namespace OrbsHavoc.Views
{
	using System.Collections.Generic;
	using System.Text;
	using Network;
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal sealed class OptionsMenu : View<OptionsMenuUI>
	{
		private string PlayerName => Encoding.UTF8.GetByteCount(UI.Name.Text) > NetworkProtocol.PlayerNameLength ? null : UI.Name.Text;

		public override void InitializeUI()
		{
			UI.InputBindings.Add(new KeyBinding(ShowPreviousMenu, Key.Escape));
			UI.Name.TextChanged = OnNameChanged;
			UI.Save.Click = Save;
			UI.Return.Click = ShowPreviousMenu;
		}

		protected override void Activate()
		{
			UI.Name.Text = Cvars.PlayerName;
			UI.Vsync.IsChecked = Cvars.Vsync;
			UI.DebugOverlay.IsChecked = Cvars.ShowDebugOverlay;
			UI.Bloom.IsChecked = Cvars.BloomEnabled;
		}

		private void OnNameChanged(string name)
		{
			UI.InvalidName.Visibility = TextString.IsNullOrWhiteSpace(PlayerName) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void Save()
		{
			if (TextString.IsNullOrWhiteSpace(PlayerName))
				return;

			ChangeValue(Cvars.PlayerNameCvar, PlayerName);
			ChangeValue(Cvars.VsyncCvar, UI.Vsync.IsChecked);
			ChangeValue(Cvars.ShowDebugOverlayCvar, UI.DebugOverlay.IsChecked);
			ChangeValue(Cvars.BloomEnabledCvar, UI.Bloom.IsChecked);

			ShowPreviousMenu();
		}

		private static void ChangeValue<T>(Cvar<T> cvar, T value)
		{
			if (!EqualityComparer<T>.Default.Equals(cvar.Value, value))
				cvar.Value = value;
		}

		private void ShowPreviousMenu()
		{
			Hide();

			if (Views.Game.IsShown)
				Views.InGameMenu.Show();
			else
				Views.MainMenu.Show();
		}
	}
}