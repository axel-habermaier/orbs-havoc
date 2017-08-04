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
			DepleteSpeed = 35,
			Range = 900,
			WeaponType = EntityType.LightingGun,
			MaxEnergy = 200,
			Damage = 50
		};

		public static readonly WeaponTemplate[] WeaponTemplates =
		{
			MiniGun,
			new WeaponTemplate(), // TODO: Plasma
			LightingGun,
			RocketLauncher,
			new WeaponTemplate(), // TODO: BFG
			new WeaponTemplate(), // TODO: Bomb
			new WeaponTemplate(), // TODO: Mine
			new WeaponTemplate(), // TODO: ShockWave
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