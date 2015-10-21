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

namespace PointWars
{
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Threading;
	using System.Threading.Tasks;
	using Platform;
	using Platform.Logging;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents the application.
	/// </summary>
	internal static class Program
	{
		/// <summary>
		///   Gets a value indicating whether the program was successfully initialized.
		/// </summary>
		public static bool Initialized { get; private set; }

		/// <summary>
		///   The entry point of the application.
		/// </summary>
		/// <param name="arguments">The command line arguments passed to the application.</param>
		private static void Main(string[] arguments)
		{
			Thread.CurrentThread.Name = "Main Thread";
			TaskScheduler.UnobservedTaskException += (o, e) => { throw e.Exception.InnerException; };
			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
			CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

			using (var platform = new PlatformLibrary())
			using (var logFile = new LogFile())
			{
				PrintToConsole();
				Log.Info("Starting {0}...", Application.Name);

				Cvars.Initialize();
				Commands.Initialize();

				try
				{
					using (new Help())
					using (new Interpreter())
					{

						// Process the autoexec.cfg first, then the command line, so that cvar values set via the 
						// command line overwrite the autoexec.cfg. We'll set all cvars now and execute all commands
						// later when the app is fully initialized.
						ConfigurationFile.Process(ConfigurationFile.AutoExec, executedByUser: false);
						CommandLine.Process(arguments);

						platform.Initialize();

						Initialized = true;
						var app = new Application();
						app.Run();

						ConfigurationFile.WriteAutoExec();
						Log.Info("{0} has shut down.", Application.Name);
					}
				}
				catch (Exception e)
				{
					ReportException(e, logFile);
				}
			}
		}

		/// <summary>
		///   Reports the given exception.
		/// </summary>
		/// <param name="exception">The exception that should be reported.</param>
		/// <param name="logFile">The log file the exception should be reported to.</param>
		private static void ReportException(Exception exception, LogFile logFile)
		{
			var message = "The application has been terminated after a fatal error. " +
						  "See the log file for further details.\n\nThe error was: {0}\n\nLog file: {1}";

			if (exception is TargetInvocationException || exception is TypeInitializationException)
				exception = exception.InnerException;

			logFile.Enqueue(new LogEntry(LogType.Error, $"Exception type: {exception.GetType().FullName}"));
			logFile.Enqueue(new LogEntry(LogType.Error, $"Exception message: {exception.Message}"));
			logFile.Enqueue(new LogEntry(LogType.Error, $"Stack trace:\n{exception.StackTrace}"));
			logFile.WriteToFile(force: true);

			message = String.Format(message, exception.Message, logFile.FilePath);
			Log.ShowErrorBox(Application.Name, message);

			if (!(exception is FatalErrorException))
			{
				Log.Error("Exception type: {0}", exception.GetType().FullName);
				Log.Error("Exception message: {0}", exception.Message);
			}

			Log.Error("The application has been terminated after a fatal error. The log file is located at '{0}'.", logFile.FilePath);
		}

		/// <summary>
		///   Wires up the log events to write all logged messages to the console.
		/// </summary>
		private static void PrintToConsole()
		{
			Log.OnFatalError += WriteToConsole;
			Log.OnError += WriteToConsole;
			Log.OnWarning += WriteToConsole;
			Log.OnInfo += WriteToConsole;
			Log.OnDebugInfo += WriteToConsole;
		}

		/// <summary>
		///   Writes the given log entry to the given text writer.
		/// </summary>
		/// <param name="entry">The log entry that should be written.</param>
		private static void WriteToConsole(LogEntry entry)
		{
#if DEBUG
			Debug.WriteLine("[{0}] {1}", entry.LogTypeString, entry.Message);
#else
			Console.WriteLine("[{0}] {1}", entry.LogTypeString, entry.Message);
#endif
		}
	}
}