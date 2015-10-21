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

namespace PointWars.Platform.Logging
{
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using JetBrains.Annotations;
	using Utilities;

	/// <summary>
	///   Provides functions to log fatal errors, errors, warnings, informational messages, and debug-time only
	///   informational messages. An event is raised whenever one of these functions is invoked.
	/// </summary>
	public static unsafe class Log
	{
		/// <summary>
		///   The object used for thread synchronization.
		/// </summary>
		private static readonly object LockObject = new object();

		/// <summary>
		///   Raised when a fatal error occurred. Typically, the program terminates after all event handlers have
		///   been executed.
		/// </summary>
		public static event Action<LogEntry> OnFatalError;

		/// <summary>
		///   Raised when an error occurred.
		/// </summary>
		public static event Action<LogEntry> OnError;

		/// <summary>
		///   Raised when a warning was generated.
		/// </summary>
		public static event Action<LogEntry> OnWarning;

		/// <summary>
		///   Raised when an informational message was generated.
		/// </summary>
		public static event Action<LogEntry> OnInfo;

		/// <summary>
		///   Raised when a debug informational message was generated.
		/// </summary>
		public static event Action<LogEntry> OnDebugInfo;

		/// <summary>
		///   Raises the OnFatalError event with the given message and terminates the application by throwing
		///   an InvalidOperationException.
		/// </summary>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnFatalError event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[DebuggerHidden, StringFormatMethod("message"), ContractAnnotation("=> halt")]
		public static void Die(string message, params object[] arguments)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			lock (LockObject)
			{
				var formattedMessage = String.Format(message, arguments);
				OnFatalError?.Invoke(new LogEntry(LogType.Fatal, formattedMessage));

				throw new FatalErrorException(formattedMessage);
			}
		}

		/// <summary>
		///   Raises the OnError event with the given message.
		/// </summary>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnError event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[StringFormatMethod("message")]
		public static void Error(string message, params object[] arguments)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			lock (LockObject)
			{
				OnError?.Invoke(new LogEntry(LogType.Error, String.Format(message, arguments)));
			}
		}

		/// <summary>
		///   Raises the OnWarning event with the given message.
		/// </summary>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnWarning event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[StringFormatMethod("message")]
		public static void Warn(string message, params object[] arguments)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			lock (LockObject)
			{
				OnWarning?.Invoke(new LogEntry(LogType.Warning, String.Format(message, arguments)));
			}
		}

		/// <summary>
		///   Raises the OnInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnInfo event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[StringFormatMethod("message")]
		public static void Info(string message, params object[] arguments)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			lock (LockObject)
			{
				OnInfo?.Invoke(new LogEntry(LogType.Info, String.Format(message, arguments)));
			}
		}

		/// <summary>
		///   In debug builds, raises the OnDebugInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnDebugInfo event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[Conditional("DEBUG"), StringFormatMethod("message")]
		public static void Debug(string message, params object[] arguments)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			lock (LockObject)
			{
				OnDebugInfo?.Invoke(new LogEntry(LogType.Debug, String.Format(message, arguments)));
			}
		}

		/// <summary>
		///   In debug builds, raises the OnDebugInfo event with the given message if the given condition is true.
		/// </summary>
		/// <param name="condition">The condition that must be true for the message to be displayed.</param>
		/// <param name="message">The message that should be formatted and passed as an argument of the OnDebugInfo event.</param>
		/// <param name="arguments">The arguments that should be copied into the message.</param>
		[Conditional("DEBUG"), StringFormatMethod("message")]
		public static void DebugIf(bool condition, string message, params object[] arguments)
		{
			if (condition)
				Debug(message, arguments);
		}

		/// <summary>
		///   Shows an OS-specific message box with the given header and message.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message that the message box should display.</param>
		public static void ShowErrorBox(string title, string message)
		{
			if (PlatformInfo.Platform == PlatformType.Windows)
				MessageBox(null, message, title, 0x10);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern uint MessageBox(void* hWnd, string text, string caption, int options);
	}
}