namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	internal static class PowerUps
	{
		internal static class Invisibility
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
		}

		internal static class Speed
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
		}

		internal static class Armor
		{
			public const float RespawnDelay = 60;
			public const float DamageFactor = 0.5f;
			public const float Time = 30;
		}

		internal static class Regeneration
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
			public const float HealthIncrease = 10;
		}

		internal static class QuadDamage
		{
			public const float RespawnDelay = 60;
			public const float Time = 30;
			public const float DamageMultiplier = 4;
		}
	}
}