namespace OrbsHavoc.Views
{
	using UI;

	internal sealed class RespawnOverlay : View<RespawnOverlayUI>
	{
		public override void Update()
		{
			UI.Update(Views.Game.GameSession.Players.LocalPlayer.RemainingRespawnDelay);
		}
	}
}