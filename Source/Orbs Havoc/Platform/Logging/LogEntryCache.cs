namespace OrbsHavoc.Platform.Logging
{
	using System.Collections.Generic;

	/// <summary>
	///   Caches log entries.
	/// </summary>
	public static class LogEntryCache
	{
		/// <summary>
		///   The list of cached log entries.
		/// </summary>
		public static List<LogEntry> LogEntries { get; private set; }

		/// <summary>
		///   Enables log entry caching.
		/// </summary>
		public static void EnableCaching()
		{
			LogEntries = new List<LogEntry>();
			Log.OnLog += Add;
		}

		/// <summary>
		///   Disables log entry caching.
		/// </summary>
		public static void DisableCaching()
		{
			LogEntries = null;
			Log.OnLog -= Add;
		}

		/// <summary>
		///   Adds the log entry to the cache.
		/// </summary>
		private static void Add(LogEntry entry)
		{
			LogEntries.Add(entry);
		}
	}
}