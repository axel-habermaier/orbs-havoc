// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using System.IO;
	using System.Linq;
	using CommandLine;
	using JetBrains.Annotations;
	using OrbsHavoc.Gameplay.SceneNodes.Entities;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
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
			{
				var levelData = LoadLevelData();
				var width = levelData.GetLength(0);
				var height = levelData.GetLength(1);

				writer.Write((short)width);
				writer.Write((short)height);

				var lineBlocks = new EntityType[width, height];
				var edgeBlocks = new EntityType[width, height];

				// Get the basic blocks from the level texture
				for (var x = 0; x < width; ++x)
				{
					for (var y = 0; y < height; ++y)
					{
						lineBlocks[x, y] = levelData[x, y];
						edgeBlocks[x, y] = levelData[x, y];
					}
				}

				// Identify straight lines
				for (var x = 1; x < width - 1; ++x)
				{
					for (var y = 1; y < height - 1; ++y)
					{
						if (levelData[x, y] != EntityType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = levelData[x - 1, y] == EntityType.Wall;
						var right = levelData[x + 1, y] == EntityType.Wall;
						var top = levelData[x, y - 1] == EntityType.Wall;
						var bottom = levelData[x, y + 1] == EntityType.Wall;

						if (!right && left)
							lineBlocks[x, y] = EntityType.LeftWall;
						else if (right && !left)
							lineBlocks[x, y] = EntityType.RightWall;
						else if (!top && bottom)
							lineBlocks[x, y] = EntityType.BottomWall;
						else if (top && !bottom)
							lineBlocks[x, y] = EntityType.TopWall;

						edgeBlocks[x, y] = lineBlocks[x, y];
					}
				}

				// Identify edges
				for (var x = 1; x < width - 1; ++x)
				{
					for (var y = 1; y < height - 1; ++y)
					{
						if (levelData[x, y] != EntityType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = lineBlocks[x - 1, y];
						var right = lineBlocks[x + 1, y];
						var top = lineBlocks[x, y - 1];
						var bottom = lineBlocks[x, y + 1];
						var topLeft = lineBlocks[x - 1, y - 1];
						var topRight = lineBlocks[x + 1, y - 1];
						var bottomLeft = lineBlocks[x - 1, y + 1];
						var bottomRight = lineBlocks[x + 1, y + 1];

						if (right.IsWall() && bottom.IsWall() && !left.IsWall() && !top.IsWall())
							edgeBlocks[x, y] = EntityType.LeftTopWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !bottomRight.IsWall())
							edgeBlocks[x, y] = EntityType.InverseLeftTopWall;
						else if (left.IsWall() && bottom.IsWall() && !right.IsWall() && !top.IsWall())
							edgeBlocks[x, y] = EntityType.RightTopWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !bottomLeft.IsWall())
							edgeBlocks[x, y] = EntityType.InverseRightTopWall;
						else if (right.IsWall() && top.IsWall() && !left.IsWall() && !bottom.IsWall())
							edgeBlocks[x, y] = EntityType.LeftBottomWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !topRight.IsWall())
							edgeBlocks[x, y] = EntityType.InverseLeftBottomWall;
						else if (left.IsWall() && top.IsWall() && !right.IsWall() && !bottom.IsWall())
							edgeBlocks[x, y] = EntityType.RightBottomWall;
						else if (right.IsWall() && bottom.IsWall() && left.IsWall() && top.IsWall() && !topLeft.IsWall())
							edgeBlocks[x, y] = EntityType.InverseRightBottomWall;
					}
				}

				// Write the blocks to the compiled asset file
				for (var x = 0; x < width; ++x)
				{
					for (var y = 0; y < height; ++y)
						writer.Write((byte)edgeBlocks[x, y]);
				}
			}
		}

		private EntityType[,] LoadLevelData()
		{
			var content = File.ReadAllLines(InFile).Select(line => line.Trim()).Where(line => !line.StartsWith("//")).ToArray();
			var height = content.Length;
			var width = content.Max(line => (int)Math.Ceiling(line.Length / 2.0));
			var data = new EntityType[width, height];

			for (var x = 0; x < width; ++x)
			{
				for (var y = 0; y < height; ++y)
					data[x, y] = MapToType(x * 2, y, content[y][x * 2]);
			}

			return data;
		}

		private static EntityType MapToType(int x, int y, char type)
		{
			switch (type)
			{
				case '□':
					return EntityType.Wall;
				case 'S':
					return EntityType.PlayerStart;
				case 'H':
					return EntityType.Health;
				case 'R':
					return EntityType.Regeneration;
				case 'Q':
					return EntityType.QuadDamage;
				case 'I':
					return EntityType.Invisibility;
				case ' ':
					return EntityType.None;
				default:
					throw new InvalidOperationException($"Unknown entity type '{type}' at position {x}x{y}.");
			}
		}
	}
}