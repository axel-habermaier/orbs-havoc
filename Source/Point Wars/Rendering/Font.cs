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

namespace PointWars.Rendering
{
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Provides the metadata and operations for drawing text.
	/// </summary>
	public sealed class Font : DisposableObject
	{
		/// <summary>
		///   The glyphs of the font.
		/// </summary>
		private Glyph[] _glyphs;

		/// <summary>
		///   Stores the kerning pairs in a space-efficient way.
		/// </summary>
		private KerningPair[] _kernings;

		/// <summary>
		///   Gets a value indicating whether the font supports kerning.
		/// </summary>
		public bool KerningSupported => _kernings != null;

		/// <summary>
		///   Gets the line height.
		/// </summary>
		public int LineHeight { get; private set; }

		/// <summary>
		///   Gets the font texture.
		/// </summary>
		internal Texture Texture { get; } = new Texture();

		/// <summary>
		///   Loads a font from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the font should be read from.</param>
		public static Font Create(ref BufferReader buffer)
		{
			var font = new Font();
			font.Load(ref buffer);
			return font;
		}

		/// <summary>
		///   Returns the width of the text.
		/// </summary>
		/// <param name="textString">The text whose width should be computed.</param>
		public int MeasureWidth(string textString)
		{
			Assert.ArgumentNotNull(textString, nameof(textString));

			using (var text = TextString.Create(textString))
				return MeasureWidth(text, 0, text.Length);
		}

		/// <summary>
		///   Returns the width of the text.
		/// </summary>
		/// <param name="text">The text whose width should be computed.</param>
		public int MeasureWidth(TextString text)
		{
			return MeasureWidth(text, 0, text.Length);
		}

		/// <summary>
		///   Returns the width of the text from text[start] to text[end - 1].
		/// </summary>
		/// <param name="textString">The text whose width should be computed.</param>
		/// <param name="start">The index of the first character.</param>
		/// <param name="end">The index of the first character that is not measured.</param>
		public int MeasureWidth(string textString, int start, int end)
		{
			Assert.ArgumentNotNull(textString, nameof(textString));

			using (var text = TextString.Create(textString))
				return MeasureWidth(text, start, end);
		}

		/// <summary>
		///   Returns the width of the text from text[start] to text[end - 1].
		/// </summary>
		/// <param name="text">The text whose width should be computed.</param>
		/// <param name="start">The index of the first character.</param>
		/// <param name="end">The index of the first character that is not measured.</param>
		public int MeasureWidth(TextString text, int start, int end)
		{
			Assert.ArgumentSatisfies(start >= 0, nameof(start), "Out of bounds.");
			Assert.ArgumentSatisfies(end <= text.Length, nameof(end), "Out of bounds.");
			Assert.That(start <= end, "Start must be less than or equal to end.");

			var width = 0;

			for (var i = start; i < end; ++i)
			{
				width += GetGlyph(text[i]).AdvanceX;

				if (KerningSupported && i > start)
					width += GetKerningOffset(text[i - 1], text[i]);
			}

			return width;
		}

		/// <summary>
		///   Gets the kerning offset for the character pair.
		/// </summary>
		/// <param name="first">The first character.</param>
		/// <param name="second">The second character.</param>
		private int GetKerningOffset(char first, char second)
		{
			var firstGlyph = GetGlyph(first);
			var end = firstGlyph.KerningStart + firstGlyph.KerningCount;

			for (var i = firstGlyph.KerningStart; i < end; ++i)
			{
				if (_kernings[i].SecondGlyph == second)
					return _kernings[i].Offset;
			}

			return 0;
		}

		/// <summary>
		///   Gets the area of the glyph that corresponds to the character at the given index of
		///   the text. The start index is used to decide whether a kerning offset has to be
		///   applied to the glyph position.
		/// </summary>
		/// <param name="text">The text for which the glyph area should be computed.</param>
		/// <param name="start">The first index of the text that is considered; required for font kerning.</param>
		/// <param name="index">The index of the character for which the glyph area should be returned.</param>
		/// <param name="offset">
		///   The offset that should be applied to the glyph's position. The X-value of
		///   the offset is updated to reflect the glyph width and the kerning offset.
		/// </param>
		internal Rectangle GetGlyphArea(TextString text, int start, int index, ref Vector2 offset)
		{
			Assert.ArgumentSatisfies(start >= 0, nameof(start), "Out of bounds.");
			Assert.ArgumentSatisfies(start < text.Length, nameof(start), "Out of bounds.");
			Assert.ArgumentSatisfies(index >= 0, nameof(index), "Out of bounds.");
			Assert.ArgumentSatisfies(index < text.Length, nameof(index), "Out of bounds.");
			Assert.That(start <= index, "Start must be less than or equal to index.");

			var glyph = GetGlyph(text[index]);
			var area = glyph.Area.Offset(offset);
			offset.X += glyph.AdvanceX;

			if (start == index || !KerningSupported)
				return area;

			var kerningOffset = GetKerningOffset(text[index - 1], text[index]);
			offset.X += kerningOffset;
			return area.Offset(kerningOffset, 0);
		}

		/// <summary>
		///   Creates a quad for the character at the given index. Not all characters are visible, so
		///   in some cases, no quad is created for a given index. The function returns true to indicate
		///   that a valid quad was created; false, otherwise.
		/// </summary>
		/// <param name="text">The text containing the character that the quad should be created for.</param>
		/// <param name="index">The index of the character within the text that the quad should be created for.</param>
		/// <param name="area">The area of the glyph.</param>
		/// <param name="color">The color of the character.</param>
		/// <param name="quad">Returns the created quad.</param>
		internal bool CreateGlyphQuad(TextString text, int index, ref Rectangle area, Color color, out Quad quad)
		{
			Assert.ArgumentSatisfies(index < text.Length, nameof(index), "Out of bounds.");

			// Spaces and new lines are invisible, so don't bother drawing them
			var character = text[index];
			if (character == ' ' || character == '\n')
			{
				quad = new Quad();
				return false;
			}

			Color? textColor;
			text.GetColor(index, out textColor);

			if (textColor.HasValue)
				color = textColor.Value;

			var glyph = GetGlyph(character);
			quad = new Quad(new Rectangle(area.Left, area.Top, area.Width, area.Height), color, glyph.TextureArea);

			return true;
		}

		/// <summary>
		///   Gets the glyph instance for the given glyph character.
		/// </summary>
		/// <param name="glyph">The glyph character.</param>
		private Glyph GetGlyph(char glyph)
		{
			var index = (int)glyph;

			// Print the invalid character for all characters that are not supported by the font; the font processor guarantees 
			// that the glyph for the invalid character is at index 0
			if (index < 0 || index >= _glyphs.Length || _glyphs[index].IsInvalid)
				return _glyphs[0];

			// New lines are not be visible at all, and have no width
			if (glyph == '\n')
				return new Glyph();

			return _glyphs[index];
		}

		/// <summary>
		///   Loads the font from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the font should be read from.</param>
		public void Load(ref BufferReader buffer)
		{
			// Load the font map
			Texture.Load(ref buffer);

			// Load the font metadata
			LineHeight = buffer.ReadUInt16();

			var numGlyphs = buffer.ReadUInt16();
			var kerningCount = buffer.ReadUInt32();

			// Load the glyph metadata
			const int maxGlyphs = 256;
			Assert.That(numGlyphs < maxGlyphs, "Font has too many glyphs.");

			_glyphs = new Glyph[256];

			for (var i = 0; i < numGlyphs; ++i)
			{
				var index = buffer.ReadByte();

				// The invalid character is the first glyph
				if (i == 0)
					index = 0;

				// Read the texture coordinates
				var x = buffer.ReadUInt16();
				var y = buffer.ReadUInt16();
				_glyphs[index].Area.Width = buffer.ReadUInt16();
				_glyphs[index].Area.Height = buffer.ReadUInt16();

				// Compute the texture coordinates
				var textureLeft = x / Texture.Width;
				var textureRight = (x + _glyphs[index].Area.Width) / Texture.Width;
				var textureBottom = (y + _glyphs[index].Area.Height) / Texture.Height;
				var textureTop = y / Texture.Height;

				_glyphs[index].TextureArea = new Rectangle(textureLeft, textureTop,
					textureRight - textureLeft,
					textureBottom - textureTop);

				// Read the glyph offsets
				_glyphs[index].Area.Left = buffer.ReadInt16();
				_glyphs[index].Area.Top = buffer.ReadInt16();
				_glyphs[index].AdvanceX = buffer.ReadInt16();
			}

			// Load the kerning data
			if (kerningCount == 0)
				return;

			_kernings = new KerningPair[kerningCount];

			for (var i = 0; i < kerningCount; ++i)
			{
				var first = buffer.ReadUInt16();
				var second = buffer.ReadUInt16();
				var offset = buffer.ReadInt16();

				_kernings[i] = new KerningPair((char)first, (char)second, offset);

				if (_glyphs[first].KerningStart == 0)
					_glyphs[first].KerningStart = i;

				++_glyphs[first].KerningCount;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Texture.SafeDispose();
		}

		/// <summary>
		///   Provides the metadata for a glyph used by a font.
		/// </summary>
		internal struct Glyph
		{
			/// <summary>
			///   The number of pixels to advance the drawing cursor after drawing this glyph.
			/// </summary>
			public short AdvanceX;

			/// <summary>
			///   The area of the glyph, relative to the current drawing cursor.
			/// </summary>
			public Rectangle Area;

			/// <summary>
			///   The amount of kerning pairs with this glyph as the first partner.
			/// </summary>
			public int KerningCount;

			/// <summary>
			///   The index of the first kerning pair with this glyph as the first partner.
			/// </summary>
			public int KerningStart;

			/// <summary>
			///   The area of the font texture that contains the glyph's image data.
			/// </summary>
			public Rectangle TextureArea;

			/// <summary>
			///   Gets a value indicating whether the glyph is invalid, as not all fonts support all glyph.
			/// </summary>
			public bool IsInvalid => Area == Rectangle.Empty && TextureArea == Rectangle.Empty;
		}

		/// <summary>
		///   A kerning pair stores an position offset for two glyphs.
		/// </summary>
		internal struct KerningPair
		{
			/// <summary>
			///   The kerning offset.
			/// </summary>
			public readonly short Offset;

			/// <summary>
			///   The second glyph of the kerning pair.
			/// </summary>
			public readonly char SecondGlyph;

			/// <summary>
			///   The first glyph of the kerning pair.
			/// </summary>
			public char FirstGlyph;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="firstGlyph">The first glyph of the kerning pair.</param>
			/// <param name="secondGlyph">The second glyph of the kerning pair.</param>
			/// <param name="offset">The kerning offset.</param>
			public KerningPair(char firstGlyph, char secondGlyph, short offset)
			{
				FirstGlyph = firstGlyph;
				SecondGlyph = secondGlyph;
				Offset = offset;
			}
		}
	}
}