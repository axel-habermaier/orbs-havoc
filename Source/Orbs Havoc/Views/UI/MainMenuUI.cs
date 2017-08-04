namespace OrbsHavoc.Views.UI
{
	using Assets;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class MainMenuUI : StackPanel
	{
		public MainMenuUI()
		{
			HorizontalAlignment = HorizontalAlignment.Center;
			VerticalAlignment = VerticalAlignment.Center;

			Children.AddRange(
				new Label
				{
					Text = Application.Name,
					Font = AssetBundle.Moonhouse80,
					Margin = new Thickness(0, 0, 0, 30)
				},
				Start = CreateButton("Start Game"),
				Join = CreateButton("Join Game"),
				Options = CreateButton("Options"),
				Exit = CreateButton("Exit")
			);
		}

		public Button Start { get; }
		public Button Join { get; }
		public Button Options { get; }
		public Button Exit { get; }

		private static Button CreateButton(string label)
		{
			return new Button
			{
				Font = AssetBundle.Moonhouse24,
				Width = 200,
				Content = label,
				Margin = new Thickness(4)
			};
		}
	}
}