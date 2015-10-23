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

namespace PointWars.Platform.Graphics
{
	using Math;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a 2-dimensional texture.
	/// </summary>
	public sealed unsafe class Texture : GraphicsObject
	{
		/// <summary>
		///   Initializes a new instance that is initialized later.
		/// </summary>
		public Texture()
		{
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Texture(Size size, uint format, void* data)
		{
			Initialize(size, format, data);
		}

		/// <summary>
		///   Gets the size of the texture.
		/// </summary>
		public Size Size { get; private set; }

		/// <summary>
		///   Gets the width of the texture.
		/// </summary>
		public float Width => Size.Width;

		/// <summary>
		///   Gets the height of the texture.
		/// </summary>
		public float Height => Size.Height;

		/// <summary>
		///   Loads a texture from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the texture should be read from.</param>
		public static Texture Create(ref BufferReader buffer)
		{
			var texture = new Texture();
			texture.Load(ref buffer);
			return texture;
		}

		/// <summary>
		///   Loads the texture from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the texture should be read from.</param>
		public void Load(ref BufferReader buffer)
		{
			var size = new Size(buffer.ReadUInt32(), buffer.ReadUInt32());
			var sizeInBytes = buffer.ReadInt32();
			var data = buffer.Pointer;
			buffer.Skip(sizeInBytes);

			Initialize(size, GL_RGBA, data);
		}

		/// <summary>
		///   Initializes the texture.
		/// </summary>
		private void Initialize(Size size, uint format, void* data)
		{
			Assert.That(size.Width > 0 && size.Height > 0, "Invalid render target size.");

			Size = size;
			Deallocate(glDeleteTextures, Handle);
			Handle = Allocate(glGenTextures, nameof(Texture));

			glBindTexture(GL_TEXTURE_2D, Handle);
			glTexImage2D(GL_TEXTURE_2D, 0, (int)format, size.IntegralWidth, size.IntegralHeight, 0, format, GL_UNSIGNED_BYTE, data);
			CheckErrors();
		}

		/// <summary>
		///   Binds the texture for rendering.
		/// </summary>
		public void Bind()
		{
			Assert.That(Handle != 0, "The texture has not been initialized.");

			if (Change(ref State.Texture, this))
				glBindTexture(GL_TEXTURE_2D, Handle);

			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.Texture, this);
			Deallocate(glDeleteTextures, Handle);
		}
	}
}