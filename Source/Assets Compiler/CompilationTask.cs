namespace AssetsCompiler
{
	using System;
	using System.IO;

	public abstract class CompilationTask
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