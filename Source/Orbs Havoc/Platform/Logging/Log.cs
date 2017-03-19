// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Platform.Logging
{
	using System;
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using JetBrains.Annotations;
	using Utilities;
	using static SDL2;

	/// <summary>
	///   Provides functions to log fatal errors, errors, warnings, informational messages, and debug-time only
	///   informational messages. An event is raised whenever one of these functions is invoked.
	/// </summary>
	public static unsafe class Log
	{
		/// <summary>
		///   The object used for thread synchronization.
		/// </summary>
		private static readonly object _lockObject = new object();

		/// <summary>
		///   Raised when a log message has been generated.
		/// </summary>
		public static event Action<LogEntry> OnLog;

		/// <summary>
		///   Raises the OnFatalError event with the given message and terminates the application by throwing
		///   an InvalidOperationException.
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
		///   Raises the OnError event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnError event.</param>
		public static void Error(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Error, message);
		}

		/// <summary>
		///   Raises the OnWarning event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnWarning event.</param>
		public static void Warn(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Warning, message);
		}

		/// <summary>
		///   Raises the OnInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnInfo event.</param>
		public static void Info(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Info, message);
		}

		/// <summary>
		///   In debug builds, raises the OnDebugInfo event with the given message.
		/// </summary>
		/// <param name="message">The message that should be passed as an argument of the OnDebugInfo event.</param>
		[Conditional("DEBUG")]
		public static void Debug(string message)
		{
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));
			RaiseEvent(LogType.Debug, message);
		}

		/// <summary>
		///   In debug builds, raises the OnDebugInfo event with the given message if the given condition is true.
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
		///   Raises the OnLog event.
		/// </summary>
		private static void RaiseEvent(LogType logType, string message)
		{
			lock (_lockObject)
			{
				OnLog?.Invoke(new LogEntry(logType, message));
			}
		}

		/// <summary>
		///   Shows an OS-specific message box with the given header and message.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message that the message box should display.</param>
		public static void ShowErrorBox(string title, string message)
		{
			try
			{
				// Try to show a native Windows message box (which looks much better than the SDL2 one);
				// if that fails because we're running on Linux, for instance, fall back to the SDL2 message box
				MessageBox(null, message, title, 0x10);
			}
			catch (DllNotFoundException)
			{
				using (var titlePtr = Interop.ToPointer(title))
				using (var messagePtr = Interop.ToPointer(message))
				{
					if (SDL_WasInit(SDL_INIT_VIDEO) != 0 && SDL_ShowSimpleMessageBox(SDL_MESSAGEBOX_ERROR, titlePtr, messagePtr, null) != 0)
						Error($"Failed to show message box: {SDL_GetError()}");
				}
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern uint MessageBox(void* hWnd, string text, string caption, int options);
	}
}