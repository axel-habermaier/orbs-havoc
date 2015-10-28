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
	using System.IO;
	using System.IO.Compression;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using CommandLine;

	public class BundleCompiler : IExecutable
	{
		[Option("shaders", Required = true, HelpText = "The path to the input shader files.")]
		public string Shaders { get; set; }

		[Option("fonts", Required = true, HelpText = "The path to the input font files.")]
		public string Fonts { get; set; }

		[Option("textures", Required = true, HelpText = "The path to the input texture files.")]
		public string Textures { get; set; }

		[Option("cursors", Required = true, HelpText = "The path to the input cursor files.")]
		public string Cursors { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output bundle files.")]
		public string PakFile { get; set; }

		[Option("code", Required = true, HelpText = "The path to the generated code file.")]
		public string CodeFile { get; set; }

		public void Execute()
		{
			var shaders = Shaders.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(shader => shader).ToArray();
			var fonts = Fonts.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(font => font).ToArray();
			var textures = Textures.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(texture => texture).ToArray();
			var cursors = Cursors.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries).OrderBy(cursor => cursor).ToArray();
			var assets = shaders
				.Select(shader => new { Type = "Shader", Name = Path.GetFileNameWithoutExtension(shader), File = shader })
				.Concat(fonts.Select(font => new { Type = "Font", Name = Path.GetFileNameWithoutExtension(font), File = font }).OrderBy(s => s.Name))
				.Concat(textures.Select(texture => new { Type = "Texture", Name = Path.GetFileNameWithoutExtension(texture), File = texture }))
				.Concat(cursors.Select(cursor => new { Type = "Cursor", Name = Path.GetFileNameWithoutExtension(cursor), File = cursor }))
				.ToArray();

			// bundles with the same sequence of asset types and names should have the same hash
			byte[] hash;
			using (var cryptoProvider = new MD5CryptoServiceProvider())
			{
				var types = $"{shaders.Length}-{fonts.Length}-{textures.Length}";
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

			writer.AppendLine("namespace PointWars.Assets");
			writer.AppendBlockStatement(() =>
			{
				writer.AppendLine("using System;");
				writer.AppendLine("using Platform.Graphics;");
				writer.AppendLine("using Platform.Memory;");
				writer.AppendLine("using UserInterface;");
				writer.NewLine();

				writer.AppendLine("partial class Assets");
				writer.AppendBlockStatement(() =>
				{
					var hashBytes = String.Join(", ", hash.Select(b => b.ToString()));
					writer.AppendLine($"private static readonly Guid Guid = new Guid(new byte[] {{ {hashBytes} }});");
					writer.NewLine();

					foreach (var asset in assets)
						writer.AppendLine($"public static {asset.Type} {asset.Name} {{ get; private set; }}");

					writer.NewLine();
					writer.AppendLine("private static void LoadAssets(ref BufferReader reader)");
					writer.AppendBlockStatement(() =>
					{
						foreach (var asset in assets)
							writer.AppendLine($"{asset.Name} = {asset.Type}.Create(ref reader);");
					});

					writer.NewLine();
					writer.AppendLine("private static void ReloadAssets(ref BufferReader reader)");
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