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

namespace PointWars.Gameplay.SceneNodes.Entities
{
	using Assets;
	using Behaviors;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a player avatar.
	/// </summary>
	internal class Avatar : Entity
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Avatar()
		{
			NetworkType = EntityType.Avatar;
		}

		/// <summary>
		///   Gets the avatar's player input behavior in server mode.
		/// </summary>
		public PlayerInputBehavior PlayerInput { get; private set; }

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the entity belongs to.</param>
		/// <param name="player">The player the ship belongs to.</param>
		public static Avatar Create(GameSession gameSession, Player player)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			var avatar = gameSession.Allocate<Avatar>();
			avatar.GameSession = gameSession;
			avatar.Player = player;
			
			gameSession.SceneGraph.Add(avatar);

			if (gameSession.ServerMode)
				avatar.AddBehavior(avatar.PlayerInput = PlayerInputBehavior.Create(gameSession.Allocator));
			else
			{
				var sprite = gameSession.Allocate<SpriteNode>();
				sprite.Texture = AssetBundle.Avatar;
				sprite.Color = Colors.YellowGreen;
				sprite.AttachTo(avatar);
			}

			return avatar;
		}
	}
}