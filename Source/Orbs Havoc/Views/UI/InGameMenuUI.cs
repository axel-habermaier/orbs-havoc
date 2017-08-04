namespace OrbsHavoc.Views.UI
{
	using Assets;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class InGameMenuUI : Border
	{
		public InGameMenuUI()
		{
			Background = new Color(0xAA000000);
			CapturesInput = true;
			AutoFocus = true;

			Child = new StackPanel
			{
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				Children =
				{
					new Label
					{
						Text = "Paused",
						Font = AssetBundle.Moonhouse80,
						Margin = new Thickness(0, 0, 0, 30),
					},
					(Continue = CreateButton("Continue")),
					(Options = CreateButton("Options")),
					(Leave = CreateButton("Leave")),
					(Exit = CreateButton("Exit"))
				}
			};
		}

		public Button Continue { get; }
		public Button Leave { get; }
		public Button Exit { get; }
		public Button Options { get; }

		private static Button CreateButton(string label)
		{
			return new Button
			{
				Font = AssetBundle.Moonhouse24,
				Width = 200,
				Content = label,
				Margin = new Thickness(4),
			};
		}
	}
}