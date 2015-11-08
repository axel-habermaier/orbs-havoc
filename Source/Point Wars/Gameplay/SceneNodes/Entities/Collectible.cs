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
	using System.Numerics;
	using Assets;
	using Behaviors;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a collectible within a level.
	/// </summary>
	internal class Collectible : Entity
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the collectible belongs to.</param>
		/// <param name="position">The position of the collectible.</param>
		/// <param name="collectibleType">The type of the collectible.</param>
		public static Collectible Create(GameSession gameSession, Vector2 position, EntityType collectibleType)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentSatisfies(collectibleType.IsCollectible(), nameof(collectibleType), "Expected a collectible type.");

			var collectible = gameSession.Allocate<Collectible>();
			collectible.GameSession = gameSession;
			collectible.Position = position;
			collectible.Type = collectibleType;
			collectible.Player = gameSession.Players.ServerPlayer;

			gameSession.SceneGraph.Add(collectible);

			if (gameSession.ServerMode)
				collectible.AddBehavior(ColliderBehavior.Create(gameSession.Allocator, 16));
			else
				SpriteNode.Create(gameSession.Allocator, collectible, AssetBundle.Health, Colors.Green);

			return collectible;
		}
	}
}