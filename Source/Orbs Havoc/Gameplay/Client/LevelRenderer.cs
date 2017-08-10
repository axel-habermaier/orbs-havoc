namespace OrbsHavoc.Gameplay.Client
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
	internal class LevelRenderer
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
			var quads = new List<Quad>(1000);

			var curvedWallTexCoords = new Rectangle(0 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var inverseCurvedTopWallTexCoords = new Rectangle(texSize / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);
			var straightWallTexCoords = new Rectangle(texSize * 2 / texWidth, 0, texSize / texWidth, texSize / texHeight);
			var wallTexCoords = new Rectangle(texSize * 3 / texWidth, 0 / texHeight, texSize / texWidth, texSize / texHeight);

			for (var x = 0; x < level.Width; ++x)
			{
				for (var y = 0; y < level.Height; ++y)
				{
					Rectangle texCoords;
					float rotation;

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

					var quad = new Quad
					{
						Color = new Color(0xFF130C49),
						Position = level.GetBlockArea(x, y).Center,
						Orientation = MathUtils.DegToRad(rotation),
						Size = new Size(Level.BlockSize, Level.BlockSize),
						TextureCoordinates = texCoords
					};

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

			for (var y = 1; y < _level.Height * 2 - 2; ++y)
			{
				var position = _level.PositionOffset + new Vector2(Level.BlockSize / 2, (y + 1) * Level.BlockSize / 2);
				var width = y % 2 == 1 ? 3 : 1;
				spriteBatch.DrawLine(position, position + new Vector2((_level.Width - 1) * Level.BlockSize, 0), new Color(0xFF130C49), width);
			}

			for (var x = 1; x < _level.Width * 2 - 2; ++x)
			{
				var position = _level.PositionOffset + new Vector2((x + 1) * Level.BlockSize / 2, Level.BlockSize / 2);
				var width = x % 2 == 1 ? 3 : 1;
				spriteBatch.DrawLine(position, position + new Vector2(0, (_level.Height - 1) * Level.BlockSize), new Color(0xFF130C49), width);
			}

			spriteBatch.RenderState.Layer += 1;
			spriteBatch.RenderState.SamplerState = SamplerState.Point;
			spriteBatch.RenderState.BlendOperation = BlendOperation.Premultiplied;
			spriteBatch.Draw(_quads, _quads.Length, AssetBundle.LevelBorders);
			spriteBatch.RenderState.SamplerState = SamplerState.Bilinear;
		}
	}
}