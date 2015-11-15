// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace OrbsHavoc.Gameplay.Behaviors
{
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   A base class for weapon behaviors, managing weapon cooldown and energy.
	/// </summary>
	internal abstract class WeaponBehavior : Behavior<Orb>
	{
		/// <summary>
		///   Indicates whether the weapon is currently firing.
		/// </summary>
		private bool _isFiring;

		/// <summary>
		///   The remaining amount of time until the next amount of energy is depleted from a continuously firing weapon.
		/// </summary>
		private float _nextDeplete;

		/// <summary>
		///   The remaining amount of time in seconds before the weapon can be fired again.
		/// </summary>
		private float _remainingCooldown;

		/// <summary>
		///   Gets or sets the weapon's template.
		/// </summary>
		protected Game.WeaponTemplate Template { get; set; }

		/// <summary>
		///   Gets or sets the weapon's energy level.
		/// </summary>
		private int Energy
		{
			get { return SceneNode.WeaponEnergyLevels[Template.WeaponType.GetWeaponSlot()]; }
			set { SceneNode.WeaponEnergyLevels[Template.WeaponType.GetWeaponSlot()] = value; }
		}

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected sealed override void OnAttached()
		{
			_isFiring = false;
			_remainingCooldown = 0;
			_nextDeplete = 0;
		}

		/// <summary>
		///   Fires a single shot of a non-continuous weapon.
		/// </summary>
		protected virtual void Fire()
		{
			Assert.NotReached("Non-continuous weapons must override the Fire method.");
		}

		/// <summary>
		///   Starts firing a continuous weapon.
		/// </summary>
		protected virtual void StartFiring()
		{
			Assert.NotReached("Continuous weapons must override the StartFiring method.");
		}

		/// <summary>
		///   Stops firing a continuous weapon.
		/// </summary>
		protected virtual void StopFiring()
		{
			Assert.NotReached("Continuous weapons must override the StopFiring method.");
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public sealed override void Execute(float elapsedSeconds)
		{
			_remainingCooldown -= elapsedSeconds;
			_nextDeplete -= elapsedSeconds;

			if (!_isFiring)
				return;

			if (_remainingCooldown <= 0 && !Template.FiresContinuously)
			{
				_remainingCooldown = Template.Cooldown;

				Energy -= Template.DepleteSpeed;
				if (Energy > 0)
					Fire();
			}

			if (Energy == 0 && Template.FiresContinuously)
			{
				_isFiring = false;
				StopFiring();
			}

			if (Template.FiresContinuously && _nextDeplete <= 0)
			{
				--Energy;
				_nextDeplete = 1.0f / Template.DepleteSpeed;
			}

			Energy = MathUtils.Clamp(Energy, (byte)0, Template.MaxEnergy);
		}

		/// <summary>
		///   Handles the given player input.
		/// </summary>
		/// <param name="fireWeapon">Indicates whether the weapon should be fired.</param>
		public void HandlePlayerInput(bool fireWeapon)
		{
			var canFire = Energy > 0 && _remainingCooldown <= 0;
			var fire = fireWeapon && (_isFiring || canFire);

			if (!_isFiring && fire && Template.FiresContinuously)
			{
				_isFiring = true;
				_nextDeplete = 0;
				StartFiring();
			}
			else if (_isFiring && !fire && Template.FiresContinuously)
			{
				_isFiring = false;
				StopFiring();
			}
			else
				_isFiring = fire;
		}
	}
}