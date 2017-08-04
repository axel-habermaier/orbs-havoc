namespace OrbsHavoc.Views
{
	using UI;

	/// <summary>
	///     Represents waiting-for-server overlay during a game session.
	/// </summary>
	internal sealed class WaitingOverlay : View<WaitingOverlayUI>
	{
		public override void Update()
		{
			if (Views.Game.Connection.IsLagging)
				UI.Update(Views.Game.Connection.TimeToDrop / 1000);
			else
				Hide();
		}
	}
}