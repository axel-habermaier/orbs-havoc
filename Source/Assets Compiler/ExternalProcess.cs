// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using System.Diagnostics;
	using System.IO;
	using JetBrains.Annotations;

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
		/// <param name="arguments">The arguments that should be copied into the command line.</param>
		[StringFormatMethod("commandLine")]
		public ExternalProcess(string fileName, string commandLine = "", params object[] arguments)
		{
			_process = new Process
			{
				EnableRaisingEvents = true,
				StartInfo = new ProcessStartInfo(fileName, String.Format(commandLine, arguments))
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
			get { return _process.StartInfo.WorkingDirectory; }
			set { _process.StartInfo.WorkingDirectory = value; }
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