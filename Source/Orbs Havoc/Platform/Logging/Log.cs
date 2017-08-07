namespace OrbsHavoc.Platform.Logging
{
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using JetBrains.Annotations;
	using Utilities;

	/// <summary>
	///     Provides functions to log fatal errors, errors, warnings, informational messages, and debug-time only
	///     informational messages. An event is raised whenever one of these functions is invoked.
	/// </summary>
	public static unsafe class Log
	{
		/// <summary>
		///     The object used for thread synchronization.
		/// </summary>
		private static readonly object _lockObject = new object();

		/// <summary>
		///     Raised when a log message has been generated.
		/// </summary>
		public static event Action<LogEntry> OnLog;

		/// <summary>
		///     Raises the OnFatalError event with the given message and terminates the application by throwing
		///     an InvalidOperationException.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnFatalError event.</param>
		[DebuggerHidden, ContractAnnotation("=> halt")]
		public static void Die(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			RaiseEvent(LogType.Fatal, message);
			throw new FatalErrorException(message);
		}

		/// <summary>
		///     Raises the OnError event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnError event.</param>
		public static void Error(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Error, message);
		}

		/// <summary>
		///     Raises the OnWarning event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnWarning event.</param>
		public static void Warn(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Warning, message);
		}

		/// <summary>
		///     Raises the OnInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnInfo event.</param>
		public static void Info(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Info, message);
		}

		/// <summary>
		///     In debug builds, raises the OnDebugInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnDebugInfo event.</param>
		[Conditional("DEBUG")]
		public static void Debug(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Debug, message);
		}

		/// <summary>
		///     In debug builds, raises the OnDebugInfo event with the given message if the given condition is true.
		/// </summary>
		/// <param name="condition">The condition that must be true for the message to be displayed.</param>
		/// <param name="message">The message that should be passed as an argument of the OnDebugInfo event.</param>
		[Conditional("DEBUG")]
		public static void DebugIf(bool condition, string message)
		{
			if (condition)
				Debug(message);
		}

		/// <summary>
		///     Raises the OnLog event.
		/// </summary>
		private static void RaiseEvent(LogType logType, string message)
		{
			lock (_lockObject)
				OnLog?.Invoke(new LogEntry(logType, message));
		}

		/// <summary>
		///     Shows a message box with the given header and message.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message that the message box should display.</param>
		public static void ShowErrorBox(string title, string message)
		{
			MessageBox(null, message, title, 0x10);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern uint MessageBox(void* hWnd, string text, string caption, int options);
	}
}