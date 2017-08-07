namespace OrbsHavoc.Platform.Logging
{
	using System;
	using System.Collections.Concurrent;
	using System.IO;
	using System.Text;
	using System.Threading;
	using Memory;
	using Utilities;

	internal class LogFile : DisposableObject
	{
		private readonly string _fileName = $"{Application.Name}.log";
		private readonly BlockingCollection<LogEntry> _logEntries = new BlockingCollection<LogEntry>();
		private readonly Thread _thread;
		private readonly TimeSpan _writeInterval = TimeSpan.FromSeconds(1);

		public LogFile()
		{
			try
			{
				UserFile.Delete(_fileName);
			}
			catch (Exception e)
			{
				Log.Warn($"Failed to delete old log file: {e.Message.EnsureEndsWithDot()}");
			}

			Log.OnLog += Enqueue;

			_thread = new Thread(WriteToFile);
			_thread.Start();
		}

		public string FilePath => Path.Combine(UserFile.UserDirectory, _fileName).Replace("\\", "/");

		private void Enqueue(LogEntry entry)
		{
			_logEntries.Add(entry);
		}

		private void WriteToFile()
		{
			var builder = new StringBuilder();
			while (!_logEntries.IsCompleted)
			{
				Thread.Sleep(_writeInterval);

				builder.Clear();
				WriteQueuedLogEntries(builder);

				UserFile.AppendText(_fileName, builder.ToString());
			}
		}

		private void WriteQueuedLogEntries(StringBuilder builder)
		{
			while (_logEntries.TryTake(out var logEntry))
			{
				builder.Append("[");
				builder.Append(logEntry.LogTypeString);
				builder.Append("]   ");
				builder.Append(logEntry.Time.ToString("F4").PadLeft(9));

				builder.Append("s   ");
				TextString.Write(builder, logEntry.Message);
				builder.Append("\n");
			}
		}

		protected override void OnDisposing()
		{
			Log.OnLog -= Enqueue;

			_logEntries.CompleteAdding();
			_thread.Join();
		}
	}
}