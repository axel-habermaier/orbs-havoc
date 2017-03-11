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
	using System.Collections.Generic;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;
	using System.Text.RegularExpressions;
	using CommandLine;
	using JetBrains.Annotations;
	using SharpFont;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class FontCompiler : CompilationTask
	{
		private const int Padding = 1;

		[Option("input", Required = true, HelpText = "The path to the input font file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output font file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			{
				var shader = File.ReadAllText(InFile);
				var match = Regex.Match(shader,
					@"File\s*=\s*(?<file>.*)\s*Size\s*=\s*(?<size>.*)\s*Aliased\s*=\s*(?<aliased>.*)\s*Characters\s*=\s*(?<range>.*)\s*InvalidChar\s*=\s*(?<invalid>.)",
					RegexOptions.Multiline);

				var file = Path.Combine(Path.GetDirectoryName(InFile), match.Groups["file"].Value.Trim());
				if (!File.Exists(file))
					throw new InvalidOperationException($"Failed to find font file '{file}'.");

				var size = UInt32.Parse(match.Groups["size"].Value.Trim());
				var aliased = Boolean.Parse(match.Groups["aliased"].Value.Trim());
				var characters = ParseCharacterRanges(match.Groups["range"].Value.Trim()).Distinct().ToArray();
				var invalidChar = match.Groups["invalid"].Value.Trim()[0];

				using (var freeType = new Library())
				using (var face = new Face(freeType, file))
				{
					face.SetPixelSizes(0, size);
					var lineHeight = (int)face.Size.Metrics.Height;
					var baseline = lineHeight + (int)face.Size.Metrics.Descender;

					// The first glyph must be the invalid character
					var glyphs = new[] { LoadGlyph(face, invalidChar, aliased) }
						.Concat(characters.Select(c => LoadGlyph(face, c, aliased)).OrderBy(glyph => glyph.Height))
						.ToArray();

					var kernings =
						(from left in glyphs
						 from right in glyphs
						 let offset = (int)face.GetKerning(left.Index, right.Index, KerningMode.Default).X
						 where offset != 0
						 select new { Left = left, Right = right, Offset = offset }).ToArray();

					var bitmapSize = Layout(glyphs);
					using (var bitmap = new Bitmap(bitmapSize.Width, bitmapSize.Height, PixelFormat.Format32bppArgb))
					{
						using (var graphics = Graphics.FromImage(bitmap))
						{
							foreach (var glyph in glyphs.Where(glyph => glyph.Bitmap != null))
								graphics.DrawImage(glyph.Bitmap, new Point(glyph.Area.Left, glyph.Area.Top));
						}

						for (var y = 0; y < bitmap.Height; y++)
						{
							for (var x = 0; x < bitmap.Width; x++)
							{
								var pixelColor = bitmap.GetPixel(x, y);
								var a = pixelColor.A;
								var r = (byte)(255 - pixelColor.R);
								var g = (byte)(255 - pixelColor.G);
								var b = (byte)(255 - pixelColor.B);
								bitmap.SetPixel(x, y, Color.FromArgb(a, r, g, b));
							}
						}

						TextureCompiler.Compile(writer, bitmap);
					}

					// Write font metadata
					writer.Write((ushort)lineHeight);
					writer.Write((ushort)glyphs.Length);
					writer.Write((uint)kernings.Length);

					// Write glyph metadata
					foreach (var glyph in glyphs)
					{
						writer.Write((byte)glyph.Character);

						// Write the font map texture coordinates in pixels
						writer.Write((ushort)glyph.Area.Left);
						writer.Write((ushort)glyph.Area.Top);
						writer.Write((ushort)glyph.Area.Width);
						writer.Write((ushort)glyph.Area.Height);

						// Write the glyph offsets
						writer.Write((short)glyph.OffsetX);
						writer.Write((short)(baseline - glyph.OffsetY));
						writer.Write((short)glyph.AdvanceX);
					}

					// Write kerning pairs
					foreach (var kerning in kernings)
					{
						writer.Write((ushort)kerning.Left.Character);
						writer.Write((ushort)kerning.Right.Character);
						writer.Write((short)kerning.Offset);
					}
				}
			}
		}

		private static Size Layout(Glyph[] glyphs)
		{
			// The layouting algorithm is a simple line-based algorithm, although the glyphs are sorted by height 
			// before the layouting; this hopefully results in all lines being mostly occupied
			// Start with a small power-of-two size and double either width or height (the smaller one) when the glyphs don't fit.

			var size = new Size(64, 64);
			bool allFit;

			do
			{
				var x = 0;
				var y = 0;
				var lineHeight = 0;

				if (size.Width <= size.Height)
					size = new Size(size.Width * 2, size.Height);
				else
					size = new Size(size.Width, size.Height * 2);

				for (var i = 0; i < glyphs.Length; ++i)
				{
					var width = glyphs[i].Width;
					var height = glyphs[i].Height;

					if (width == 0 && height == 0)
						continue;

					// Check if there is enough horizontal space left, otherwise start a new line
					if (x + width > size.Width)
					{
						x = 0;
						y += lineHeight;
						lineHeight = 0;
					}

					// Store the area
					glyphs[i].Area = new Rectangle(x, y, width, height);

					// Advance the current position
					x += width + Padding;
					lineHeight = Math.Max(lineHeight, height + 1);
				}

				allFit = y + lineHeight <= size.Height;
			} while (!allFit);

			return size;
		}

		private Glyph LoadGlyph(Face face, uint character, bool aliased)
		{
			var glyphIndex = face.GetCharIndex(character);
			if (glyphIndex == 0)
				throw new InvalidOperationException($"The font '{InFile}' does not contain a glyph for '{(char)character}'.");

			var flags = aliased ? LoadTarget.Mono : LoadTarget.Normal;
			face.LoadGlyph(glyphIndex, LoadFlags.Default, flags);
			face.Glyph.RenderGlyph(aliased ? RenderMode.Mono : RenderMode.Normal);

			return new Glyph
			{
				Character = (char)character,
				Index = glyphIndex,
				OffsetX = face.Glyph.BitmapLeft,
				OffsetY = face.Glyph.BitmapTop,
				AdvanceX = (int)face.Glyph.Advance.X,
				Width = face.Glyph.Bitmap.Width,
				Height = face.Glyph.Bitmap.Rows,
				Area = Rectangle.Empty,
				Bitmap = face.Glyph.Bitmap.Width == 0 || face.Glyph.Bitmap.Rows == 0
					? null
					: face.Glyph.Bitmap.ToGdipBitmap()
			};
		}

		private IEnumerable<char> ParseCharacterRanges(string range)
		{
			var ranges = range.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var r in ranges)
			{
				var pair = r.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
				if (pair.Length != 2)
					throw new InvalidOperationException($"'{InFile}': Invalid character range '{range}'.");

				if (!Int32.TryParse(pair[0], out var begin) || !Int32.TryParse(pair[1], out var end))
					throw new InvalidOperationException($"'{InFile}': Invalid character range '{range}'.");

				for (var i = begin; i <= end; ++i)
				{
					if (!Char.IsControl((char)i))
						yield return (char)i;
				}
			}
		}

		private struct Glyph
		{
			public uint Index;
			public int OffsetX;
			public int OffsetY;
			public int AdvanceX;
			public Bitmap Bitmap;
			public char Character;
			public int Width;
			public int Height;
			public Rectangle Area;
		}
	}
}