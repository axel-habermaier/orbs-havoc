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

namespace PointWars.Rendering
{
	using Math;
	using Platform.Graphics;
	using Utilities;

	/// <summary>
	///   Improves text drawing performance by caching the quads of a text.
	/// </summary>
	public struct TextRenderer
	{
		/// <summary>
		///   The text color.
		/// </summary>
		private Color _color;

		/// <summary>
		///   The number of cached quads.
		/// </summary>
		private int _numQuads;

		/// <summary>
		///   The quads of the text.
		/// </summary>
		private Quad[] _quads;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="color">The text color.</param>
		public TextRenderer(Color color)
			: this()
		{
			_color = color;
		}

		/// <summary>
		///   Gets or sets the text color.
		/// </summary>
		public Color Color
		{
			get { return _color; }
			set
			{
				_color = value;

				for (var i = 0; _quads != null && i < _numQuads; ++i)
					_quads[i].ChangeColor(value);
			}
		}

		/// <summary>
		///   Rebuilds the cache of the layouted text quads for more efficient rendering.
		/// </summary>
		/// <param name="font">
		///   The font that was used to layout the text and that should be used
		///   to draw the text.
		/// </param>
		/// <param name="text">The text that was layouted and should be drawn.</param>
		/// <param name="layoutData">The layouting data for the individual characters of the text.</param>
		internal void RebuildCache(Font font, TextString text, Rectangle[] layoutData)
		{
			Assert.ArgumentNotNull(font, nameof(font));
			Assert.ArgumentNotNull(layoutData, nameof(layoutData));
			Assert.That(text.Length <= layoutData.Length, "Layout data missing.");

			_numQuads = 0;
			if (text.IsWhitespaceOnly)
				return;

			// Ensure that the quads list does not have to be resized by settings its capacity to the number of
			// characters; however, this wastes some space as not all characters generate quads
			if (_quads == null || text.Length > _quads.Length)
				_quads = new Quad[text.Length];

			for (var i = 0; i < text.Length; ++i)
			{
				Quad quad;
				if (font.CreateGlyphQuad(text, i, ref layoutData[i], _color, out quad))
					_quads[_numQuads++] = quad;
			}
		}

		/// <summary>
		///   Draws the cached text.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the text.</param>
		/// <param name="texture">The font texture.</param>
		internal void DrawCached(SpriteBatch spriteBatch, Texture texture)
		{
			Assert.ArgumentNotNull(texture, nameof(texture));

			if (_numQuads != 0)
				spriteBatch.Draw(_quads, _numQuads, texture);
		}
	}
}