﻿namespace AssetsCompiler
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using CommandLine;
	using JetBrains.Annotations;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal class BundleCompiler : CompilationTask
	{
		[Option("shaders", Required = true, HelpText = "The paths to the input shader files.")]
		public string Shaders { get; set; }

		[Option("fonts", Required = true, HelpText = "The paths to the input font files.")]
		public string Fonts { get; set; }

		[Option("textures", Required = true, HelpText = "The paths to the input texture files.")]
		public string Textures { get; set; }

		[Option("cursors", Required = true, HelpText = "The paths to the input cursor files.")]
		public string Cursors { get; set; }

		[Option("levels", Required = true, HelpText = "The paths to the input level files.")]
		public string Levels { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output bundle files.")]
		public string PakFile { get; set; }

		[Option("code", Required = true, HelpText = "The path to the generated code file.")]
		public string CodeFile { get; set; }

		protected override string GeneratedFile => PakFile;

		protected override void Execute()
		{
			var shaders = Shaders.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(shader => shader).ToArray();
			var fonts = Fonts.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(font => font).ToArray();
			var textures = Textures.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(texture => texture).ToArray();
			var levels = Levels.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(level => level).ToArray();
			var cursors = Cursors.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(cursor => cursor).ToArray();
			var assets = shaders
				.Select(shader => new { Type = "Shader", Name = Path.GetFileNameWithoutExtension(shader), File = shader })
				.Concat(fonts.Select(font => new { Type = "Font", Name = Path.GetFileNameWithoutExtension(font), File = font }).OrderBy(s => s.Name))
				.Concat(textures.Select(texture => new { Type = "Texture", Name = Path.GetFileNameWithoutExtension(texture), File = texture }))
				.Concat(levels.Select(level => new { Type = "Level", Name = Path.GetFileNameWithoutExtension(level), File = level }))
				.Concat(cursors.Select(cursor => new { Type = "Cursor", Name = Path.GetFileNameWithoutExtension(cursor), File = cursor }))
				.OrderBy(asset => asset.Type)
				.ThenBy(asset => asset.Name)
				.ToArray();

			byte[] hash;
			using (var cryptoProvider = new MD5CryptoServiceProvider())
			{
				var types = String.Join("\n", assets.GroupBy(asset => asset.Type).Select(group => group.Count()));
				var names = String.Join("\n", assets.Select(asset => asset.Name));
				hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes($"{types};{names}"));
			}

			var uncompressedContent = new MemoryStream();
			foreach (var fileContent in assets.Select(asset => File.ReadAllBytes(asset.File)))
				uncompressedContent.Write(fileContent, 0, fileContent.Length);

			var contentArray = uncompressedContent.ToArray();
			byte[] compressedContent;
			using (var output = new MemoryStream())
			{
				using (var zip = new GZipStream(output, CompressionMode.Compress, true))
					zip.Write(contentArray, 0, contentArray.Length);
				compressedContent = output.ToArray();
			}

			using (var stream = File.Create(PakFile))
			using (var pakWriter = new BinaryWriter(stream))
			{
				pakWriter.Write(hash);
				pakWriter.Write(contentArray.Length);
				pakWriter.Write(compressedContent.Length);
				pakWriter.Write(compressedContent);
			}

			var writer = new CodeWriter();
			writer.WriterHeader();

			writer.AppendLine("namespace OrbsHavoc.Assets");
			writer.AppendBlockStatement(() =>
			{
				writer.AppendLine("using System;");
				writer.AppendLine("using Gameplay;");
				writer.AppendLine("using Platform.Graphics;");
				writer.AppendLine("using Platform.Memory;");
				writer.AppendLine("using UserInterface;");
				writer.NewLine();

				writer.AppendLine("partial class AssetBundle");
				writer.AppendBlockStatement(() =>
				{
					var hashBytes = String.Join(", ", hash.Select(b => b.ToString()));
					writer.AppendLine($"private static readonly Guid Guid = new Guid(new byte[] {{ {hashBytes} }});");
					writer.NewLine();

					foreach (var asset in assets)
						writer.AppendLine($"public static {asset.Type} {asset.Name} {{ get; }} = new {asset.Type}();");

					writer.NewLine();
					writer.AppendLine("private static void LoadAssets(BufferReader reader)");
					writer.AppendBlockStatement(() =>
					{
						foreach (var asset in assets)
							writer.AppendLine($"{asset.Name}.Load(ref reader);");
					});

					writer.NewLine();
					writer.AppendLine("private static void DisposeAssets()");
					writer.AppendBlockStatement(() =>
					{
						foreach (var asset in assets)
							writer.AppendLine($"{asset.Name}.SafeDispose();");
					});
				});
			});

			File.WriteAllText(CodeFile, writer.ToString());
		}
	}
}