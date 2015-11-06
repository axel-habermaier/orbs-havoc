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

namespace AssetsCompiler
{
	using System;
	using System.Drawing;
	using System.IO;
	using CommandLine;
	using PointWars.Gameplay;

	public class LevelCompiler : IExecutable
	{
		[Option("input", Required = true, HelpText = "The path to the input level file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output level file.")]
		public string OutFile { get; set; }

		public void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			using (var bitmap = (Bitmap)Image.FromFile(InFile))
			{
				writer.Write((short)bitmap.Width);
				writer.Write((short)bitmap.Height);

				var originalBlocks = new BlockType[bitmap.Width][];
				var lineBlocks = new BlockType[bitmap.Width][];
				var edgeBlocks = new BlockType[bitmap.Width][];

				// Get the basic blocks from the level texture
				for (var x = 0; x < bitmap.Width; ++x)
				{
					originalBlocks[x] = new BlockType[bitmap.Height];
					lineBlocks[x] = new BlockType[bitmap.Height];
					edgeBlocks[x] = new BlockType[bitmap.Height];

					for (var y = 0; y < bitmap.Height; ++y)
					{
						var color = bitmap.GetPixel(x, y);
						var block = MapColor(color.ToArgb());

						if (block == null)
							throw new InvalidOperationException($"{color} at {x}x{y} is invalid.");

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
						if (originalBlocks[x][y] != BlockType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = x > 0 && originalBlocks[x - 1][y] == BlockType.Wall;
						var right = x < bitmap.Width - 1 && originalBlocks[x + 1][y] == BlockType.Wall;
						var top = y > 0 && originalBlocks[x][y - 1] == BlockType.Wall;
						var bottom = y < bitmap.Height - 1 && originalBlocks[x][y + 1] == BlockType.Wall;

						if (!top || !bottom)
							lineBlocks[x][y] = BlockType.HorizontalWall;
						else if (!left || !right)
							lineBlocks[x][y] = BlockType.VerticalWall;

						edgeBlocks[x][y] = lineBlocks[x][y];
					}
				}

				// Identify edges
				for (var x = 0; x < bitmap.Width; ++x)
				{
					for (var y = 0; y < bitmap.Height; ++y)
					{
						if (originalBlocks[x][y] != BlockType.Wall)
							continue;

						// The type of the wall depends on how the block is surrounded with walls
						var left = x > 0 ? lineBlocks[x - 1][y] : BlockType.Empty;
						var right = x < bitmap.Width - 1 ? lineBlocks[x + 1][y] : BlockType.Empty;
						var top = y > 0 ? lineBlocks[x][y - 1] : BlockType.Empty;
						var bottom = y < bitmap.Height - 1 ? lineBlocks[x][y + 1] : BlockType.Empty;

						if (right == BlockType.HorizontalWall && bottom == BlockType.VerticalWall)
							edgeBlocks[x][y] = BlockType.LeftTopWall;
						else if (right != BlockType.Empty && bottom != BlockType.Empty && left == BlockType.Empty && top == BlockType.Empty)
							edgeBlocks[x][y] = BlockType.LeftTopWall;
						else if (left == BlockType.HorizontalWall && bottom == BlockType.VerticalWall)
							edgeBlocks[x][y] = BlockType.RightTopWall;
						else if (right == BlockType.Empty && bottom != BlockType.Empty && left != BlockType.Empty && top == BlockType.Empty)
							edgeBlocks[x][y] = BlockType.RightTopWall;
						else if (right == BlockType.HorizontalWall && top == BlockType.VerticalWall)
							edgeBlocks[x][y] = BlockType.LeftBottomWall;
						else if (right != BlockType.Empty && bottom == BlockType.Empty && left == BlockType.Empty && top != BlockType.Empty)
							edgeBlocks[x][y] = BlockType.LeftBottomWall;
						else if (left == BlockType.HorizontalWall && top == BlockType.VerticalWall)
							edgeBlocks[x][y] = BlockType.RightBottomWall;
						else if (right == BlockType.Empty && bottom == BlockType.Empty && left != BlockType.Empty && top != BlockType.Empty)
							edgeBlocks[x][y] = BlockType.RightBottomWall;
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

		private static BlockType? MapColor(int color)
		{
			if (color == Color.White.ToArgb())
				return BlockType.Empty;

			if (color == Color.Black.ToArgb())
				return BlockType.Wall;

			return null;
		}
	}
}