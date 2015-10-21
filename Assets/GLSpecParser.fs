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

module GLSpecParser

open System
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Xml
open System.Xml.Linq
open Utilities

type Enum = { Name : string; Value : string }
type Param = { Name : string; Type : string; Group : string }
type Func = { Name : string; Params : Param array; ReturnType : string }
type Version = { Number : string; Enums : Enum array; Funcs : Func array }

let private parse () =
    let spec = XDocument.Load "../../Dependencies/gl.xml"
    let enums =
        spec.Root.Descendants (XName.Get "enum")
        |> Seq.filter (fun e -> e.Attribute(XName.Get "value") <> null && (e.Attribute(XName.Get "api") = null || e.Attribute(XName.Get "api").Value = "gl"))
        |> Seq.map (fun e -> { Name = e.Attribute(XName.Get "name").Value; Value = e.Attribute(XName.Get "value").Value })
        |> Seq.map (fun e -> (e.Name, e))
        |> Map
    let funcs =
        spec.Root.Descendants (XName.Get "commands")
        |> Seq.collect (fun e -> e.Descendants (XName.Get "command"))
        |> Seq.filter (fun e -> e.Attribute(XName.Get "api") = null || e.Attribute(XName.Get "api").Value = "gl")
        |> Seq.map (fun e ->
            let name = e.Element(XName.Get "proto").Element(XName.Get "name").Value
            let returnType = e.Element(XName.Get "proto").Value.Replace(name, "").Trim();
            let parameters =
                e.Descendants (XName.Get "param")
                |> Seq.map (fun p -> 
                { 
                    Name = p.Element(XName.Get "name").Value
                    Type = p.Value.Substring(0, p.Value.LastIndexOf(p.Element(XName.Get "name").Value)).Trim()
                    Group = if p.Attribute(XName.Get "group") <> null then p.Attribute(XName.Get "group").Value else null
                })
                |> Seq.toArray
            
            { Name = name; Params = parameters; ReturnType = returnType }
        )
        |> Seq.map (fun f -> (f.Name, f))
        |> Map
    let groups = 
        spec.Root.Descendants (XName.Get "group")
        |> Seq.map (fun e -> 
            (e.Attribute(XName.Get "name").Value,
             e.Elements(XName.Get "enum") |> Seq.map (fun e' -> e'.Attribute(XName.Get "name").Value) |> Seq.toArray)
        )
        |> Seq.map (fun (name, enums') -> (name, enums' |> Seq.filter enums.ContainsKey |> Seq.map (fun e -> enums.[e]) |> Seq.toArray))
        |> Map
    let versions = new List<Version>()
    for version in spec.Root.Elements (XName.Get "feature") |> Seq.sortBy (fun e -> e.Attribute(XName.Get "number").Value) do
        if version.Attribute(XName.Get "api").Value = "gl" && version.Attribute(XName.Get "number").Value.[0] <= '3' then
            let getContents (section : XElement seq) =
                let section = section |> Seq.toArray
                let enums =
                    section
                    |> Seq.collect (fun e -> e.Descendants (XName.Get "enum"))
                    |> Seq.map (fun e -> e.Attribute(XName.Get "name").Value)
                    |> Seq.toArray
                let funcs = 
                    section
                    |> Seq.collect (fun e -> e.Descendants (XName.Get "command"))
                    |> Seq.map (fun e -> e.Attribute(XName.Get "name").Value)
                    |> Seq.toArray
                (enums, funcs)
            let (requiredEnums, requiredFuncs) =
                version.Descendants (XName.Get "require")
                |> Seq.filter (fun e -> e.Attribute(XName.Get "profile") = null || e.Attribute(XName.Get "profile").Value = "core")
                |> getContents
            let (removedEnums, removedFuncs) =
                version.Descendants (XName.Get "remove") |> getContents

            let featureEnums = 
                if versions.Count > 0 then 
                    seq { yield! requiredEnums; yield! versions.[versions.Count - 1].Enums |> Seq.map (fun e -> e.Name) }
                else requiredEnums |> Seq.ofArray
            let featureFuncs = 
                if versions.Count > 0 then
                    seq { yield! requiredFuncs; yield! versions.[versions.Count - 1].Funcs |> Seq.map (fun f -> f.Name) }
                else requiredFuncs |> Seq.ofArray

            let featureEnums = featureEnums |> Seq.except removedEnums
            let featureFuncs = featureFuncs |> Seq.except removedFuncs
            let featureFuncs = featureFuncs |> Seq.map (fun f -> funcs.[f])
                
            let featureEnums =
                if version.Attribute(XName.Get "number").Value = "1.0" then
                    featureFuncs 
                    |> Seq.collect (fun f -> f.Params)
                    |> Seq.map (fun p -> p.Group)
                    |> Seq.filter (fun g -> g <> null && groups.ContainsKey g)
                    |> Seq.collect (fun g -> groups.[g])
                else
                    featureEnums 
                    |> Seq.map (fun e -> enums.[e]) 

            versions.Add(
                { 
                    Number = version.Attribute(XName.Get "number").Value
                    Enums = featureEnums |> Seq.distinct |> Seq.sortBy (fun e -> e.Name) |> Seq.toArray
                    Funcs = featureFuncs |> Seq.distinct |> Seq.sortBy (fun f -> f.Name) |> Seq.toArray
                })

    versions |> Seq.filter (fun v -> v.Funcs |> Seq.exists (fun f -> f.Name = "glVertexP2uiv"))|> Seq.iter (fun v ->printfn "%s" v.Number)

    versions 
    |> Seq.filter (fun v -> v.Number = "3.3")
    |> Seq.exactlyOne

let rec private mapType (glType : string) =
    let glType = glType.Replace("const", "").Trim()
    let glType = if glType.EndsWith("*") then mapType(glType.Substring(0, glType.Length - 2)) + "*" else glType
    match glType with
    | "GLboolean" -> "bool"
    | "GLuint" | "GLenum" | "GLbitfield" -> "uint32"
    | "GLint" | "GLsizei" | "GLsizeiptr" | "GLfixed" | "GLclampx" | "GLinptrARB" | "GLsizeiptrARB" -> "int32"
    | "GLsync" | "GLintptr" | "GLDEBUGPROC" -> "void*"
    | "GLfloat" | "GLclampf" -> "float32"
    | "GLdouble" -> "float64"
    | "GLubyte" -> "uint8"
    | "GLbyte" -> "int8"
    | "GLushort" -> "uint16"
    | "GLshort" -> "int16"
    | "GLuint64" -> "uint64"
    | "GLint64" -> "int64"
    | "GLchar" -> "uint8"
    | glType -> glType

let generateIL () =
    let bindings = parse ()
    let writer = new CodeWriter ()

    writer.AppendLine ".class public auto ansi abstract sealed beforefieldinit PointWars.Platform.Graphics.OpenGL3 extends System.Object"
    writer.AppendBlockStatement (fun () ->
        for enum in bindings.Enums do
            let constantType =
                let mutable isHex = false
                let mutable value = enum.Value
                if value.StartsWith "0x" then
                    isHex <- true
                    value <- value.Substring 2

                let (success, _) =
                    if isHex then UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                    else UInt32.TryParse(value)

                if success then "uint32" else "uint64"

            writer.AppendLine ".field public static literal %s %s = %s(%s)" constantType enum.Name constantType enum.Value
        writer.NewLine()
                
        for func in bindings.Funcs do
            writer.AppendLine ".field private initonly static native int _%s" func.Name
        writer.NewLine()

        for func in bindings.Funcs do
            writer.Append ".method public hidebysig static %s %s" (mapType func.ReturnType) func.Name
            let parameters = String.Join(", ", func.Params |> Seq.map (fun p -> sprintf "%s '%s'" (mapType p.Type) p.Name))
            writer.AppendLine "(%s) cil managed aggressiveinlining" parameters
            writer.AppendBlockStatement (fun () ->
                writer.AppendLine ".custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = (01 00 00 00)"
                writer.AppendLine ".maxstack %d" (func.Params.Length + 1)
                writer.NewLine()

                func.Params |> Seq.iteri (fun idx _ -> writer.AppendLine "ldarg.s %d" idx)
                writer.AppendLine "ldsfld native int PointWars.Platform.Graphics.OpenGL3::_%s" func.Name
                let parameters = String.Join(", ", func.Params |> Seq.map (fun p -> mapType p.Type))
                writer.AppendLine "calli unmanaged stdcall %s(%s)" (mapType func.ReturnType) parameters
                writer.AppendLine "ret"
            )
            writer.NewLine()

        writer.AppendLine ".method public hidebysig static void Load(class [mscorlib]System.Func`2<string, native int> loader) cil managed" 
        writer.AppendBlockStatement (fun () ->
            writer.AppendLine ".maxstack 2"
            writer.NewLine()

            for func in bindings.Funcs do
                writer.AppendLine "ldarg.0"
                writer.AppendLine "ldstr \"%s\"" func.Name
                writer.AppendLine "call instance !1 class [mscorlib]System.Func`2<string, native int>::Invoke(!0)"
                writer.AppendLine "stsfld native int PointWars.Platform.Graphics.OpenGL3::_%s" func.Name
                writer.NewLine()
            writer.AppendLine "ret"
        )
    )

    File.WriteAllText ("../../Source/Platform/Graphics/OpenGL.gen.asm", writer.ToString())