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

namespace PointWars.Gameplay
{
	/// <summary>
	///   The configurable parameters of a weapon.
	/// </summary>
	internal struct WeaponTemplate
	{
		/// <summary>
		///   The template of the mini gun that fires bullets.
		/// </summary>
		public static WeaponTemplate MiniGun = new WeaponTemplate
		{
			Cooldown = 0.1f,
			DepleteSpeed = 0,
			BaseSpeed = 1000,
			WeaponType = WeaponType.MiniGun,
			MaxEnergy = 1
		};

		/// <summary>
		///   The template of the lighting gun that creates a lighting beam.
		/// </summary>
		public static WeaponTemplate LightingGun = new WeaponTemplate
		{
			Cooldown = -1,
			DepleteSpeed = 50,
			Range = 2000,
			WeaponType = WeaponType.LightingGun,
			MaxEnergy = 200
		};

		/// <summary>
		///   The speed of a projectile released by the weapon.
		/// </summary>
		public float BaseSpeed;

		/// <summary>
		///   The amount of seconds to wait before to consecutive shots of the weapon. A negative value indicates that the weapon
		///   fires continuously.
		/// </summary>
		public float Cooldown;

		/// <summary>
		///   The amount of energy to deplete per shot or per second when the weapon is firing.
		/// </summary>
		public byte DepleteSpeed;

		/// <summary>
		///   The type of the weapon.
		/// </summary>
		public WeaponType WeaponType;

		/// <summary>
		///   The range of a range-based weapon.
		/// </summary>
		public float Range;

		/// <summary>
		///   The maximum allowed energy level of the weapon.
		/// </summary>
		public byte MaxEnergy;

		/// <summary>
		///   Gets a value indicating whether the weapon fires continuously.
		/// </summary>
		public bool FiresContinuously => Cooldown < 0;
	}
}