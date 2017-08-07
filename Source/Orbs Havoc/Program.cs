namespace OrbsHavoc
{
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Reflection;
	using System.Runtime.InteropServices;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using JetBrains.Annotations;
	using Platform;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using Utilities;

	internal static class Program
	{
		private static void Main(string[] arguments)
		{
			SetErrorMode(GetErrorMode() | ErrorMode.NoGpFaultErrorBox);

			try
			{
				using (var logFile = new LogFile())
				{
					Thread.CurrentThread.Name = "Main Thread";
					TaskScheduler.UnobservedTaskException += (o, e) => ReportException(e.Exception.InnerException, logFile.FilePath);
					AppDomain.CurrentDomain.UnhandledException += (o, e) => ReportException(e.ExceptionObject as Exception, logFile.FilePath);
					CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
					CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

					LogEntryCache.EnableCaching();
					Log.OnLog += WriteToConsole;

					Log.Info($"Starting {Application.Name}...");
					Log.Info($"User file directory: {UserFile.UserDirectory}");

					Cvars.Initialize();
					Commands.Initialize();

					try
					{
						using (new Help())
						using (new Interpreter())
						{
							// Process the autoexec.cfg first, then the command line, so that cvar values set via the 
							// command line overwrite the autoexec.cfg.
							ConfigurationFile.Process(ConfigurationFile.AutoExec, executedByUser: false);
							CommandLine.Process(arguments);

							using (SDL2.Initialize())
								Application.Run();

							ConfigurationFile.WriteAutoExec();
							Log.Info($"{Application.Name} has shut down.");
						}
					}
					catch (Exception e)
					{
						ReportException(e, logFile.FilePath);
					}
				}
			}
			finally
			{
				ObjectPool.DisposeGlobalPools();
			}
		}

		private static void ReportException(Exception exception, string logFilePath)
		{
			if (exception == null)
				return;

			while (exception is TargetInvocationException || exception is TypeInitializationException)
				exception = exception.InnerException;

			Log.Error($"Exception type: {exception.GetType().FullName}");
			Log.Error($"Exception message: {exception.Message}");
			Log.Error($"Stack trace:\n{exception.StackTrace}");

			var message = "The application has been terminated after a fatal error. " +
						  "See the log file for further details.\n\nThe error was: {0}\n\nLog file: {1}";
			message = String.Format(message, exception.Message, logFilePath);

			Log.ShowErrorBox(Application.Name, message);
			Log.Error("The application has been terminated after a fatal error.");
		}

		private static void WriteToConsole(LogEntry entry)
		{
#if DEBUG
			var builder = new StringBuilder();
			TextString.Write(builder, entry.Message);

			Debug.WriteLine("[{0}] {1}", entry.LogTypeString, builder);
#else
			var output = System.Console.Out;

			output.Write("[");
			output.Write(entry.LogTypeString);
			output.Write("] ");

			TextString.Write(output, entry.Message);
			output.WriteLine();
#endif
		}

		[DllImport("kernel32.dll")]
		private static extern ErrorMode SetErrorMode(ErrorMode mode);

		[DllImport("kernel32.dll")]
		private static extern ErrorMode GetErrorMode();

		[Flags]
		[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
		private enum ErrorMode : uint
		{
			SystemDefault = 0x0,
			FailCriticalErrors = 0x0001,
			NoAlignmentFaultExcept = 0x0004,
			NoGpFaultErrorBox = 0x0002,
			NoOpenFileErrorBox = 0x8000
		}
	}
}