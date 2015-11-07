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
	using System.Collections.Generic;
	using Network;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Manages the active players that participate in a game session.
	/// </summary>
	internal sealed class PlayerCollection : DisposableObject
	{
		/// <summary>
		///   The list of active players.
		/// </summary>
		private readonly List<Player> _players = new List<Player>();

		/// <summary>
		///   Indicates whether players are managed in server mode.
		/// </summary>
		private readonly bool _serverMode;

		/// <summary>
		///   The allocator that is used to allocate network identities for the players.
		/// </summary>
		private NetworkIdentityAllocator _identityAllocator;

		/// <summary>
		///   Maps network identities to actual player objects.
		/// </summary>
		private NetworkIdentityMap<Player> _identityMap;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate game objects.</param>
		/// <param name="serverMode">Indicates whether players should be managed in server mode.</param>
		public PlayerCollection(PoolAllocator allocator, bool serverMode)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			_serverMode = serverMode;

			if (_serverMode)
			{
				_identityAllocator = new NetworkIdentityAllocator(NetworkProtocol.MaxPlayers + 1);

				var serverPlayer = Player.Create(allocator, "<server>", PlayerKind.Bot, NetworkProtocol.ServerPlayerIdentity);
				serverPlayer.IsLocalPlayer = true;

				Add(serverPlayer);
			}
			else
				_identityMap = new NetworkIdentityMap<Player>(NetworkProtocol.MaxPlayers + 1);
		}

		/// <summary>
		///   Gets the player representing the server.
		/// </summary>
		public Player ServerPlayer
		{
			get
			{
				foreach (var player in _players)
				{
					if (player.IsServerPlayer)
						return player;
				}

				return null;
			}
		}

		/// <summary>
		///   Gets or sets the local player.
		/// </summary>
		public Player LocalPlayer
		{
			get
			{
				foreach (var player in _players)
				{
					if (player.IsLocalPlayer)
						return player;
				}

				return null;
			}
		}

		/// <summary>
		///   Gets the number of active players, not counting the server player.
		/// </summary>
		public int Count => _players.Count - 1;

		/// <summary>
		///   Gets the player that corresponds to the given identity. Returns null if no player with the given identity could
		///   be found, or if the generation did not match.
		/// </summary>
		/// <param name="identity">The identity of the player that should be returned.</param>
		public Player this[NetworkIdentity identity] => _identityMap[identity];

		/// <summary>
		///   Gets an enumerator that can be used enumerate all active players.
		/// </summary>
		public List<Player>.Enumerator GetEnumerator()
		{
			return _players.GetEnumerator();
		}

		/// <summary>
		///   Adds the given player to the list.
		/// </summary>
		/// <param name="player">The player that should be added.</param>
		public void Add(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.That(!_players.Contains(player), "The player has already been added.");

			if (_serverMode)
				player.Identity = _identityAllocator.Allocate();
			else
				_identityMap.Add(player.Identity, player);

			_players.Add(player);
		}

		/// <summary>
		///   Removes the player from the list.
		/// </summary>
		/// <param name="player">The player that should be removed.</param>
		public void Remove(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentSatisfies(_players.Contains(player), nameof(player), "Cannot remove an unknown player.");

			_players.Remove(player);

			if (_serverMode)
				_identityAllocator.Free(player.Identity);
			else
				_identityMap.Remove(player.Identity);

			player.SafeDispose();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_players.SafeDisposeAll();
		}
	}
}