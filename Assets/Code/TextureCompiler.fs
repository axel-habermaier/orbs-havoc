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

module TextureCompiler

#nowarn "9" // unsafe code

open System
open System.Linq
open System.IO
open Assets
open System.Drawing
open System.Drawing.Imaging
open Microsoft.FSharp.NativeInterop

let compileBitmap (bitmap : Bitmap) (bundle : AssetBundle) =
    let buffer = Array.zeroCreate (bitmap.Width * bitmap.Height * 4)
    let rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height)
    let imageData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat)
    let sourceData = NativePtr.ofNativeInt<byte> imageData.Scan0

    match bitmap.PixelFormat with
    | PixelFormat.Format24bppRgb ->
        // Switch from BGR to RGBA and to premultiplied alpha.
        let mutable i = 0
        let mutable j = 0
        while j < buffer.Length do
            let b = NativePtr.get sourceData i
            let g = NativePtr.get sourceData (i + 1)
            let r = NativePtr.get sourceData (i + 2)
            buffer.[j] <- r;
            buffer.[j + 1] <- g;
            buffer.[j + 2] <- b;
            buffer.[j + 3] <- (byte)255;
            i <- i + 3
            j <- j + 4
    | PixelFormat.Format32bppArgb ->
        // Switch from BGRA to RGBA and to premultiplied alpha.
        let mutable i = 0
        while i < buffer.Length do
            let b = (float32)(NativePtr.get sourceData i) / 255.0f;
            let g = (float32)(NativePtr.get sourceData (i + 1)) / 255.0f;
            let r = (float32)(NativePtr.get sourceData (i + 2)) / 255.0f;
            let a = (float32)(NativePtr.get sourceData (i + 3)) / 255.0f;

            buffer.[i] <- ((byte)(r * a * 255.0f));
            buffer.[i + 1] <- ((byte)(g * a * 255.0f));
            buffer.[i + 2] <- ((byte)(b * a * 255.0f));
            buffer.[i + 3] <- ((byte)(a * 255.0f));
            i <- i + 4
    | _ -> failwithf "Unsupported pixel format '%A'." bitmap.PixelFormat

    bitmap.UnlockBits imageData

    bundle.WriteUInt32((uint32)bitmap.Width)
    bundle.WriteUInt32((uint32)bitmap.Height)
    bundle.WriteByteArray buffer