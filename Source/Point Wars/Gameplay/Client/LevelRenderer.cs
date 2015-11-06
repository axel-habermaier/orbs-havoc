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
	using Assets;
	using Platform.Graphics;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Renders the static geometry of a level.
	/// </summary>
	public class LevelRenderer
	{
		private const int BlockSize = 64;
		private const int TexSize = 64;
		private readonly Quad[] _quads;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="level">The level that should be rendered.</param>
		public LevelRenderer(Level level)
		{
			Assert.ArgumentNotNull(level, nameof(level));

			var texWidth = AssetBundle.LevelBorders.Width;
			var texHeight = AssetBundle.LevelBorders.Height;

			var offsetX = MathUtils.Round(level.Width / 2 * BlockSize);
			var offsetY = MathUtils.Round(level.Height / 2 * BlockSize);
			var quads = new List<Quad>(10000);

			var horizontalTexCoords = new Rectangle(TexSize * 2 / texWidth, TexSize / texHeight, TexSize / texWidth, TexSize / texHeight);
			var verticalTexCoords = new Rectangle(TexSize * 2 / texWidth, 0 / texHeight, TexSize / texWidth, TexSize / texHeight);
			var leftTopWallTexCoords = new Rectangle(0 / texWidth, 0 / texHeight, TexSize / texWidth, TexSize / texHeight);
			var rightTopWallTexCoords = new Rectangle(TexSize / texWidth, 0 / texHeight, TexSize / texWidth, TexSize / texHeight);
			var leftBottomWallTexCoords = new Rectangle(0 / texWidth, TexSize / texHeight, TexSize / texWidth, TexSize / texHeight);
			var rightBottomWallTexCoords = new Rectangle(TexSize / texWidth, TexSize / texHeight, TexSize / texWidth, TexSize / texHeight);

			for (var x = 0; x < level.Width; ++x)
			{
				for (var y = 0; y < level.Height; ++y)
				{
					Rectangle texCoords;
					switch (level.Blocks[x][y])
					{
						case BlockType.Empty:
						case BlockType.Wall:
							continue;
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
							Assert.NotReached("Unknown block type.");
							texCoords = Rectangle.Empty;
							break;
					}

					var rectangle = new Rectangle(x * BlockSize - offsetX, y * BlockSize - offsetY, BlockSize, BlockSize);
					quads.Add(new Quad(rectangle, Colors.White, texCoords));
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
		}
	}
}