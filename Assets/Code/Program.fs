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

open Assets

[<EntryPoint>]
let main argv = 
    RegistryGenerator.generateCvars ()
    RegistryGenerator.generateCommands ()
    GLGenerator.generateIL ()

    Bundle.create ()
    |> FontCompiler.compile {  Name = "DefaultFont"; File = "LiberationMono-Regular.ttf";Size = 11; Aliased = true; InvalidChar = '□'; CharacterRange = (0, 256) }
    |> ShaderCompiler.compile { Name = "SpriteBatchShader"; VertexShader = "SpriteBatch.vert"; FragmentShader = "SpriteBatch.frag" }
    |> Bundle.generate

    0
