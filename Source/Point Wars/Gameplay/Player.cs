﻿// The MIT License (MIT)
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
	using Entities;
	using Network;
	using Platform.Memory;
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
		///   Gets or sets the player's ping.
		/// </summary>
		public int Ping { get; set; }

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
		///   Gets or sets the entity controlled by the player.
		/// </summary>
		public Avatar Entity { get; set; }

		/// <summary>
		///   Allocates a player using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the player.</param>
		/// <param name="name">The name of the player.</param>
		/// <param name="identity">The network identity of the player.</param>
		public static Player Create(PoolAllocator allocator, string name, NetworkIdentity identity = default(NetworkIdentity))
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));

			var player = allocator.Allocate<Player>();
			player.Identity = identity;
			player.Name = name;
			player.Kills = 0;
			player.Deaths = 0;
			player.Ping = 0;
			player.LeaveReason = LeaveReason.Unknown;
			player.IsLocalPlayer = false;

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
			Entity = null;
		}
	}
}