namespace AssetsCompiler
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using CommandLine;
	using JetBrains.Annotations;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class TextureCompiler : CompilationTask
	{
		[Option("input", Required = true, HelpText = "The path to the input texture file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output texture file.")]
		public string OutFile { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			using (var bitmap = (Bitmap)Image.FromFile(InFile))
			{
				Compile(writer, bitmap);
			}
		}

		public static unsafe void Compile(BinaryWriter writer, Bitmap bitmap)
		{
			var length = bitmap.Width * bitmap.Height * 4;
			var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			var imageData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			var sourceData = (byte*)imageData.Scan0;

			writer.Write((uint)bitmap.Width);
			writer.Write((uint)bitmap.Height);
			writer.Write(length);

			switch (bitmap.PixelFormat)
			{
				case PixelFormat.Format24bppRgb:
				{
					// Switch from BGR to RGBA and to premultiplied alpha.
					for (int i = 0, j = 0; j < length; i += 3, j += 4)
					{
						var b = sourceData[i];
						var g = sourceData[i + 1];
						var r = sourceData[i + 2];

						writer.Write(r);
						writer.Write(g);
						writer.Write(b);
						writer.Write((byte)255);
					}
					break;
				}
				case PixelFormat.Format32bppArgb:
				{
					// Switch from BGRA to RGBA and to premultiplied alpha.
					for (var i = 0; i < length; i += 4)
					{
						var b = sourceData[i] / 255.0f;
						var g = sourceData[i + 1] / 255.0f;
						var r = sourceData[i + 2] / 255.0f;
						var a = sourceData[i + 3] / 255.0f;

						writer.Write((byte)(r * a * 255.0f));
						writer.Write((byte)(g * a * 255.0f));
						writer.Write((byte)(b * a * 255.0f));
						writer.Write((byte)(a * 255.0f));
					}
					break;
				}
				default:
					throw new InvalidOperationException("Unsupported pixel format.");
			}

			bitmap.UnlockBits(imageData);
		}
	}
}