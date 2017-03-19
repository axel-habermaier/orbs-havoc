// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Text.RegularExpressions;
	using CommandLine;
	using JetBrains.Annotations;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class ShaderCompiler : CompilationTask
	{
		private const string Preamble = "#version 330\n";

		[Option("input", Required = true, HelpText = "The path to the input shader file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output shader file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			{
				var shader = File.ReadAllText(InFile);
				var includesMatch = Regex.Match(shader, @"#include ""(.*)""");

				while (includesMatch.Success)
				{
					var include = File.ReadAllText(Path.Combine(Path.GetDirectoryName(InFile), includesMatch.Groups[1].Value.Trim()));
					shader = shader.Replace(includesMatch.Groups[0].Value, include);

					includesMatch = includesMatch.NextMatch();
				}

				var samplers = ExtractSamplers(ref shader);
				var blocks = ExtractUniformBlocks(ref shader);

				var vertexShader = Preamble + ExtractShader(shader, "Vertex");
				var geometryShader = ExtractShader(shader, "Geometry");
				var fragmentShader = Preamble + ExtractShader(shader, "Fragment");

				if (!String.IsNullOrWhiteSpace(geometryShader))
					geometryShader = Preamble + geometryShader;

				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					Validate(vertexShader, "vert");
					if (!String.IsNullOrWhiteSpace(geometryShader))
						Validate(geometryShader, "geom");
					Validate(fragmentShader, "frag");
				}

				var vertexShaderBytes = Encoding.UTF8.GetBytes(vertexShader);
				writer.Write(vertexShaderBytes.Length);
				writer.Write(vertexShaderBytes);

				if (!String.IsNullOrWhiteSpace(geometryShader))
				{
					var geometryShaderBytes = Encoding.UTF8.GetBytes(geometryShader);
					writer.Write(geometryShaderBytes.Length);
					writer.Write(geometryShaderBytes);
				}
				else
					writer.Write(0);

				var fragmentShaderBytes = Encoding.UTF8.GetBytes(fragmentShader);
				writer.Write(fragmentShaderBytes.Length);
				writer.Write(fragmentShaderBytes);

				WriteMetadata(writer, samplers);
				WriteMetadata(writer, blocks);
			}
		}

		private static void WriteMetadata(BinaryWriter writer, List<Tuple<string, int>> items)
		{
			writer.Write(items.Count);
			foreach (var item in items)
			{
				var name = Encoding.UTF8.GetBytes(item.Item1);
				writer.Write(name.Length + 1);

				foreach (var c in name)
					writer.Write(c);

				writer.Write((byte)0);
				writer.Write(item.Item2);
			}
		}

		private static string ExtractShader(string shader, string keyword)
		{
			return shader
				.Split('\n')
				.SkipWhile(l => !l.StartsWith($"// {keyword}"))
				.Skip(1)
				.TakeWhile(l => !l.StartsWith("// Vertex") && !l.StartsWith("// Geometry") && !l.StartsWith("// Fragment"))
				.Aggregate(String.Empty, (s, line) => s + line.Trim() + Environment.NewLine);
		}

		private static List<Tuple<string, int>> ExtractSamplers(ref string shader)
		{
			const string regex = @"layout\s*\(\s*binding\s*=\s*(?<slot>\d*)\s*\)\s*uniform\s*sampler2D\s*(?<sampler>.*)\s*;";

			var samplers = new List<Tuple<string, int>>();
			var match = Regex.Match(shader, regex, RegexOptions.Multiline);

			while (match.Success)
			{
				samplers.Add(Tuple.Create(match.Groups["sampler"].Value, Int32.Parse(match.Groups["slot"].Value)));
				match = match.NextMatch();
			}

			shader = Regex.Replace(shader, regex, "uniform sampler2D ${sampler};", RegexOptions.Multiline);
			return samplers;
		}

		private static List<Tuple<string, int>> ExtractUniformBlocks(ref string shader)
		{
			const string regex = @"layout\s*\(std140\s*,\s*binding\s*=\s*(?<binding>\d*)\s*\)\s*uniform\s*(?<block>\S*)\s*\{";

			var match = Regex.Match(shader, regex, RegexOptions.Multiline);
			var blocks = new List<Tuple<string, int>>();

			while (match.Success)
			{
				blocks.Add(Tuple.Create(match.Groups["block"].Value, Int32.Parse(match.Groups["binding"].Value)));
				match = match.NextMatch();
			}

			shader = Regex.Replace(shader, regex, "layout(std140) uniform ${block} {", RegexOptions.Multiline);
			return blocks;
		}

		private void Validate(string shader, string extension)
		{
			var path = Path.ChangeExtension(InFile, extension);

			try
			{
				File.WriteAllText(path, shader);

				var process = new ExternalProcess("../../Dependencies/glslangValidator.exe", $"\"{path}\"");
				if (process.Run() != 0)
					throw new InvalidOperationException($"GLSL shader '{InFile}' contains errors.");
			}
			finally
			{
				File.Delete(path);
			}
		}
	}
}