namespace OrbsHavoc.Views
{
	using UI;
	using Utilities;

	internal class Scoreboard : View<ScoreboardUI>
	{
		private bool _dirty = true;

		public override void Update()
		{
			if (Views.Console.IsShown || Views.Chat.IsShown || Views.InGameMenu.IsShown)
				Hide();

			if (!_dirty)
				return;

			Assert.NotNull(Views.Game.GameSession);
			UI.Update(Views.Game.GameSession.Players);

			_dirty = false;
		}

		public void OnPlayersChanged()
		{
			_dirty = true;
		}
	}
}