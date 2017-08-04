namespace OrbsHavoc.Gameplay.SceneNodes.Entities
{
	/// <summary>
	///   Identifies the type of an entity.
	/// </summary>
	public enum EntityType : byte
	{
		Unknown = 0,
		None = Unknown,
		Orb,
		Bullet,
		LightingBolt,
		Rocket,
		Armor,
		Regeneration,
		QuadDamage,
		Speed,
		Invisibility,
		MiniGun,
		PlasmaGun,
		LightingGun,
		RocketLauncher,
		Bfg,
		Bomb,
		Mine,
		ShockWave,
		Health,
		LeftWall,
		RightWall,
		TopWall,
		BottomWall,
		Wall,
		LeftTopWall,
		RightTopWall,
		LeftBottomWall,
		RightBottomWall,
		InverseLeftTopWall,
		InverseRightTopWall,
		InverseLeftBottomWall,
		InverseRightBottomWall,
		PlayerStart
	}

	/// <summary>
	///   Provides extension methods for the entity type enumeration.
	/// </summary>
	public static class EntityTypeExtensions
	{
		/// <summary>
		///   Gets the weapon slot corresponding to the given entity type.
		/// </summary>
		public static int GetWeaponSlot(this EntityType weapon)
		{
			return (int)weapon - (int)EntityType.MiniGun;
		}

		/// <summary>
		///   Gets the weapon slot corresponding to the given entity type.
		/// </summary>
		public static EntityType GetWeaponFromSlot(this int weaponSlot)
		{
			return (EntityType)(weaponSlot + (int)EntityType.MiniGun);
		}

		/// <summary>
		///   Indicates whether the given entity type represents a wall.
		/// </summary>
		public static bool IsWall(this EntityType type)
		{
			switch (type)
			{
				case EntityType.LeftWall:
				case EntityType.RightWall:
				case EntityType.TopWall:
				case EntityType.BottomWall:
				case EntityType.Wall:
				case EntityType.LeftTopWall:
				case EntityType.RightTopWall:
				case EntityType.LeftBottomWall:
				case EntityType.RightBottomWall:
				case EntityType.InverseLeftTopWall:
				case EntityType.InverseRightTopWall:
				case EntityType.InverseLeftBottomWall:
				case EntityType.InverseRightBottomWall:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		///   Indicates whether the given entity type represents an inverse curved wall.
		/// </summary>
		public static bool IsInverseCurvedWall(this EntityType type)
		{
			switch (type)
			{
				case EntityType.InverseLeftTopWall:
				case EntityType.InverseRightTopWall:
				case EntityType.InverseLeftBottomWall:
				case EntityType.InverseRightBottomWall:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		///   Indicates whether the given entity type represents a weapon.
		/// </summary>
		public static bool IsWeapon(this EntityType type)
		{
			switch (type)
			{
				case EntityType.MiniGun:
				case EntityType.PlasmaGun:
				case EntityType.LightingGun:
				case EntityType.RocketLauncher:
				case EntityType.Bfg:
				case EntityType.Bomb:
				case EntityType.Mine:
				case EntityType.ShockWave:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		///   Indicates whether the given entity type represents a power up.
		/// </summary>
		public static bool IsPowerUp(this EntityType type)
		{
			switch (type)
			{
				case EntityType.Armor:
				case EntityType.Regeneration:
				case EntityType.QuadDamage:
				case EntityType.Speed:
				case EntityType.Invisibility:
					return true;
				default:
					return false;
			}
		}

		/// <summary>
		///   Indicates whether the given entity type represents a collectible.
		/// </summary>
		public static bool IsCollectible(this EntityType type)
		{
			switch (type)
			{
				case EntityType.Armor:
				case EntityType.Regeneration:
				case EntityType.QuadDamage:
				case EntityType.Speed:
				case EntityType.Invisibility:
				case EntityType.Health:
				case EntityType.MiniGun:
				case EntityType.PlasmaGun:
				case EntityType.LightingGun:
				case EntityType.RocketLauncher:
				case EntityType.Bfg:
				case EntityType.Bomb:
				case EntityType.Mine:
				case EntityType.ShockWave:
					return true;
				default:
					return false;
			}
		}
	}
}