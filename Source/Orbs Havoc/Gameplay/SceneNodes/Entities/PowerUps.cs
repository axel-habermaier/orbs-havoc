namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	public static class PowerUps
	{
		public static class Invisibility
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
		}

		public static class Speed
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
		}

		public static class Armor
		{
			public const float RespawnDelay = 60;
			public const float DamageFactor = 0.5f;
			public const float Time = 30;
		}

		public static class Regeneration
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
			public const float HealthIncrease = 10;
		}

		public static class QuadDamage
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
			public const float DamageMultiplier = 4;
		}
	}
}