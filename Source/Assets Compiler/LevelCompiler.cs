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

namespace AssetsCompiler
{
	using System;
	using System.Drawing;
	using System.IO;
	using CommandLine;
	using PointWars.Gameplay.SceneNodes.Entities;

	public class LevelCompiler : CompilationTask
	{
		[Option("input", Required = true, HelpText = "The path to the input level file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output level file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			using (var bitmap = (Bitmap)Image.FromFile(InFile))
			{
				writer.Write((short)bitmap.Width);
				writer.Write((short)bitmap.Height);

				var originalBlocks = new EntityType[bitmap.Width][];
				var lineBlocks = new EntityType[bitmap.Width][];
				var edgeBlocks = new EntityType[bitmap.Width][];

				// Get the basic blocks from the level texture
				for (var x = 0; x < bitmap.Width; ++x)
				{
					originalBlocks[x] = new EntityType[bitmap.Height];
					lineBlocks[x] = new EntityType[bitmap.Height];
					edgeBlocks[x] = new EntityType[bitmap.Height];

					for (var y = 0; y < bitmap.Height; ++y)
					{
						var color = bitmap.GetPixel(x, y);
						var block = MapColor(color.ToArgb());

						if (block == null)
							throw new InvalidOperationException($"'{InFile}': {color} at {x}x{y} is invalid.");

						originalBlocks[x][y] = block.Value;
						lineBlocks[x][y] = block.Value;
						edgeBlocks[x][y] = block.Value;
					}
				}

				// Identify straight lines
				for (var x = 0; x < bitmap.Width; ++x)
				{
					for (var y = 0; y < bitmap.Height; ++y)
					{
						if (originalBlocks[x][y] != EntityType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = x > 0 && originalBlocks[x - 1][y] == EntityType.Wall;
						var right = x < bitmap.Width - 1 && originalBlocks[x + 1][y] == EntityType.Wall;
						var top = y > 0 && originalBlocks[x][y - 1] == EntityType.Wall;
						var bottom = y < bitmap.Height - 1 && originalBlocks[x][y + 1] == EntityType.Wall;

						if (!right && left)
							lineBlocks[x][y] = EntityType.LeftWall;
						else if (right && !left)
							lineBlocks[x][y] = EntityType.RightWall;
						else if (!top && bottom)
							lineBlocks[x][y] = EntityType.BottomWall;
						else if (top && !bottom)
							lineBlocks[x][y] = EntityType.TopWall;

						edgeBlocks[x][y] = lineBlocks[x][y];
					}
				}

				// Identify edges
				for (var x = 0; x < bitmap.Width; ++x)
				{
					for (var y = 0; y < bitmap.Height; ++y)
					{
						if (originalBlocks[x][y] != EntityType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = x > 0 ? lineBlocks[x - 1][y] : EntityType.None;
						var right = x < bitmap.Width - 1 ? lineBlocks[x + 1][y] : EntityType.None;
						var top = y > 0 ? lineBlocks[x][y - 1] : EntityType.None;
						var bottom = y < bitmap.Height - 1 ? lineBlocks[x][y + 1] : EntityType.None;
						var topLeft = x > 0 && y > 0 ? lineBlocks[x - 1][y - 1] : EntityType.None;
						var topRight = x < bitmap.Width - 1 && y > 0 ? lineBlocks[x + 1][y - 1] : EntityType.None;
						var bottomLeft = x > 0 && y < bitmap.Height - 1 ? lineBlocks[x - 1][y + 1] : EntityType.None;
						var bottomRight = x < bitmap.Width - 1 && y < bitmap.Height - 1 ? lineBlocks[x + 1][y + 1] : EntityType.None;

						if (right.IsWall() && bottom.IsWall() && !left.IsWall() && !top.IsWall())
							edgeBlocks[x][y] = EntityType.LeftTopWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !bottomRight.IsWall())
							edgeBlocks[x][y] = EntityType.InverseLeftTopWall;
						else if (left.IsWall() && bottom.IsWall() && !right.IsWall() && !top.IsWall())
							edgeBlocks[x][y] = EntityType.RightTopWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !bottomLeft.IsWall())
							edgeBlocks[x][y] = EntityType.InverseRightTopWall;
						else if (right.IsWall() && top.IsWall() && !left.IsWall() && !bottom.IsWall())
							edgeBlocks[x][y] = EntityType.LeftBottomWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !topRight.IsWall())
							edgeBlocks[x][y] = EntityType.InverseLeftBottomWall;
						else if (left.IsWall() && top.IsWall() && !right.IsWall() && !bottom.IsWall())
							edgeBlocks[x][y] = EntityType.RightBottomWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !topLeft.IsWall())
							edgeBlocks[x][y] = EntityType.InverseRightBottomWall;
					}
				}

				// Write the blocks to the compiled asset file
				for (var x = 0; x < bitmap.Width; ++x)
				{
					for (var y = 0; y < bitmap.Height; ++y)
						writer.Write((byte)edgeBlocks[x][y]);
				}
			}
		}

		private static EntityType? MapColor(int color)
		{
			unchecked
			{
				if (color == Color.White.ToArgb())
					return EntityType.None;

				if (color == Color.Black.ToArgb())
					return EntityType.Wall;

				if (color == Color.Gray.ToArgb())
					return EntityType.PlayerStart;

				if (color == (int)0xFF00FF00)
					return EntityType.Health;

				if (color == (int)0xFF0094FF)
					return EntityType.QuadDamage;

				if (color == (int)0xFF007F0E)
					return EntityType.Regeneration;

				if (color == (int)0xFF4800FF)
					return EntityType.Invisibility;

				return null;
			}
		}
	}
}