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

namespace Assets

open System
open System.Collections.Generic
open System.IO
open System.IO.Compression
open System.Linq
open System.Text
open System.Security.Cryptography

type AssetType = Font | Texture | Shader

type AssetBundle () =
    let buffer = new MemoryStream(1024 * 1024)
    let writer = new BinaryWriter(buffer)
    let assets = new List<AssetType * string>()

    member this.AddAsset = assets.Add
    member this.WriteBoolean (value : bool) = writer.Write((byte)(if value then 1 else 0))
    member this.WriteSByte (value : sbyte) = writer.Write((byte)value)
    member this.WriteByte (value : byte) = writer.Write(value)
    member this.WriteInt16 (value : int16) = writer.Write(value)
    member this.WriteUInt16 (value : uint16) = writer.Write(value)
    member this.WriteCharacter (character : char) = this.WriteUInt16((uint16)character)
    member this.WriteInt32 (value : int32) = writer.Write(value)
    member this.WriteUInt32 (value : uint32) = writer.Write(value)
    member this.WriteInt64 (value : int64) = writer.Write(value)
    member this.WriteUInt64 (value : uint64) = writer.Write(value)
    member this.WriteString (value : string) = this.WriteByteArray(Encoding.UTF8.GetBytes(value))
    member this.WriteByteArray (value : byte[]) = this.WriteInt32(value.Length); this.Copy(value)
    member this.Copy (data : byte[]) = buffer.Write(data, 0, data.Length)

    member this.Generate () =
        writer.Flush()
        let buffer = buffer.ToArray()
        let hash =
            // bundles with the same sequence of asset types should have the same hash
            let typeSequence = String.Join(Environment.NewLine, assets |> Seq.map (fun (assetType, _) -> sprintf "%A" assetType))
            use cryptoProvider = new MD5CryptoServiceProvider() 
            cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes(typeSequence))
        let compressedContent =
            use output = new MemoryStream()
            let zip = new GZipStream(output, CompressionMode.Compress, true)
            zip.Write(buffer, 0, buffer.Length)
            zip.Dispose()
            output.ToArray()

        use outputBuffer = new MemoryStream(buffer.Length)
        use outputWriter = new BinaryWriter(outputBuffer)
        outputWriter.Write(hash)
        outputWriter.Write(buffer.Length)
        outputWriter.Write(compressedContent.Length)
        outputWriter.Write(compressedContent)
        outputWriter.Flush()

        let writer = CodeWriter ()
        writer.AppendLine "namespace PointWars.Rendering"
        writer.AppendBlockStatement (fun () ->
            writer.AppendLine "using System;"
            writer.AppendLine "using Platform.Graphics;"
            writer.AppendLine "using Platform.Memory;"
            writer.NewLine()

            writer.AppendLine "partial class Assets"
            writer.AppendBlockStatement (fun () ->
                let hash = String.Join(", ", hash |> Seq.map (fun b -> b.ToString()))
                writer.AppendLine "private static readonly Guid Guid = new Guid(new byte[] { %s });" hash
                writer.NewLine()

                for (assetType, assetName) in assets do
                    writer.AppendLine "public static %A %s { get; private set; }" assetType assetName

                writer.NewLine()
                writer.AppendLine("private static void LoadAssets(ref BufferReader reader)")
                writer.AppendBlockStatement (fun () ->
                    for (assetType, assetName) in assets do
                        writer.AppendLine "%s = %A.Create(ref reader);" assetName assetType
                )

                writer.NewLine()
                writer.AppendLine("private static void ReloadAssets(ref BufferReader reader)")
                writer.AppendBlockStatement (fun () ->
                    for (assetType, assetName) in assets do
                        writer.AppendLine "%s.Load(ref reader);" assetName
                )

                writer.NewLine()
                writer.AppendLine("private static void DisposeAssets()")
                writer.AppendBlockStatement (fun () ->
                    for (assetType, assetName) in assets do
                        writer.AppendLine "%s.SafeDispose();" assetName
                )
            )
        )

        File.WriteAllBytes("Assets.pak", outputBuffer.ToArray())
        File.WriteAllText("../../Source/Point Wars/Rendering/Assets.g.cs", writer.ToString())

module Bundle =
    let create () =
        new AssetBundle ()

    let generate (bundle : AssetBundle) =
        bundle.Generate ()