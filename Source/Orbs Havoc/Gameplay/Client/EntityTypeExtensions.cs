namespace OrbsHavoc.Gameplay.Client
{
	using Assets;
	using Platform.Graphics;
	using Rendering;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Provides extension methods for the entity type enumeration.
	/// </summary>
	internal static class EntityTypeExtensions
	{
		/// <summary>
		///   Gets the color for the given collectible.
		/// </summary>
		public static Color GetColor(this EntityType type)
		{
			Assert.ArgumentSatisfies(type.IsCollectible(), nameof(type), "Expected a collectible type.");

			switch (type)
			{
				case EntityType.Health:
				case EntityType.Regeneration:
					return new Color(0, 255, 0, 255);
				case EntityType.QuadDamage:
				case EntityType.MiniGun:
					return new Color(0xFF0083FF);
				case EntityType.Invisibility:
				case EntityType.RocketLauncher:
					return new Color(0xFF4800FF);
				case EntityType.LightingGun:
					return Colors.White;
				default:
					Assert.NotReached("Unexpected entity type.");
					return default;
			}
		}

		/// <summary>
		///   Gets the texture for the given collectible.
		/// </summary>
		public static Texture GetTexture(this EntityType type)
		{
			Assert.ArgumentSatisfies(type.IsCollectible(), nameof(type), "Expected a collectible type.");

			switch (type)
			{
				case EntityType.Health:
					return AssetBundle.Health;
				case EntityType.MiniGun:
					return AssetBundle.MiniGun;
				case EntityType.LightingGun:
					return AssetBundle.LightingGun;
				case EntityType.RocketLauncher:
					return AssetBundle.Rocket;
				case EntityType.Regeneration:
					return AssetBundle.Regeneration;
				case EntityType.QuadDamage:
					return AssetBundle.QuadDamage;
				case EntityType.Invisibility:
					return AssetBundle.Invisibility;
				default:
					Assert.NotReached("Unexpected entity type.");
					return null;
			}
		}
	}
}