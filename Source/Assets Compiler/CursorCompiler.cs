namespace AssetsCompiler
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.IO;
	using CommandLine;
	using JetBrains.Annotations;

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public class CursorCompiler : CompilationTask
	{
		[Option("input", Required = true, HelpText = "The path to the input cursor file.")]
		public string InFile { get; set; }

		[Option("output", Required = true, HelpText = "The path to the output cursor file.")]
		public string OutFile { get; set; }

		[Option("hotspotX", Required = true, HelpText = "The X value of the hot spot.")]
		public int HotSpotX { get; set; }

		[Option("hotspotY", Required = true, HelpText = "The Y value of the hot spot.")]
		public int HotSpotY { get; set; }

		protected override string GeneratedFile => OutFile;

		protected override unsafe void Execute()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(OutFile));

			using (var stream = File.Create(OutFile))
			using (var writer = new BinaryWriter(stream))
			using (var bitmap = (Bitmap)Image.FromFile(InFile))
			{
				if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
					throw new InvalidOperationException($"Cursor '{InFile}' must be in 32bit RGBA format.");

				var length = bitmap.Width * bitmap.Height * 4;
				var rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
				var imageData = bitmap.LockBits(rectangle, ImageLockMode.ReadOnly, bitmap.PixelFormat);
				var sourceData = (byte*)imageData.Scan0;

				writer.Write(HotSpotX);
				writer.Write(HotSpotY);
				writer.Write((uint)bitmap.Width);
				writer.Write((uint)bitmap.Height);
				writer.Write(length);

				// Switch from BGRA to RGBA
				for (var i = 0; i < length; i += 4)
				{
					var b = sourceData[i];
					var g = sourceData[i + 1];
					var r = sourceData[i + 2];
					var a = sourceData[i + 3];

					writer.Write(r);
					writer.Write(g);
					writer.Write(b);
					writer.Write(a);
				}

				bitmap.UnlockBits(imageData);
			}
		}
	}
}