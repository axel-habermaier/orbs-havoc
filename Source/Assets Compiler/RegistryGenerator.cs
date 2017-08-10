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
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal class RegistryGenerator : CompilationTask
	{
		[Option("input", Required = true, HelpText = "The path to the input cvars file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output cvars file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			var syntaxTree = SyntaxFactory.ParseSyntaxTree(File.ReadAllText(InFile));
			var root = syntaxTree.GetRoot();
			var writer = new CodeWriter();

			writer.WriterHeader();
			writer.AppendLine("namespace OrbsHavoc.Scripting");

			writer.AppendBlockStatement(() =>
			{
				var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().Select(u => u.Name.ToString());
				var allUsings = new[] { "System", "System.Diagnostics", "Utilities" }.Union(usings).OrderBy(u => u);

				foreach (var ns in allUsings)
					writer.AppendLine($"using {ns};");

				writer.NewLine();
				GenerateCvars(writer, root);
				GenerateCommands(writer, root);
			});

			File.WriteAllText(OutFile, writer.ToString());
		}

		private static string GetGenericTypes(MethodDeclarationSyntax command)
		{
			if (command.ParameterList.Parameters.Count == 0)
				return String.Empty;

			return $"<{(String.Join(", ", command.ParameterList.Parameters.Select(p => p.Type.ToString())))}>";
		}

		private static string GetRuntimeName(string name)
		{
			var builder = new StringBuilder();
			builder.Append(Char.ToLower(name[0]));

			foreach (var c in name.Skip(1))
			{
				if (Char.IsUpper(c))
					builder.Append("_");
				builder.Append(Char.ToLower(c));
			}

			return builder.ToString();
		}

		private static string GetSummaryText(string summary)
		{
			var lines = summary.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var comment = String.Join(" ", lines.Select(c => c.Replace("///", "").Trim()));
			var regexMatch = Regex.Match(comment, "<summary>(.*)</summary>");

			return regexMatch.Groups[1].Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Trim();
		}

		private static string GetParameterTag(string parameter, string summary)
		{
			var lines = summary.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			var comment = String.Join(" ", lines.Select(c => c.Replace("///", "").Trim()));
			var regexMatch = Regex.Match(comment, $"<param name=\"{parameter}\">(.*?)</param>");

			return regexMatch.Groups[1].Value.Replace("\\", "\\\\").Replace("\"", "\\\"").Trim();
		}

		private static IEnumerable<string> GetSummary(string docComment)
		{
			yield return " <summary>";
			yield return $"   {GetSummaryText(docComment)}";
			yield return " </summary>";
		}

		private static void WriteDocumentation(CodeWriter writer, IEnumerable<string> docComment)
		{
			foreach (var line in docComment)
				writer.AppendLine($"///{line}");
		}

		private static bool HasAttribute(SyntaxNode node, string attribute)
		{
			return node.DescendantNodes().OfType<AttributeSyntax>().Any(a => a.Name.ToString() == attribute);
		}

		private static IEnumerable<AttributeSyntax> GetValidators(SyntaxNode node)
		{
			return node.DescendantNodes().OfType<AttributeSyntax>().Where(a =>
			{
				var name = a.Name.ToString();
				return name != "DefaultValue" && name != "SystemOnly" && name != "Persistent";
			}).ToArray();
		}

		private static void WriteValidators(CodeWriter writer, SyntaxNode node)
		{
			var validators = GetValidators(node).ToArray();
			if (validators.Length > 0)
				writer.Append(", ");

			writer.AppendSeparated(validators, ", ", validator =>
			{
				writer.Append($"new {validator.Name.ToString()}Attribute");
				if (validator.ArgumentList == null)
					writer.Append("()");
				else
					writer.Append($"{validator.ArgumentList}");
			});
		}

		private static void GenerateCvars(CodeWriter writer, SyntaxNode root)
		{
			var cvars = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().OrderBy(c => c.Identifier.ToString()).ToArray();
			if (cvars.Length == 0)
				return;

			writer.AppendLine("partial class Cvars");
			writer.AppendBlockStatement(() =>
			{
				foreach (var cvar in cvars)
				{
					writer.AppendLine(cvar.GetLeadingTrivia().ToString().Trim());
					writer.AppendLine($"public static Cvar<{cvar.Type}> {cvar.Identifier}Cvar {{ get; private set; }}");
					writer.NewLine();
				}

				foreach (var cvar in cvars)
				{
					writer.AppendLine(cvar.GetLeadingTrivia().ToString().Trim());
					writer.AppendLine($"public static {cvar.Type} {cvar.Identifier}");
					writer.AppendBlockStatement(() =>
					{
						writer.AppendLine("[DebuggerHidden]");
						writer.AppendLine($"get {{ return {cvar.Identifier}Cvar.Value; }}");
						writer.AppendLine("[DebuggerHidden]");
						writer.AppendLine("set");
						writer.AppendBlockStatement(() =>
						{
							writer.AppendLine("Assert.ArgumentNotNull((object)value, nameof(value));");
							writer.AppendLine($"{cvar.Identifier}Cvar.Value = value;");
						});
					});
					writer.NewLine();
				}

				foreach (var cvar in cvars)
				{
					writer.AppendLine("/// <summary>");
					writer.AppendLine($"///  Raised when the '{cvar.Identifier}' cvar is changed.");
					writer.AppendLine("/// </summary>");
					writer.AppendLine($"public static event Action {cvar.Identifier}Changed");
					writer.AppendBlockStatement(() =>
					{
						writer.AppendLine($"add {{ {cvar.Identifier}Cvar.Changed += value; }}");
						writer.AppendLine($"remove {{ {cvar.Identifier}Cvar.Changed -= value; }}");
					});
					writer.NewLine();
				}
				writer.AppendLine("/// <summary>");
				writer.AppendLine("///  Initializes the declared cvars.");
				writer.AppendLine("/// </summary>");
				writer.AppendLine("public static void Initialize()");
				writer.AppendBlockStatement(() =>
				{
					foreach (var cvar in cvars)
					{
						var runtimeName = GetRuntimeName(cvar.Identifier.ToString());
						var defaultValueAttribute = cvar.DescendantNodes().OfType<AttributeSyntax>().First(a => a.Name.ToString() == "DefaultValue");
						var defaultValue = defaultValueAttribute.ArgumentList.Arguments.First().ToString().Trim();
						if (defaultValue.StartsWith("@\""))
							defaultValue = defaultValue.Substring(2, defaultValue.Length - 3).Replace("\"\"", "\"");
						if (defaultValue.StartsWith("\"") && cvar.Type.ToString() != "string")
							defaultValue = defaultValue.Substring(1, defaultValue.Length - 2);

						writer.Append($"{cvar.Identifier}Cvar = new Cvar<{cvar.Type}>(");
						writer.Append($"\"{runtimeName}\", {defaultValue}, \"{GetSummaryText(cvar.GetLeadingTrivia().ToString())}\", ");
						writer.Append($"{HasAttribute(cvar, "Persistent").ToString().ToLower()}, ");
						writer.Append($"{HasAttribute(cvar, "SystemOnly").ToString().ToLower()}");
						WriteValidators(writer, cvar);
						writer.AppendLine(");");
					}

					writer.NewLine();
					foreach (var cvar in cvars)
						writer.AppendLine($"Register({cvar.Identifier}Cvar);");
				});
			});
		}

		private static void GenerateCommands(CodeWriter writer, SyntaxNode root)
		{
			var commands = root.DescendantNodes().OfType<MethodDeclarationSyntax>().OrderBy(c => c.Identifier.ToString()).ToArray();
			if (commands.Length == 0)
				return;

			writer.AppendLine("partial class Commands");
			writer.AppendBlockStatement(() =>
			{
				foreach (var command in commands)
				{
					WriteDocumentation(writer, GetSummary(command.GetLeadingTrivia().ToString().Trim()));
					writer.AppendLine($"public static Command{GetGenericTypes(command)} {command.Identifier}Command {{ get; private set; }}");
					writer.NewLine();
				}

				foreach (var command in commands)
				{
					var parameters = String.Join(", ", command.ParameterList.Parameters.Select(
						p => p.Default != null ? $"{p.Type} {p.Identifier} = {p.Default.Value}" : $"{p.Type} {p.Identifier}"));
					writer.AppendLine(command.GetLeadingTrivia().ToString().Trim());
					writer.AppendLine("[DebuggerHidden]");
					writer.AppendLine($"public static {command.ReturnType} {command.Identifier}({parameters})");
					writer.AppendBlockStatement(() =>
					{
						foreach (var parameter in command.ParameterList.Parameters)
							writer.AppendLine($"Assert.ArgumentNotNull((object){parameter.Identifier}, nameof({parameter.Identifier}));");

						var arguments = String.Join(", ", command.ParameterList.Parameters.Select(p => p.Identifier.ToString()));
						writer.AppendLine($"{command.Identifier}Command.Invoke({arguments});");
					});
					writer.NewLine();
				}

				foreach (var command in commands)
				{
					writer.AppendLine("/// <summary>");
					writer.AppendLine($"///   Raised when the '{command.Identifier}' command is invoked.");
					writer.AppendLine("/// </summary>");
					writer.AppendLine($"public static event Action{GetGenericTypes(command)} On{command.Identifier}");
					writer.AppendBlockStatement(() =>
					{
						writer.AppendLine($"add {{ {command.Identifier}Command.Invoked += value; }}");
						writer.AppendLine($"remove {{ {command.Identifier}Command.Invoked -= value; }}");
					});
					writer.NewLine();
				}

				writer.AppendLine("/// <summary>");
				writer.AppendLine("///  Initializes the declared commands.");
				writer.AppendLine("/// </summary>");
				writer.AppendLine("public static void Initialize()");
				writer.AppendBlockStatement(() =>
				{
					foreach (var command in commands)
					{
						var runtimeName = GetRuntimeName(command.Identifier.ToString());
						writer.Append($"{command.Identifier}Command = new Command{GetGenericTypes(command)}(");
						writer.Append($"\"{runtimeName}\", \"{GetSummaryText(command.GetLeadingTrivia().ToString())}\", ");
						writer.Append(HasAttribute(command, "SystemOnly").ToString().ToLower());
						if (command.ParameterList.Parameters.Count > 0)
							writer.Append(", ");

						writer.AppendSeparated(command.ParameterList.Parameters, ", ", parameter =>
						{
							var parameterDescription = GetParameterTag(parameter.Identifier.ToString(), command.GetLeadingTrivia().ToString());

							writer.Append($"new CommandParameter(\"{parameter.Identifier}\", typeof({parameter.Type}), ");
							writer.Append($"{(parameter.Default != null).ToString().ToLower()}, ");
							if (parameter.Default == null)
								writer.Append($"default({parameter.Type})");
							else
								writer.Append(parameter.Default.Value.ToString());

							writer.Append($", \"{parameterDescription}\"");
							WriteValidators(writer, parameter);
							writer.Append(")");
						});
						writer.AppendLine(");");
					}

					writer.NewLine();
					foreach (var command in commands)
						writer.AppendLine($"Register({command.Identifier}Command);");
				});
			});
		}
	}
}