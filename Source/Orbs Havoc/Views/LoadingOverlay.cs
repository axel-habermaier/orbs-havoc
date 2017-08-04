namespace OrbsHavoc.Views
{
	using System.Net;
	using Platform.Input;
	using Platform.Logging;
	using Scripting;
	using UI;
	using UserInterface.Input;
	using Utilities;

	internal sealed class LoadingOverlay : View<LoadingOverlayUI>
	{
		private Clock _clock = new Clock();
		private IPEndPoint _serverEndPoint;

		public override void InitializeUI()
		{
			UI.InputBindings.Add(new KeyBinding(Commands.Disconnect, Key.Escape));
		}

		public void Load(IPEndPoint serverEndPoint)
		{
			Assert.ArgumentNotNull(serverEndPoint, nameof(serverEndPoint));

			_serverEndPoint = serverEndPoint;
			_clock.Reset();

			Show();
			Log.Info($"Connecting to {serverEndPoint}...");

			Views.Console.Hide();
			Views.MessageBoxes.CloseAll();
			Views.JoinGameMenu.Hide();
			Views.StartGameMenu.Hide();
			Views.OptionsMenu.Hide();
			Views.MainMenu.Hide();
		}

		public override void Update()
		{
			UI.Update(_serverEndPoint, _clock.Seconds);
		}
	}
}