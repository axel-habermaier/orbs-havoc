namespace OrbsHavoc.Views
{
	using UI;

	internal sealed class MainMenu : View<MainMenuUI>
	{
		public override void Initialize()
		{
			Show();
		}

		public override void InitializeUI()
		{
			UI.Start.Click = () =>
			{
				Views.StartGameMenu.Show();
				Hide();
			};

			UI.Join.Click = () =>
			{
				Views.JoinGameMenu.Show();
				Hide();
			};

			UI.Options.Click = () =>
			{
				Views.OptionsMenu.Show();
				Hide();
			};

			UI.Exit.Click = Views.Exit;
		}
	}
}