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

module FontCompiler

open System
open System.Linq
open System.IO
open Assets
open System.Drawing
open System.Drawing.Imaging
open SharpFont

type Glyph = {
    Index : uint32
    OffsetX : int
    OffsetY : int
    AdvanceX : int
    Bitmap : Bitmap
    Character : char
    Width : int32
    Height : int32
    mutable Area : Rectangle
}

type Font = { 
    Name : string
    File : string
    Size : int
    Aliased : bool
    InvalidChar : char
    CharacterRange : int * int
}

type KerningPair = {
    Left : Glyph
    Right : Glyph
    Offset : int
}

let private loadGlyph font (face : Face) c =
    let glyphIndex = face.GetCharIndex(c)
    if glyphIndex = 0u then
        failwithf "The font does not contain a glyph for '%c'." ((char)c)

    let flags = if font.Aliased then LoadTarget.Mono else LoadTarget.Normal
    face.LoadGlyph(glyphIndex, LoadFlags.Default, flags)
    face.Glyph.RenderGlyph(if font.Aliased then RenderMode.Mono else RenderMode.Normal)

    {
        Character = (char)c
        Index = glyphIndex
        OffsetX = face.Glyph.BitmapLeft
        OffsetY = face.Glyph.BitmapTop
        AdvanceX = (int)face.Glyph.Advance.X
        Width = face.Glyph.Bitmap.Width
        Height = face.Glyph.Bitmap.Rows
        Area = Rectangle.Empty
        Bitmap = 
            if face.Glyph.Bitmap.Width = 0 || face.Glyph.Bitmap.Rows = 0 then null
            else face.Glyph.Bitmap.ToGdipBitmap()
    }

let compile font (bundle : AssetBundle) =
    let path = Path.Combine("../../Assets/Fonts", font.File)
    use freeType = new Library()
    use face = new Face(freeType, path)
    let (firstChar, lastChar) = font.CharacterRange
    let characters = seq { yield font.InvalidChar; yield! [(char)firstChar .. (char)lastChar] } |> Seq.distinct |> Seq.filter (Char.IsControl >> not)
    face.SetPixelSizes(0u, (uint32)font.Size)
    let glyphs = characters |> Seq.map (fun c -> loadGlyph font face ((uint32)c)) |> Seq.filter (fun g -> g.Bitmap <> null)
    let glyphs = glyphs |> Seq.sortBy (fun g -> g.Height) |> Seq.toArray
    let lineHeight = (int)face.Size.Metrics.Height
    let baseline = lineHeight + (int)face.Size.Metrics.Descender
    let padding = 1

    let getKerning left right =
        (int)(face.GetKerning(left.Index, right.Index, KerningMode.Default).X)

    let kerningPairs = 
        if face.FaceFlags.HasFlag(FaceFlags.Kerning) then
            seq {
                for left in glyphs do
                    for right in glyphs do
                        let offset = getKerning left right
                        if offset <> 0 then
                            yield { Left = left; Right = right; Offset = offset }
            } |> Seq.toArray
        else [||]

    // The layouting algorithm is a simple line-based algorithm, although the glyphs are sorted by height 
    // before the layouting; this hopefully results in all lines being mostly occupied
    // Start with a small power-of-two size and double either width or height (the smaller one) when the glyphs don't fit.
    let rec layoutGlyphs width height =
        let mutable width = width
        let mutable height = height
        let mutable x = 0
        let mutable y = 0
        let mutable lineHeight = 0

        if width <= height then
            width <- width * 2
        else
            height <- height * 2

        glyphs |> Seq.iter (fun g ->
            // Check if there is enough horizontal space left, otherwise start a new line
            if x + g.Width > width then
                x <- 0
                y <- y + lineHeight
                lineHeight <- 0

            // Store the area
            g.Area <- new Rectangle(x, y, g.Width, g.Height)

            // Advance the current position
            x <- x + g.Width + padding;
            lineHeight <- Math.Max(lineHeight, g.Height + 1)
        )

        // If we overflowed, try again
        if y + lineHeight > height then layoutGlyphs width height
        else (width, height)

    let (width, height) = layoutGlyphs 64 64
    use bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb)
    use graphics = Graphics.FromImage(bitmap)
    glyphs |> Seq.iter (fun g -> graphics.DrawImage(g.Bitmap, new Point(g.Area.Left, g.Area.Top)))
    TextureCompiler.compileBitmap bitmap bundle

    // Write font metadata
    bundle.WriteUInt16 ((uint16)lineHeight)
    bundle.WriteUInt16 ((uint16)glyphs.Length);
    bundle.WriteUInt32 ((uint32)kerningPairs.Length) 

    // Write glyph metadata
    glyphs |> Array.iter (fun glyph ->
        // Glyphs are identified by their character ASCII id, except for the invalid character, which must lie at index 0
        if glyph.Character = font.InvalidChar then
            bundle.WriteByte((byte)0);
        else
            bundle.WriteByte((byte)glyph.Character);

        // Write the font map texture coordinates in pixels
        bundle.WriteUInt16((uint16)glyph.Area.Left);
        bundle.WriteUInt16((uint16)glyph.Area.Top);
        bundle.WriteUInt16((uint16)glyph.Area.Width);
        bundle.WriteUInt16((uint16)glyph.Area.Height);

        // Write the glyph offsets
        bundle.WriteInt16((int16)glyph.OffsetX);
        bundle.WriteInt16((int16)(baseline - glyph.OffsetY));
        bundle.WriteInt16((int16)glyph.AdvanceX);
    )

    // Write kerning pairs
    kerningPairs |> Array.iter (fun pair ->
        bundle.WriteUInt16((uint16)pair.Left.Character);
        bundle.WriteUInt16((uint16)pair.Right.Character);
        bundle.WriteInt16((int16)pair.Offset);
    )

    // Add the font asset
    bundle.AddAsset (AssetType.Font, font.Name)
    bundle
