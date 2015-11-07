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

namespace PointWars.Gameplay
{
	using System.Numerics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents a level within which a game session takes place.
	/// </summary>
	public class Level : DisposableObject
	{
		/// <summary>
		///   The size of a block within the level.
		/// </summary>
		public const float BlockSize = 128;

		/// <summary>
		///   The thickness of the walls.
		/// </summary>
		public const float WallThickness = 20.0f;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private Level()
		{
		}

		/// <summary>
		///   Gets the level's position offset.
		/// </summary>
		public Vector2 PositionOffset { get; private set; }

		/// <summary>
		///   Gets the level's width in block units.
		/// </summary>
		public int Width { get; private set; }

		/// <summary>
		///   Gets the level's height in block units.
		/// </summary>
		public int Height { get; private set; }

		/// <summary>
		///   Gets the level's blocks.
		/// </summary>
		public BlockType[] Blocks { get; private set; }

		/// <summary>
		///   Gets the block type at the given location.
		/// </summary>
		/// <param name="x">The zero-based index in x-direction.</param>
		/// <param name="y">The zero-based index in y-direction.</param>
		public BlockType this[int x, int y]
		{
			get
			{
				Assert.InRange(x, 0, Width - 1);
				Assert.InRange(y, 0, Height - 1);

				return Blocks[x * Height + y];
			}
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="buffer">The buffer the level should be loaded from.</param>
		public static Level Create(ref BufferReader buffer)
		{
			var level = new Level();
			level.Load(ref buffer);
			return level;
		}

		/// <summary>
		///   Loads the level from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the level should be loaded from.</param>
		public void Load(ref BufferReader buffer)
		{
			Width = buffer.ReadInt16();
			Height = buffer.ReadInt16();

			PositionOffset = new Vector2(-MathUtils.Round(Width / 2 * BlockSize), -MathUtils.Round(Height / 2 * BlockSize));
			Blocks = new BlockType[Width * Height];

			for (var i = 0; i < Width * Height; ++i)
				Blocks[i] = (BlockType)buffer.ReadByte();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			// Nothing to do here; however, the assets bundle generator expects all assets to be disposable
		}

		/// <summary>
		///   Gets the blocks covered by the circle at the given position with the given radius.
		/// </summary>
		/// <param name="circle">The circle the covered blocks should be returned for.</param>
		/// <param name="xFirst">Gets the zero-based index in x-direction of the first covered block.</param>
		/// <param name="yFirst">Gets the zero-based index in y-direction of the first covered block.</param>
		/// <param name="xLast">Gets the zero-based index in x-direction of the last covered block.</param>
		/// <param name="yLast">Gets the zero-based index in y-direction of the last covered block.</param>
		public void GetCoveredBlocks(Circle circle, out int xFirst, out int yFirst, out int xLast, out int yLast)
		{
			GetBlock(circle.Position - new Vector2(circle.Radius, circle.Radius), out xFirst, out yFirst);
			GetBlock(circle.Position + new Vector2(circle.Radius, circle.Radius), out xLast, out yLast);
		}

		/// <summary>
		///   Gets the block the given position lies within.
		/// </summary>
		/// <param name="position">The position the block should be returned for.</param>
		/// <param name="x">Gets the zero-based index in x-direction of the block.</param>
		/// <param name="y">Gets the zero-based index in y-direction of the block.</param>
		public void GetBlock(Vector2 position, out int x, out int y)
		{
			position -= PositionOffset;

			x = (int)(position.X / BlockSize);
			y = (int)(position.Y / BlockSize);
		}

		/// <summary>
		///   Gets the area occupied by the block at the given location.
		/// </summary>
		/// <param name="x">The zero-based index of the block in x-direction.</param>
		/// <param name="y">The zero-based index of the block in y-direction.</param>
		public Rectangle GetBlockArea(int x, int y)
		{
			Assert.InRange(x, 0, Width - 1);
			Assert.InRange(y, 0, Height - 1);

			return new Rectangle(x * BlockSize + PositionOffset.X, y * BlockSize + PositionOffset.Y, BlockSize, BlockSize);
		}
	}
}