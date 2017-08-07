namespace OrbsHavoc.Gameplay.Client
{
	using System.Diagnostics;
	using System.Numerics;
	using Network;
	using Network.Messages;
	using Platform;
	using UserInterface.Input;
	using SceneNodes.Entities;
	using Scripting;
	using Utilities;

	internal class InputManager
	{
		private readonly Keyboard _keyboard;
		private readonly Mouse _mouse;
		private readonly Player _player;
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
		private readonly Window _window;
		private InputState _firePrimary;
		private InputState _fireSecondary;
		private uint _frameNumber;
		private InputState _moveDown;
		private InputState _moveLeft;
		private InputState _moveRight;
		private InputState _moveUp;
		private EntityType _primaryWeapon = EntityType.MiniGun;

		public InputManager(Player player, Window window, Keyboard keyboard, Mouse mouse)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNull(window, nameof(window));
			Assert.ArgumentSatisfies(player.IsLocalPlayer, nameof(player), "Expected the local player.");
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentNotNull(mouse, nameof(mouse));

			_player = player;
			_window = window;
			_keyboard = keyboard;
			_mouse = mouse;

			_moveUp.Input = Cvars.InputMoveUpCvar;
			_moveDown.Input = Cvars.InputMoveDownCvar;
			_moveLeft.Input = Cvars.InputMoveLeftCvar;
			_moveRight.Input = Cvars.InputMoveRightCvar;
			_firePrimary.Input = Cvars.InputFirePrimaryCvar;
			_fireSecondary.Input = Cvars.InputFireSecondaryCvar;
		}

		public bool ShowScoreboard => Cvars.InputShowScoreboard.IsTriggered(_keyboard, _mouse);

		public void Update()
		{
			_moveUp.UpdateTriggered(_keyboard, _mouse);
			_moveDown.UpdateTriggered(_keyboard, _mouse);
			_moveLeft.UpdateTriggered(_keyboard, _mouse);
			_moveRight.UpdateTriggered(_keyboard, _mouse);
			_firePrimary.UpdateTriggered(_keyboard, _mouse);
			_fireSecondary.UpdateTriggered(_keyboard, _mouse);

			if (Cvars.InputSelectMiniGun.IsTriggered(_keyboard, _mouse, TriggerType.WentDown))
				_primaryWeapon = EntityType.MiniGun;
			else if (Cvars.InputSelectRocketLauncher.IsTriggered(_keyboard, _mouse, TriggerType.WentDown))
				_primaryWeapon = EntityType.RocketLauncher;
			else if (Cvars.InputSelectLightingGun.IsTriggered(_keyboard, _mouse, TriggerType.WentDown))
				_primaryWeapon = EntityType.LightingGun;
			else if (Cvars.InputNextWeapon.IsTriggered(_keyboard, _mouse, TriggerType.WentDown))
				SelectPrimaryWeapon(1);
			else if (Cvars.InputPreviousWeapon.IsTriggered(_keyboard, _mouse, TriggerType.WentDown))
				SelectPrimaryWeapon(-1);

			if (_player.Orb != null && _player.Orb.WeaponEnergyLevels[_primaryWeapon.GetWeaponSlot()] <= 0)
				SelectBestPrimaryWeapon();
		}

		private void SelectBestPrimaryWeapon()
		{
			if (_player.Orb.WeaponEnergyLevels[EntityType.Bfg.GetWeaponSlot()] > 0)
				_primaryWeapon = EntityType.Bfg;
			else if (_player.Orb.WeaponEnergyLevels[EntityType.RocketLauncher.GetWeaponSlot()] > 0)
				_primaryWeapon = EntityType.RocketLauncher;
			else if (_player.Orb.WeaponEnergyLevels[EntityType.LightingGun.GetWeaponSlot()] > 0)
				_primaryWeapon = EntityType.LightingGun;
			else if (_player.Orb.WeaponEnergyLevels[EntityType.PlasmaGun.GetWeaponSlot()] > 0)
				_primaryWeapon = EntityType.PlasmaGun;
			else
				_primaryWeapon = EntityType.MiniGun;
		}

		private void SelectPrimaryWeapon(int direction)
		{
			if (_player.Orb == null)
				return;

			// This loop is guaranteed to terminate because the minigun is always selectable
			while (true)
			{
				var slot = _primaryWeapon.GetWeaponSlot();
				var selectedSlot = (slot + direction + Orb.WeaponsCount) % Orb.WeaponsCount;
				_primaryWeapon = selectedSlot.GetWeaponFromSlot();

				if (_player.Orb.WeaponEnergyLevels[_primaryWeapon.GetWeaponSlot()] > 0)
					return;
			}
		}

		/// <summary>
		///     Sends the input state to the server while the player is actively playing.
		/// </summary>
		public void SendActiveInput(GameSession gameSession, Connection connection)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(connection, nameof(connection));

			if (!CheckSendInterval())
				return;

			// Update the input states
			_moveUp.UpdateSendState();
			_moveDown.UpdateSendState();
			_moveLeft.UpdateSendState();
			_moveRight.UpdateSendState();
			_firePrimary.UpdateSendState();
			_fireSecondary.UpdateSendState();

			// Get the coordinates the player currently targets and end the input message to the server
			var target = _mouse.Position - _window.Size / 2;

			connection.EnqueueMessage(PlayerInputMessage.Create(
				gameSession.Allocator, gameSession.Players.LocalPlayer.Identity, ++_frameNumber, target,
				_moveUp.State, _moveDown.State,
				_moveLeft.State, _moveRight.State,
				_firePrimary.State, _fireSecondary.State,
				_primaryWeapon));
		}

		/// <summary>
		///     Sends the input state when the local game session is inactive, i.e., when a menu is open.
		/// </summary>
		public void SendInactiveInput(GameSession gameSession, Connection connection)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(connection, nameof(connection));

			if (!CheckSendInterval())
				return;

			connection.EnqueueMessage(PlayerInputMessage.Create(
				gameSession.Allocator, gameSession.Players.LocalPlayer.Identity, ++_frameNumber, Vector2.Zero,
				0, 0, 0, 0, 0, 0, _primaryWeapon));
		}

		private bool CheckSendInterval()
		{
			if (_stopwatch.Elapsed.TotalMilliseconds < 1000.0 / NetworkProtocol.InputUpdateFrequency)
				return false;

			_stopwatch.Restart();
			return true;
		}

		private struct InputState
		{
			public Cvar<InputTrigger> Input;
			public byte State;
			private bool _triggered;

			public void UpdateSendState()
			{
				State = (byte)((State << 1) | (_triggered ? 1 : 0));
				_triggered = false;
			}

			public void UpdateTriggered(Keyboard keyboard, Mouse mouse)
			{
				_triggered |= Input.Value.IsTriggered(keyboard, mouse);
			}
		}
	}
}