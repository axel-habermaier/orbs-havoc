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
	using Platform.Memory;

	/// <summary>
	///   Represents a level within which a game session takes place.
	/// </summary>
	public class Level : DisposableObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private Level()
		{
		}

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
		public BlockType[][] Blocks { get; private set; }

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

			Blocks = new BlockType[Width][];

			for (var x = 0; x < Width; ++x)
			{
				Blocks[x] = new BlockType[Height];
				for (var y = 0; y < Height; ++y)
					Blocks[x][y] = (BlockType)buffer.ReadByte();
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			// Nothing to do here; however, the assets bundle generator expects all assets to be disposable
		}
	}
}