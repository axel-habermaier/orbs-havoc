namespace OrbsHavoc.UserInterface
{
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using static Platform.SDL2;

	/// <summary>
	///   Represents the mouse cursor.
	/// </summary>
	public unsafe class Cursor : DisposableObject
	{
		private Color _color = Colors.White;
		private void* _cursor;
		private byte[] _data;
		private int _height;
		private int _width;
		private int _x;
		private int _y;

		/// <summary>
		///   Loads the cursor from the given reader.
		/// </summary>
		/// <param name="buffer">The buffer the cursor should be created from.</param>
		public void Load(ref BufferReader buffer)
		{
			_x = buffer.ReadInt32();
			_y = buffer.ReadInt32();
			_width = buffer.ReadInt32();
			_height = buffer.ReadInt32();
			_data = buffer.ReadByteArray();

			ChangeColor(_color);
		}

		/// <summary>
		///   Initializes the cursor.
		/// </summary>
		private void Initialize(byte* data)
		{
			SDL_FreeCursor(_cursor);

			var sdlSurface = SDL_CreateRGBSurfaceFrom(data, _width, _height, 32, _width * 4, 0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);
			if (sdlSurface == null)
				Log.Die($"Failed to create surface for hardware cursor: {SDL_GetError()}");

			_cursor = SDL_CreateColorCursor(sdlSurface, _x, _y);
			if (_cursor == null)
				Log.Die($"Failed to create hardware cursor: {SDL_GetError()}");

			SDL_FreeSurface(sdlSurface);
		}

		/// <summary>
		///   Changes the cursor's color.
		/// </summary>
		/// <param name="color">The new cursor color.</param>
		public void ChangeColor(Color color)
		{
			_color = color;

			var data = stackalloc byte[_data.Length];
			for (var i = 0; i < _data.Length; i += 4)
			{
				data[i + 0] = (byte)(_data[i + 0] * color.Red / 255.0f);
				data[i + 1] = (byte)(_data[i + 1] * color.Green / 255.0f);
				data[i + 2] = (byte)(_data[i + 2] * color.Blue / 255.0f);
				data[i + 3] = _data[i + 3];
			}

			Initialize(data);
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