namespace OrbsHavoc.Views
{
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface.Input;

	internal sealed class InGameMenu : View<InGameMenuUI>
	{
		public override void InitializeUI()
		{
			UI.InputBindings.Add(new KeyBinding(Hide, Key.Escape));
			UI.Continue.Click += Hide;
			UI.Options.Click += () =>
			{
				Hide();
				Views.OptionsMenu.Show();
			};
			UI.Leave.Click += Leave;
			UI.Exit.Click += Views.Exit;
		}

		private void Leave()
		{
			Views.MessageBoxes.ShowYesNo("Leave Game", "Do you really want to leave the game?", Commands.Disconnect);
		}
	}
}