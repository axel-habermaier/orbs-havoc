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

namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	internal static class Weapons
	{
		public static WeaponTemplate MiniGun = new WeaponTemplate
		{
			Cooldown = 0.2f,
			DepleteSpeed = 0,
			Speed = 1300,
			WeaponType = EntityType.MiniGun,
			MaxEnergy = 1,
			Damage = 2,
			MinSpread = 0.01f,
			MaxSpread = 0.07f
		};

		public static WeaponTemplate RocketLauncher = new WeaponTemplate
		{
			Cooldown = 0.8f,
			DepleteSpeed = 1,
			Speed = 500,
			WeaponType = EntityType.RocketLauncher,
			MaxEnergy = 20,
			Damage = 45,
			Range = 200,
			MinSpread = 0,
			MaxSpread = 0
		};

		public static WeaponTemplate LightingGun = new WeaponTemplate
		{
			Cooldown = -1,
			DepleteSpeed = 50,
			Range = 2000,
			WeaponType = EntityType.LightingGun,
			MaxEnergy = 0
		};

		public static readonly WeaponTemplate[] WeaponTemplates =
		{
			MiniGun,
			LightingGun, // TODO: Plasma
			LightingGun,
			RocketLauncher,
			LightingGun, // TODO: BFG
			LightingGun, // TODO: Bomb
			LightingGun, // TODO: Mine
			LightingGun, // TODO: ShockWave
		};

		internal struct WeaponTemplate
		{
			public float Speed;
			public float Cooldown;
			public byte DepleteSpeed;
			public EntityType WeaponType;
			public float Range;
			public byte MaxEnergy;
			public bool FiresContinuously => Cooldown < 0;
			public float Damage;
			public float MinSpread;
			public float MaxSpread;
		}
	}
}