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

namespace AssetsCompiler
{
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using CommandLine;

	public class ShaderCompiler : IExecutable
	{
		private const string Preamble = @"
	#version 330
	#extension GL_ARB_separate_shader_objects : enable
	#extension GL_ARB_shading_language_420pack : enable

	";

		[Option("input", Required = true, HelpText = "The path to the input shader file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output shader file.")]
		public string OutFile { get; set; }

		public void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			{
				var shader = File.ReadAllText(InFile);
				var match = Regex.Match(shader, @"Vertex(\s*){(?<vs>(.|\s)*?)}(\s*)Fragment(\s*){(?<fs>(.|\s)*?)}$", RegexOptions.Multiline);
				var vertexShader = Encoding.UTF8.GetBytes(Preamble + match.Groups["vs"].Value.Trim());
				var fragmentShader = Encoding.UTF8.GetBytes(Preamble + match.Groups["fs"].Value.Trim());

				writer.Write(vertexShader.Length);
				writer.Write(fragmentShader.Length);
				writer.Write(vertexShader);
				writer.Write(fragmentShader);
			}
		}
	}
}