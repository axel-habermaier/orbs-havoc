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

namespace PointWars.Gameplay.Client
{
	using System.Collections.Generic;
	using System.Numerics;
	using Assets;
	using Platform.Graphics;
	using Rendering;
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

			var horizontalTexCoords = new Rectangle(texSize * 2 / texWidth, texSize / texHeight, texSize / texWidth, texSize / texHeight);
			var verticalTexCoords = new Rectangle(texSize * 2 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var leftTopWallTexCoords = new Rectangle(0 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var rightTopWallTexCoords = new Rectangle(texSize / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var leftBottomWallTexCoords = new Rectangle(0 / texWidth, texSize / texHeight, texSize / texWidth, texSize / texHeight);
			var rightBottomWallTexCoords = new Rectangle(texSize / texWidth, texSize / texHeight, texSize / texWidth, texSize / texHeight);

			for (var x = 0; x < level.Width; ++x)
			{
				for (var y = 0; y < level.Height; ++y)
				{
					Rectangle texCoords;
					switch (level[x, y])
					{
						case BlockType.VerticalWall:
							texCoords = verticalTexCoords;
							break;
						case BlockType.HorizontalWall:
							texCoords = horizontalTexCoords;
							break;
						case BlockType.LeftTopWall:
							texCoords = leftTopWallTexCoords;
							break;
						case BlockType.RightTopWall:
							texCoords = rightTopWallTexCoords;
							break;
						case BlockType.LeftBottomWall:
							texCoords = leftBottomWallTexCoords;
							break;
						case BlockType.RightBottomWall:
							texCoords = rightBottomWallTexCoords;
							break;
						default:
							continue;
					}

					quads.Add(new Quad(level.GetBlockArea(x, y), Colors.White, texCoords));
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

			for (var y = 0; y <= _level.Height; ++y)
			{
				var position = _level.PositionOffset + new Vector2(0, y * Level.BlockSize);
				spriteBatch.DrawLine(position, position + new Vector2(_level.Width * Level.BlockSize, 0), Colors.DarkRed, 1);
			}

			for (var x = 0; x <= _level.Width; ++x)
			{
				var position = _level.PositionOffset + new Vector2(x * Level.BlockSize, 0);
				spriteBatch.DrawLine(position, position + new Vector2(0, _level.Height * Level.BlockSize), Colors.DarkRed, 1);
			}
#endif
		}
	}
}