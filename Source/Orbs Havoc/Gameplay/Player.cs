﻿namespace OrbsHavoc.Gameplay
{
	using Network;
	using Platform.Memory;
	using Rendering;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Represents a player that is participating in a game session.
	/// </summary>
	internal class Player : PooledObject
	{
		/// <summary>
		///   Gets or sets the name of the player.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///   Gets or sets the kind of the player.
		/// </summary>
		public PlayerKind Kind { get; set; }

		/// <summary>
		///   Gets or sets the player's ping.
		/// </summary>
		public int Ping { get; set; }

		/// <summary>
		///   Gets or sets the player's rank. At the end of a game session, the player with rank 1 wins.
		/// </summary>
		public int Rank { get; set; }

		/// <summary>
		///   Gets the remaining time until the player respawns.
		/// </summary>
		public float RemainingRespawnDelay { get; set; }

		/// <summary>
		///   Gets or sets the reason why the player left the game session.
		/// </summary>
		public LeaveReason LeaveReason { get; set; }

		/// <summary>
		///   Gets or sets the number of kills that the player has scored.
		/// </summary>
		public int Kills { get; set; }

		/// <summary>
		///   Gets or sets the number of deaths.
		/// </summary>
		public int Deaths { get; set; }

		/// <summary>
		///   Gets or sets the player's color.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		///   Gets a range of colors similar to the player's color.
		/// </summary>
		public Range<Color> ColorRange
		{
			get
			{
				var hsv = Color.ToHsv();
				const float delta = 0.025f;

				hsv.X += delta;
				var color1 = Color.FromHsv(hsv);

				hsv.X -= 2 * delta;
				var color2 = Color.FromHsv(hsv);

				return new Range<Color>(color1, color2);
			}
		}

		/// <summary>
		///   Gets or sets a value indicating whether this player is the server player.
		/// </summary>
		public bool IsServerPlayer => Identity == NetworkProtocol.ServerPlayerIdentity;

		/// <summary>
		///   Gets or sets the player's network identity.
		/// </summary>
		public NetworkIdentity Identity { get; set; }

		/// <summary>
		///   Gets or sets a value indicating whether this player is the local one.
		/// </summary>
		public bool IsLocalPlayer { get; set; }

		/// <summary>
		///   Gets or sets the orb controlled by the player.
		/// </summary>
		public Orb Orb { get; set; }

		/// <summary>
		///   Allocates a player using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the player.</param>
		/// <param name="name">The name of the player.</param>
		/// <param name="kind">The kind of the player.</param>
		/// <param name="identity">The network identity of the player.</param>
		public static Player Create(PoolAllocator allocator, string name, PlayerKind kind, NetworkIdentity identity = default)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			Assert.ArgumentInRange(kind, nameof(kind));

			var player = allocator.Allocate<Player>();
			player.Identity = identity;
			player.Name = name;
			player.Kind = kind;
			player.Kills = 0;
			player.Deaths = 0;
			player.Ping = 0;
			player.LeaveReason = LeaveReason.Unknown;
			player.IsLocalPlayer = false;
			player.RemainingRespawnDelay = 0;

			return player;
		}

		/// <summary>
		///   Returns a string that represents the current object.
		/// </summary>
		public override string ToString()
		{
			return $"'{Name}' ({Identity})";
		}

		/// <summary>
		///   Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected override void OnReturning()
		{
			Orb = null;
		}
	}
}