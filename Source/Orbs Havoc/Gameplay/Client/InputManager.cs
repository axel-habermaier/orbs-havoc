// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Gameplay.Client
{
	using System.Numerics;
	using Network;
	using Network.Messages;
	using Platform.Input;
	using Platform.Memory;
	using SceneNodes.Entities;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Manages the input state of the local player.
	/// </summary>
	internal class InputManager : DisposableObject
	{
		private readonly LogicalInputDevice _inputDevice;
		private readonly LogicalInput _lightingGun;
		private readonly LogicalInput _miniGun;
		private readonly LogicalInput _nextWeapon;
		private readonly Player _player;
		private readonly LogicalInput _previousWeapon;
		private readonly LogicalInput _rocketLauncher;
		private Clock _clock = new Clock();
		private InputState _firePrimary;
		private InputState _fireSecondary;
		private uint _frameNumber;
		private InputState _moveDown;
		private InputState _moveLeft;
		private InputState _moveRight;
		private InputState _moveUp;
		private EntityType _primaryWeapon = EntityType.MiniGun;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="player">The local player whose input is managed.</param>
		/// <param name="inputDevice">The input device that should be used to obtain the player input.</param>
		public InputManager(Player player, LogicalInputDevice inputDevice)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentSatisfies(player.IsLocalPlayer, nameof(player), "Expected the local player.");
			Assert.ArgumentNotNull(inputDevice, nameof(inputDevice));

			_inputDevice = inputDevice;
			_player = player;

			_miniGun = new LogicalInput(Cvars.InputSelectMiniGunCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_rocketLauncher = new LogicalInput(Cvars.InputSelectRocketLauncherCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_lightingGun = new LogicalInput(Cvars.InputSelectLightingGunCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_nextWeapon = new LogicalInput(Cvars.InputNextWeaponCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_previousWeapon = new LogicalInput(Cvars.InputPreviousWeaponCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);

			_moveUp.Input = new LogicalInput(Cvars.InputMoveUpCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_moveDown.Input = new LogicalInput(Cvars.InputMoveDownCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_moveLeft.Input = new LogicalInput(Cvars.InputMoveLeftCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_moveRight.Input = new LogicalInput(Cvars.InputMoveRightCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_firePrimary.Input = new LogicalInput(Cvars.InputFirePrimaryCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);
			_fireSecondary.Input = new LogicalInput(Cvars.InputFireSecondaryCvar, KeyTriggerType.Pressed, MouseTriggerType.Pressed);

			_inputDevice.Add(_moveUp.Input);
			_inputDevice.Add(_moveDown.Input);
			_inputDevice.Add(_moveLeft.Input);
			_inputDevice.Add(_moveRight.Input);
			_inputDevice.Add(_firePrimary.Input);
			_inputDevice.Add(_fireSecondary.Input);

			_inputDevice.Add(_miniGun);
			_inputDevice.Add(_rocketLauncher);
			_inputDevice.Add(_lightingGun);
			_inputDevice.Add(_nextWeapon);
			_inputDevice.Add(_previousWeapon);
		}

		/// <summary>
		///   Updates the current input state.
		/// </summary>
		public void Update()
		{
			_moveUp.Triggered |= _moveUp.Input.IsTriggered;
			_moveDown.Triggered |= _moveDown.Input.IsTriggered;
			_moveLeft.Triggered |= _moveLeft.Input.IsTriggered;
			_moveRight.Triggered |= _moveRight.Input.IsTriggered;
			_firePrimary.Triggered |= _firePrimary.Input.IsTriggered;
			_fireSecondary.Triggered |= _fireSecondary.Input.IsTriggered;

			if (_miniGun.IsTriggered)
				_primaryWeapon = EntityType.MiniGun;
			else if (_rocketLauncher.IsTriggered)
				_primaryWeapon = EntityType.RocketLauncher;
			else if (_lightingGun.IsTriggered)
				_primaryWeapon = EntityType.LightingGun;
			else if (_nextWeapon.IsTriggered)
				SelectPrimaryWeapon(1);
			else if (_previousWeapon.IsTriggered)
				SelectPrimaryWeapon(-1);

			if (_player.Orb != null && _player.Orb.WeaponEnergyLevels[_primaryWeapon.GetWeaponSlot()] <= 0)
				SelectBestPrimaryWeapon();
		}

		/// <summary>
		///   Selects the bets available primary weapon.
		/// </summary>
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

		/// <summary>
		///   Selects the next or previous available primary weapon.
		/// </summary>
		private void SelectPrimaryWeapon(int direction)
		{
			if (_player.Orb == null)
				return;

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
		///   Sends the input state.
		/// </summary>
		/// <param name="gameSession">The game session the input should be provided for.</param>
		/// <param name="connection">The connection to the server the client input should be sent to.</param>
		public void SendInput(GameSession gameSession, Connection connection)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(connection, nameof(connection));

			// Ensure we don't spam the server with input message
			if (_clock.Milliseconds < 1000.0 / NetworkProtocol.InputUpdateFrequency)
				return;

			_clock.Reset();

			// Update the input states
			_moveUp.Update();
			_moveDown.Update();
			_moveLeft.Update();
			_moveRight.Update();
			_firePrimary.Update();
			_fireSecondary.Update();

			// Get the coordinates the player currently targets and end the input message to the server
			var target = _inputDevice.Mouse.Position - new Vector2(_inputDevice.Window.Size.Width / 2, _inputDevice.Window.Size.Height / 2);
			connection.EnqueueMessage(PlayerInputMessage.Create(
				gameSession.Allocator, gameSession.Players.LocalPlayer.Identity, ++_frameNumber, target,
				_moveUp.State, _moveDown.State,
				_moveLeft.State, _moveRight.State,
				_firePrimary.State, _fireSecondary.State,
				_primaryWeapon));
		}

		/// <summary>
		///   Sends the input state when the local game session is inactive, i.e., when a menu is open.
		/// </summary>
		/// <param name="gameSession">The game session the input should be provided for.</param>
		/// <param name="connection">The connection to the server the client input should be sent to.</param>
		public void SendInactiveInput(GameSession gameSession, Connection connection)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(connection, nameof(connection));

			// Ensure we don't spam the server with input message
			if (_clock.Milliseconds < 1000.0 / NetworkProtocol.InputUpdateFrequency)
				return;

			_clock.Reset();

			var primaryWeapon = _player.Orb?.PrimaryWeapon ?? EntityType.MiniGun;

			connection.EnqueueMessage(PlayerInputMessage.Create(
				gameSession.Allocator, gameSession.Players.LocalPlayer.Identity, ++_frameNumber, Vector2.Zero,
				0, 0, 0, 0, 0, 0, primaryWeapon));
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_inputDevice.Remove(_moveUp.Input);
			_inputDevice.Remove(_moveDown.Input);
			_inputDevice.Remove(_moveLeft.Input);
			_inputDevice.Remove(_moveRight.Input);
			_inputDevice.Remove(_firePrimary.Input);
			_inputDevice.Remove(_fireSecondary.Input);

			_inputDevice.Remove(_miniGun);
			_inputDevice.Remove(_rocketLauncher);
			_inputDevice.Remove(_lightingGun);
			_inputDevice.Remove(_nextWeapon);
			_inputDevice.Remove(_previousWeapon);
		}

		/// <summary>
		///   Stores the state of an input until it is sent to the server.
		/// </summary>
		private struct InputState
		{
			/// <summary>
			///   The logical input that is used to determine whether the input is triggered.
			/// </summary>
			public LogicalInput Input;

			/// <summary>
			///   The current trigger state, also including the seven previous ones.
			/// </summary>
			public byte State;

			/// <summary>
			///   Indicates whether the state has been triggered since the last update of the server.
			/// </summary>
			public bool Triggered;

			/// <summary>
			///   Removes the oldest trigger state from the given input state and adds the current one.
			/// </summary>
			public void Update()
			{
				State = (byte)((State << 1) | (Triggered ? 1 : 0));
				Triggered = false;
			}
		}
	}
}