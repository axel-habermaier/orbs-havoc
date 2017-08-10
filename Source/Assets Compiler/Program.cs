namespace AssetsCompiler
{
	using System;
	using System.Globalization;
	using CommandLine;
	using CommandLine.Text;
	using JetBrains.Annotations;

	internal static class Program
	{
		public static int Main(string[] args)
		{
			try
			{
				CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
				CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

				CompilationTask compilationTask = null;
				var options = new Options();

				if (!Parser.Default.ParseArguments(args, options, (verb, parsedCommand) => compilationTask = (CompilationTask)parsedCommand))
					return -1;

				compilationTask.Run();
				return 0;
			}
			catch (Exception e)
			{
				Console.WriteLine("A fatal compilation error occurred:");
				Console.WriteLine("{0}", e.Message);
				Console.WriteLine("{0}", e.StackTrace);

				if (e.InnerException != null)
				{
					Console.WriteLine("Inner exception:");
					Console.WriteLine("{0}", e.InnerException.Message);
					Console.WriteLine("{0}", e.InnerException.StackTrace);
				}

				return -1;
			}
		}

		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private class Options
		{
			[VerbOption("shader", HelpText = "Compiles a shader.")]
			public ShaderCompiler ShaderCompiler { get; set; }

			[VerbOption("registry", HelpText = "Generates registry code.")]
			public RegistryGenerator RegistryGenerator { get; set; }

			[VerbOption("texture", HelpText = "Compiles a texture.")]
			public TextureCompiler TextureCompiler { get; set; }

			[VerbOption("cursor", HelpText = "Compiles a cursor.")]
			public CursorCompiler CursorCompiler { get; set; }

			[VerbOption("font", HelpText = "Compiles a font.")]
			public FontCompiler FontCompiler { get; set; }

			[VerbOption("level", HelpText = "Compiles a level.")]
			public LevelCompiler LevelCompiler { get; set; }

			[VerbOption("bundle", HelpText = "Compiles a bundle.")]
			public BundleCompiler BundleCompiler { get; set; }

			[VerbOption("opengl", HelpText = "Generates OpenGL bindings.")]
			public GLGenerator GLGenerator { get; set; }

			[HelpVerbOption]
			public string GetUsage(string verb)
			{
				return HelpText.AutoBuild(this, verb);
			}
		}
	}
}