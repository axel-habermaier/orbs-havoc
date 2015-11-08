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

namespace PointWars.Gameplay.Client
{
	using System.Collections.Generic;
	using System.Numerics;
	using Assets;
	using Platform.Graphics;
	using Rendering;
	using SceneNodes.Entities;
	using Utilities;

	/// <summary>
	///   Renders the static geometry of a level.
	/// </summary>
	public class LevelRenderer
	{
		private readonly Level _level;
		private readonly Quad[] _quads;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="level">The level that should be rendered.</param>
		public LevelRenderer(Level level)
		{
			Assert.ArgumentNotNull(level, nameof(level));

			_level = level;
			var texWidth = AssetBundle.LevelBorders.Width;
			var texHeight = AssetBundle.LevelBorders.Height;

			const int texSize = 128;
			var quads = new List<Quad>(10000);

			var curvedWallTexCoords = new Rectangle(0 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var inverseCurvedTopWallTexCoords = new Rectangle(texSize / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var straightWallTexCoords = new Rectangle(texSize * 2 / texWidth, 0, texSize / texWidth, texSize / texHeight);
			var wallTexCoords = new Rectangle(texSize * 3 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);

			for (var x = 0; x < level.Width; ++x)
			{
				for (var y = 0; y < level.Height; ++y)
				{
					Rectangle texCoords;
					float  rotation ;

					switch (level[x, y])
					{
						case EntityType.Wall:
							texCoords = wallTexCoords;
							rotation = 0;
							break;
						case EntityType.LeftWall:
							texCoords = straightWallTexCoords;
							rotation = 180;
							break;
						case EntityType.RightWall:
							texCoords = straightWallTexCoords;
							rotation = 0;
							break;
						case EntityType.TopWall:
							texCoords = straightWallTexCoords;
							rotation = 270;
							break;
						case EntityType.BottomWall:
							texCoords = straightWallTexCoords;
							rotation = 90;
							break;
						case EntityType.LeftTopWall:
							texCoords = curvedWallTexCoords;
							rotation = 0;
							break;
						case EntityType.RightTopWall:
							texCoords = curvedWallTexCoords;
							rotation = 90;
							break;
						case EntityType.LeftBottomWall:
							texCoords = curvedWallTexCoords;
							rotation = 270;
							break;
						case EntityType.RightBottomWall:
							texCoords = curvedWallTexCoords;
							rotation = 180;
							break;
						case EntityType.InverseLeftTopWall:
							texCoords = inverseCurvedTopWallTexCoords;
							rotation = 0;
							break;
						case EntityType.InverseRightTopWall:
							texCoords = inverseCurvedTopWallTexCoords;
							rotation = 90;
							break;
						case EntityType.InverseLeftBottomWall:
							texCoords = inverseCurvedTopWallTexCoords;
							rotation = 270;
							break;
						case EntityType.InverseRightBottomWall:
							texCoords = inverseCurvedTopWallTexCoords;
							rotation = 180;
							break;
						default:
							continue;
					}

					var area = new Rectangle(-Level.BlockSize / 2, -Level.BlockSize / 2, Level.BlockSize, Level.BlockSize);
					var quad = new Quad(area, new Color(0xFF130C49), texCoords);
					var matrix = Matrix3x2.CreateRotation(MathUtils.DegToRad(rotation)) * Matrix3x2.CreateTranslation(level.GetBlockArea(x, y).Center);

					Quad.Transform(ref quad, ref matrix);
					quads.Add(quad);
				}
			}

			_quads = quads.ToArray();
		}

		/// <summary>
		///   Draws the game session.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			spriteBatch.SamplerState = SamplerState.Point;
			spriteBatch.Draw(_quads, _quads.Length, AssetBundle.LevelBorders);
			spriteBatch.SamplerState = SamplerState.Bilinear;

#if true
			--spriteBatch.Layer;

			for (var y = 0; y < _level.Height * 2 - 1; ++y)
			{
				var position = _level.PositionOffset + new Vector2(Level.BlockSize / 2, (y + 1) * Level.BlockSize / 2);
				spriteBatch.DrawLine(position, position + new Vector2((_level.Width - 1) * Level.BlockSize, 0), new Color(0xFF130C49), 1);
			}

			for (var x = 0; x < _level.Width * 2 - 1; ++x)
			{
				var position = _level.PositionOffset + new Vector2((x + 1) * Level.BlockSize / 2, Level.BlockSize / 2);
				spriteBatch.DrawLine(position, position + new Vector2(0, (_level.Height - 1) * Level.BlockSize), new Color(0xFF130C49), 1);
			}
#endif
		}
	}
}