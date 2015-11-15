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

namespace OrbsHavoc.Gameplay.Client
{
	using System;
	using Assets;
	using Platform.Graphics;
	using Rendering;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Provides extension methods for the entity type enumeration.
	/// </summary>
	public static class EntityTypeRendering
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
				default:
					throw new InvalidOperationException("Unexpected entity type.");
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
				case EntityType.RocketLauncher:
					return AssetBundle.Rocket;
				case EntityType.Regeneration:
					return AssetBundle.Regeneration;
				case EntityType.QuadDamage:
					return AssetBundle.QuadDamage;
				case EntityType.Invisibility:
					return AssetBundle.Invisibility;
				default:
					throw new InvalidOperationException("Unexpected entity type.");
			}
		}
	}
}