namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Platform;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;
	using Views.UI;

	internal sealed class ConsoleHistory : DisposableObject
	{
		private const int MaxHistory = 64;
		private const string HistoryFileName = "console.txt";

		private readonly List<string> _history;
		private int _historyIndex;

		public ConsoleHistory()
		{
			try
			{
				_history = UserFile.ReadAllText(HistoryFileName)
									 .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
									 .Select(h => h.Truncate(ConsoleUI.MaxLogEntryLength))
									 .Take(MaxHistory)
									 .ToList();
			}
			catch (Exception e)
			{
				_history = new List<string>();
				Log.Error($"Failed to load console history: {e.Message}");
			}

			ResetSelectedHistoryEntry();
		}

		public event Action<string> CurrentHistoryEntryChanged;

		protected override void OnDisposing()
		{
			var builder = new StringBuilder();
			foreach (var history in _history)
				builder.Append(history).Append("\n");

			try
			{
				UserFile.WriteAllText(HistoryFileName, builder.ToString());
				Log.Info("Console history has been persisted.");
			}
			catch (Exception e)
			{
				Log.Error($"Failed to persist console history: {e.Message}");
			}
		}

		public void AddToHistory(string entry)
		{
			// Store the input in the input history, if it differs from the last entry
			if (_history.Count > 0 && _history[0] == entry)
				return;

			// If the history is full, remove the oldest entry
			if (_history.Count == MaxHistory)
				_history.RemoveAt(_history.Count - 1);

			_history.Insert(0, entry);
			ResetSelectedHistoryEntry();
		}

		public void SelectNewerHistoryEntry()
		{
			ShowHistoryEntry(_historyIndex - 1);
		}

		public void SelectOlderHistoryEntry()
		{
			ShowHistoryEntry(_historyIndex + 1);
		}

		public void ResetSelectedHistoryEntry()
		{
			_historyIndex = -1;
		}

		private void ShowHistoryEntry(int index)
		{
			if (_history.Count == 0)
				return;

			// Ensure the index is inside the bounds
			if (index >= _history.Count)
				index = _history.Count - 1;
			else
				CurrentHistoryEntryChanged?.Invoke(index < 0 ? String.Empty : _history[index]);

			_historyIndex = index < 0 ? -1 : index;
		}
	}
}