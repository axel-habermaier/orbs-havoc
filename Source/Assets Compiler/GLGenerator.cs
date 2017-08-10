namespace AssetsCompiler
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using CommandLine;
	using JetBrains.Annotations;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal class GLGenerator : CompilationTask
	{
		private static readonly string[] _extensions = { };

		[Option("input", Required = true, HelpText = "The path to the input OpenGL file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output OpenGL file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			var spec = XDocument.Load(InFile);
			var enums = spec
				.Root.Descendants("enum")
				.Where(e => e.Attribute("value") != null && (e.Attribute("api") == null || e.Attribute("api").Value == "gl"))
				.Select(e => new { Name = e.Attribute("name").Value, e.Attribute("value").Value, Element = e })
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
							 Type = p.Value.Substring(0, p.Value.LastIndexOf(p.Element("name").Value, StringComparison.Ordinal)).Trim(),
							 Group = p.Attribute("group")?.Value
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

			foreach (var extension in _extensions)
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

			var orderedEnums = gl.Enums.OrderBy(e => e.Name);
			var orderedFuncs = gl.Funcs.OrderBy(f => f.Name);

			writer.AppendLine("namespace OrbsHavoc.Platform.Graphics");
			writer.AppendBlockStatement(() =>
			{
				writer.AppendLine("using System.Diagnostics;");
				writer.AppendLine("using System.Runtime.CompilerServices;");
				writer.NewLine();

				writer.AppendLine("internal unsafe static partial class OpenGL3");
				writer.AppendBlockStatement(() =>
				{
					foreach (var e in orderedEnums)
					{
						var constantType = GetConstantType(e.Value);
						writer.AppendLine($"public const {constantType} {e.Name} = unchecked(({constantType}){e.Value});");
					}

					writer.NewLine();
					foreach (var func in orderedFuncs)
					{
						var parameters = String.Join(", ", func.Params.Select(p => $"{MapType(p.Type)} @{p.Name}"));
						writer.AppendLine($"private delegate {MapType(func.ReturnType)} {func.Name}Func({parameters});");
					}

					writer.NewLine();
					foreach (var func in orderedFuncs)
						writer.AppendLine($"private static {func.Name}Func _{func.Name};");

					writer.NewLine();
					foreach (var func in orderedFuncs)
					{
						writer.AppendLine("[MethodImpl(MethodImplOptions.AggressiveInlining), DebuggerHidden]");
						writer.Append($"public static {MapType(func.ReturnType)} {func.Name}");
						var parameters = String.Join(", ", func.Params.Select(p => $"{MapType(p.Type)} @{p.Name}"));
						writer.AppendLine($"({parameters})");
						writer.AppendBlockStatement(() =>
						{
							writer.AppendLine($"if (_{func.Name} == null)");
							writer.IncreaseIndent();
							writer.AppendLine($"_{func.Name} = Load<{func.Name}Func>(\"{func.Name}\");");
							writer.DecreaseIndent();
							writer.NewLine();

							if (func.ReturnType != "void")
								writer.Append("var _result = ");

							writer.Append($"_{func.Name}(");
							writer.AppendSeparated(func.Params, ", ", p => writer.Append("@" + p.Name));
							writer.AppendLine(");");

							if (func.Name != "glGetError")
								writer.AppendLine("CheckErrors();");

							if (func.ReturnType != "void")
								writer.AppendLine("return _result;");
						});
						writer.NewLine();
					}
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
					return "int";
				case "GLsync":
				case "GLintptr":
				case "GLDEBUGPROC":
					return "void*";
				case "GLfloat":
				case "GLclampf":
					return "float";
				case "GLdouble":
					return "double";
				case "GLubyte":
				case "GLbyte":
				case "GLchar":
					return "byte";
				case "GLushort":
				case "GLshort":
					return "short";
				case "GLuint64":
				case "GLint64":
				case "GLsizeiptr":
					return "long";
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

			if (isHex && UInt32.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var result))
				return "int";

			if (UInt32.TryParse(value, out result))
				return "int";

			return "long";
		}
	}
}