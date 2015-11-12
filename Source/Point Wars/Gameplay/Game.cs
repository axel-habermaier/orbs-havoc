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
	using SceneNodes.Entities;

	/// <summary>
	///   Provides gameplay-specific constants.
	/// </summary>
	internal static class Game
	{
		public const int WeaponCount = 8;
		public const float HealthCollectibleHealthIncrease = 20;
		public const float ArmorRespawnDelay = 60;
		public const float ArmorDamageFactor = 0.5f;
		public const float ArmorTime = 30;
		public const float RegenerationRespawnDelay = 60;
		public const float RegenerationTime = 30;
		public const float RegenerationHealthIncrease = 10;
		public const float QuadDamageRespawnDelay = 60;
		public const float QuadDamageTime = 30;
		public const float QuadDamageMultiplier = 4;
		public const float SpeedRespawnDelay = 60;
		public const float SpeedTime = 30;
		public const float InvisibilityRespawnDelay = 60;
		public const float InvisibilityTime = 30;
		public const float HealthRespawnDelay = 10;
		public const float MaxAvatarHealth = 100;
		public const float MaxAvatarRegenerationHealth = 200;
		public const float MaxHealthLimitExceededDecrease = 5;
		public const float RespawnDelay = 2;

		public static WeaponTemplate MiniGunTemplate = new WeaponTemplate
		{
			Cooldown = 0.2f,
			DepleteSpeed = 0,
			Speed = 1300,
			WeaponType = EntityType.MiniGun,
			MaxEnergy = 1,
			Damage = 5
		};

		public static WeaponTemplate LightingGunTemplate = new WeaponTemplate
		{
			Cooldown = -1,
			DepleteSpeed = 50,
			Range = 2000,
			WeaponType = EntityType.LightingGun,
			MaxEnergy = 200
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
		}
	}
}