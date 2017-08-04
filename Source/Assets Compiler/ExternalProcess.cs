namespace AssetsCompiler
{
	using System;
	using System.Diagnostics;
	using System.IO;

	/// <summary>
	///   Represents external process.
	/// </summary>
	internal class ExternalProcess : IDisposable
	{
		/// <summary>
		///   The external process.
		/// </summary>
		private readonly Process _process;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="fileName">The file name of the external executable.</param>
		/// <param name="commandLine">The command line arguments that should be passed to the executable.</param>
		public ExternalProcess(string fileName, string commandLine = "")
		{
			_process = new Process
			{
				EnableRaisingEvents = true,
				StartInfo = new ProcessStartInfo(fileName, commandLine)
				{
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				}
			};

			_process.OutputDataReceived += (o, e) => LogMessage(e.Data);
			_process.ErrorDataReceived += (o, e) => LogMessage(e.Data);
		}

		/// <summary>
		///   Gets or sets the process' working directory.
		/// </summary>
		public string WorkingDirectory
		{
			get => _process.StartInfo.WorkingDirectory;
			set => _process.StartInfo.WorkingDirectory = value;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_process.Dispose();
		}

		/// <summary>
		///   Adds the message to the log entry queue.
		/// </summary>
		/// <param name="message">The message that should be added.</param>
		private void LogMessage(string message)
		{
			if (String.IsNullOrWhiteSpace(message))
				return;

			Console.WriteLine("{0}: {1}", Path.GetFileName(_process.StartInfo.FileName), message);
		}

		/// <summary>
		///   Runs the process.
		/// </summary>
		public int Run()
		{
			_process.Start();

			_process.BeginErrorReadLine();
			_process.BeginOutputReadLine();

			_process.WaitForExit();
			return _process.ExitCode;
		}
	}
}