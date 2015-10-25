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
		[Option("shaders", Required = true, HelpText = "The path to the input shader file.")]
		public string Shaders { get; set; }

		[Option("fonts", Required = true, HelpText = "The path to the input fonts file.")]
		public string Fonts { get; set; }

		[Option("textures", Required = true, HelpText = "The path to the input texture file.")]
		public string Textures { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output bundle file.")]
		public string OutFile { get; set; }

		public void Execute()
		{
			var shaders = Shaders.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			var fonts = Fonts.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
			var textures = Textures.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

			// bundles with the same sequence of asset types should have the same hash
			byte[] hash;
			using (var cryptoProvider = new MD5CryptoServiceProvider())
				hash = cryptoProvider.ComputeHash(Encoding.UTF8.GetBytes($"{shaders.Length}-{fonts.Length}-{textures.Length}"));

			var content = new MemoryStream();
			var files = shaders.Concat(fonts).Concat(textures);
			foreach (var fileContent in files.Select(File.ReadAllBytes))
				content.Write(fileContent, 0, fileContent.Length);

			var contentArray = content.ToArray();
			byte[] compressedContent;
			using (var output = new MemoryStream())
			{
				using (var zip = new GZipStream(output, CompressionMode.Compress, true))
					zip.Write(contentArray, 0, contentArray.Length);
				compressedContent = output.ToArray();
			}

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			{
				writer.Write(hash);
				writer.Write(contentArray.Length);
				writer.Write(compressedContent.Length);
				writer.Write(compressedContent);
			}
		}
	}
}