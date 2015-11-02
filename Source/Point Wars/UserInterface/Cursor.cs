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

namespace PointWars.UserInterface
{
	using Platform.Logging;
	using Platform.Memory;
	using static Platform.SDL2;

	/// <summary>
	///   Represents the mouse cursor.
	/// </summary>
	public unsafe class Cursor : DisposableObject
	{
		/// <summary>
		///   The underlying hardware cursor instance.
		/// </summary>
		private void* _cursor;

		/// <summary>
		///   Initializes a cursor.
		/// </summary>
		/// <param name="buffer">The buffer the cursor should be created from.</param>
		public static Cursor Create(ref BufferReader buffer)
		{
			var cursor = new Cursor();
			cursor.Load(ref buffer);
			return cursor;
		}

		/// <summary>
		///   Loads the cursor from the given reader.
		/// </summary>
		/// <param name="buffer">The buffer the cursor should be created from.</param>
		public void Load(ref BufferReader buffer)
		{
			var x = buffer.ReadInt32();
			var y = buffer.ReadInt32();
			var width = buffer.ReadInt32();
			var height = buffer.ReadInt32();
			var length = buffer.ReadInt32();

			using (var data = buffer.Pointer)
			{
				buffer.Skip(length);

				var sdlSurface = SDL_CreateRGBSurfaceFrom(data, width, height, 32, width * 4,
					0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);

				if (sdlSurface == null)
					Log.Die("Failed to create surface for hardware cursor: {0}.", SDL_GetError());

				_cursor = SDL_CreateColorCursor(sdlSurface, x, y);
				if (_cursor == null)
					Log.Die("Failed to create hardware cursor: {0}", SDL_GetError());

				SDL_FreeSurface(sdlSurface);
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			SDL_FreeCursor(_cursor);
		}

		/// <summary>
		///   Draws the cursor.
		/// </summary>
		public void Draw()
		{
			SDL_SetCursor(_cursor);
		}
	}
}