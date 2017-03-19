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
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;
	using Memory;
	using Utilities;

	/// <summary>
	///   Captures all generated logs and outputs them to a log file.
	/// </summary>
	internal class LogFile : DisposableObject
	{
		/// <summary>
		///   The number of log messages that must be queued before the messages are written to the file system.
		/// </summary>
		private const int BatchSize = 200;

		/// <summary>
		///   A cached string builder used to write the log file.
		/// </summary>
		private static readonly StringBuilder _builder = new StringBuilder();

		/// <summary>
		///   The name of the generated log file.
		/// </summary>
		private readonly string _fileName = $"{Application.Name}.log";

		/// <summary>
		///   The unwritten log entries that have been generated.
		/// </summary>
		private readonly Queue<LogEntry> _logEntries = new Queue<LogEntry>();

		/// <summary>
		///   The task that writes the log entries to the file system.
		/// </summary>
		private Task _writeTask;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public LogFile()
		{
			Log.OnLog += Enqueue;

			// Delete any previously created log file
			FileSystem.Delete(_fileName);
		}

		/// <summary>
		///   Gets the path of the log file.
		/// </summary>
		public string FilePath => Path.Combine(FileSystem.UserDirectory, _fileName).Replace("\\", "/");

		/// <summary>
		///   Enqueues the given log entry.
		/// </summary>
		/// <param name="entry">The log entry that should be enqueued.</param>
		internal void Enqueue(LogEntry entry)
		{
			_logEntries.Enqueue(entry);
			WriteToFile();
		}

		/// <summary>
		///   Writes the generated log messages into the log file.
		/// </summary>
		/// <param name="force">If true, all unwritten messages are written; otherwise, writes are batched to improve performance.</param>
		internal void WriteToFile(bool force = false)
		{
			if (!force && _logEntries.Count < BatchSize)
				return;

			if (_logEntries.Count == 0)
				return;

			try
			{
				WaitForCompletion();

				var logEntries = _logEntries.ToArray();
				_logEntries.Clear();

				_writeTask = Task.Run(() => FileSystem.AppendText(_fileName, ToString(logEntries)));

				if (force)
					WaitForCompletion();
			}
			catch (Exception e)
			{
				Log.Error($"Failed to append to log file: {e.Message}");
			}
		}

		/// <summary>
		///   Waits for the completion of the write task. Any exceptions that have been thrown during the execution of the write task
		///   are collected into a new exception.
		/// </summary>
		private void WaitForCompletion()
		{
			if (_writeTask == null)
				return;

			try
			{
				if (!_writeTask.IsCompleted)
					_writeTask.Wait();
				else if (_writeTask.Exception != null)
					throw _writeTask.Exception;
			}
			catch (AggregateException e)
			{
				throw new InvalidOperationException(String.Join("\n", e.InnerExceptions.Select(inner => inner.Message)));
			}
			finally
			{
				_writeTask = null;
			}
		}

		/// <summary>
		///   Generates the string representation for the given log entries.
		/// </summary>
		/// <param name="logEntries">The log entries that should be converted to a string.</param>
		private static string ToString(LogEntry[] logEntries)
		{
			_builder.Clear();

			foreach (var entry in logEntries)
			{
				_builder.Append("[");
				_builder.Append(entry.LogTypeString);
				_builder.Append("]   ");
				_builder.Append(entry.Time.ToString("F4").PadLeft(9));

				_builder.Append("s   ");
				TextString.Write(_builder, entry.Message);
				_builder.Append("\n");
			}

			return _builder.ToString();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Log.OnLog -= Enqueue;
			WriteToFile(true);
		}
	}
}