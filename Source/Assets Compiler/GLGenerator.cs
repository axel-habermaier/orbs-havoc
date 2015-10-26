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
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using CommandLine;

	public class GLGenerator : IExecutable
	{
		private static readonly string[] Extensions = { "GL_ARB_buffer_storage" };

		[Option("input", Required = true, HelpText = "The path to the input OpenGL file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output OpenGL file.")]
		public string OutFile { get; set; }

		public void Execute()
		{
			var spec = XDocument.Load(InFile);
			var enums = spec
				.Root.Descendants("enum")
				.Where(e => e.Attribute("value") != null && (e.Attribute("api") == null || e.Attribute("api").Value == "gl"))
				.Select(e => new { Name = e.Attribute("name").Value, Value = e.Attribute("value").Value, Element = e })
				.ToDictionary(e => e.Name);
			var funcs = spec
				.Root.Descendants("commands")
				.SelectMany(e => e.Descendants("command"))
				.Where(e => e.Attribute("api") == null || e.Attribute("api").Value == "gl")
				.Select(e =>
				{
					var name = e.Element("proto").Element("name").Value;
					var returnType = e.Element("proto").Value.Replace(name, "").Trim();
					var parameters =
						e.Descendants("param")
						 .Select(p => new
						 {
							 Name = p.Element("name").Value,
							 Type = p.Value.Substring(0, p.Value.LastIndexOf(p.Element("name").Value)).Trim(),
							 Group = p.Attribute("group") != null ? p.Attribute("group").Value : null
						 })
						 .ToArray();

					return new { Name = name, Params = parameters, ReturnType = returnType };
				})
				.ToDictionary(f => f.Name);
			var groups = spec
				.Root.Descendants("group")
				.Select(e =>
				{
					var groupName = e.Attribute("name").Value;
					var enumTypes = from type in e.Elements("enum")
									let name = type.Attribute("name").Value
									where enums.ContainsKey(name) && enums[name].Element.Parent.Attribute("vendor") == null
									select enums[name];

					return new { Name = groupName, Enums = enumTypes.ToArray() };
				})
				.ToDictionary(g => g.Name, g => g.Enums);

			var seed = new { Funcs = funcs.Values.ToList(), Enums = enums.Values.ToList() };
			seed.Enums.Clear();
			seed.Funcs.Clear();

			var gl = spec
				.Root.Elements("feature")
				.OrderBy(e => e.Attribute("number").Value)
				.Where(e => e.Attribute("api").Value == "gl" && e.Attribute("number").Value[0] <= '3')
				.Aggregate(seed, (info, e) =>
				{
					var removedFuncs = e
						.Descendants("remove")
						.SelectMany(f => f.Descendants("command"))
						.Select(f => f.Attribute("name").Value);

					var removedEnums = e
						.Descendants("remove")
						.SelectMany(f => f.Descendants("enum"))
						.Select(f => f.Attribute("name").Value);

					var requiredFuncs = e
						.Descendants("require")
						.Where(f => f.Attribute("profile") == null || f.Attribute("profile").Value == "core")
						.SelectMany(f => f.Descendants("command"))
						.Select(f => f.Attribute("name").Value)
						.Union(info.Funcs.Select(f => f.Name))
						.Except(removedFuncs)
						.Distinct()
						.Select(f => funcs[f])
						.ToList();

					var requiredEnums =
						(e.Attribute("number").Value == "1.0"
							? requiredFuncs
								.SelectMany(g => g.Params)
								.Select(g => g.Group)
								.Where(g => g != null && groups.ContainsKey(g))
								.SelectMany(g => groups[g].Select(h => h.Name))
							: e
								.Descendants("require")
								.Where(f => f.Attribute("profile") == null || f.Attribute("profile").Value == "core")
								.SelectMany(f => f.Descendants("enum"))
								.Select(f => f.Attribute("name").Value)
							)
							.Union(info.Enums.Select(f => f.Name))
							.Except(removedEnums)
							.Distinct()
							.Select(f => enums[f])
							.ToList();

					return new { Funcs = requiredFuncs, Enums = requiredEnums };
				});

			foreach (var extension in Extensions)
			{
				gl.Enums.AddRange(spec
					.Descendants("extension")
					.Where(e => e.Attribute("name").Value == extension)
					.SelectMany(e => e.Descendants("enum"))
					.Select(e => enums[e.Attribute("name").Value])
					.Except(gl.Enums));

				gl.Funcs.AddRange(spec
					.Descendants("extension")
					.Where(e => e.Attribute("name").Value == extension)
					.SelectMany(e => e.Descendants("command"))
					.Select(e => funcs[e.Attribute("name").Value])
					.Except(gl.Funcs));
			}

			var writer = new CodeWriter();
			writer.WriterHeader();

			writer.AppendLine(".class public auto ansi abstract sealed beforefieldinit PointWars.Platform.Graphics.OpenGL3 extends System.Object");
			writer.AppendBlockStatement(() =>
			{
				foreach (var e in gl.Enums)
				{
					var constantType = GetConstantType(e.Value);
					writer.AppendLine($".field public static literal {constantType} {e.Name} = {constantType}({e.Value})");
				}

				writer.NewLine();
				foreach (var func in gl.Funcs)
					writer.AppendLine($".field private initonly static native int _{func.Name}");

				writer.NewLine();
				foreach (var func in gl.Funcs)
				{
					writer.Append($".method public hidebysig static {MapType(func.ReturnType)} {func.Name}");
					var parameters = String.Join(", ", func.Params.Select(p => $"{MapType(p.Type)} '{p.Name}'"));
					writer.AppendLine($"({parameters}) cil managed aggressiveinlining");
					writer.AppendBlockStatement(() =>
					{
						writer.AppendLine(".custom instance void [mscorlib]System.Diagnostics.DebuggerHiddenAttribute::.ctor() = (01 00 00 00)");
						writer.AppendLine($".maxstack {func.Params.Length + 1}");
						writer.NewLine();

						for (var i = 0; i < func.Params.Length; ++i)
							writer.AppendLine($"ldarg.s {i}");

						writer.AppendLine($"ldsfld native int PointWars.Platform.Graphics.OpenGL3::_{func.Name}");
						var types = String.Join(", ", func.Params.Select(p => MapType(p.Type)));
						writer.AppendLine($"calli unmanaged stdcall {MapType(func.ReturnType)}({types})");
						writer.AppendLine("ret");
					});
					writer.NewLine();
				}

				writer.AppendLine(".method public hidebysig static void Load(class [mscorlib]System.Func`2<string, native int> loader) cil managed");
				writer.AppendBlockStatement(() =>
				{
					writer.AppendLine(".maxstack 2");
					writer.NewLine();

					foreach (var func in gl.Funcs)
					{
						writer.AppendLine("ldarg.0");
						writer.AppendLine($"ldstr \"{func.Name}\"");
						writer.AppendLine("call instance !1 class [mscorlib]System.Func`2<string, native int>::Invoke(!0)");
						writer.AppendLine($"stsfld native int PointWars.Platform.Graphics.OpenGL3::_{func.Name}");
						writer.NewLine();
					}
					writer.AppendLine("ret");
				});
			});
			File.WriteAllText(OutFile, writer.ToString());
		}

		private static string MapType(string glType)
		{
			glType = glType.Replace("const", "").Trim();
			glType = glType.EndsWith("*") ? MapType(glType.Substring(0, glType.Length - 1)) + "*" : glType;
			switch (glType)
			{
				case "GLboolean":
					return "bool";
				case "GLuint":
				case "GLenum":
				case "GLbitfield":
				case "GLint":
				case "GLsizei":
				case "GLfixed":
				case "GLclampx":
					return "int32";
				case "GLsizeiptr":
				case "GLsync":
				case "GLintptr":
				case "GLDEBUGPROC":
					return "void*";
				case "GLfloat":
				case "GLclampf":
					return "float32";
				case "GLdouble":
					return "float64";
				case "GLubyte":
				case "GLbyte":
				case "GLchar":
					return "uint8";
				case "GLushort":
				case "GLshort":
					return "int16";
				case "GLuint64":
				case "GLint64":
					return "int64";
				default:
					return glType;
			}
		}

		private static string GetConstantType(string value)
		{
			var isHex = false;
			if (value.StartsWith("0x"))
			{
				isHex = true;
				value = value.Substring(2);
			}

			uint result;
			if (isHex && UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
				return "int32";

			if (UInt32.TryParse(value, out result))
				return "int32";

			return "int64";
		}
	}
}