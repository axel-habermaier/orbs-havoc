namespace AssetsCompiler
{
	using System;
	using System.IO;

	internal abstract class CompilationTask
	{
		protected abstract string GeneratedFile { get; }

		public void Run()
		{
			try
			{
				Execute();
			}
			catch (Exception)
			{
				File.Delete(GeneratedFile);
				throw;
			}
		}

		protected abstract void Execute();
	}
}