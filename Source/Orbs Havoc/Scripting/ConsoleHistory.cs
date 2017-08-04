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
				_history = FileSystem.ReadAllText(HistoryFileName)
									 .Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)
									 .Select(h => h.Truncate(ConsoleUI.MaxLogEntryLength))
									 .Take(MaxHistory)
									 .ToList();
			}
			catch (FileSystemException e)
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
				FileSystem.WriteAllText(HistoryFileName, builder.ToString());
				Log.Info("Console history has been persisted.");
			}
			catch (FileSystemException e)
			{
				Log.Error($"Failed to persist console history: {e.Message}");
			}
		}

		public void AddToHistory(string entry)
		{
			if (String.IsNullOrWhiteSpace(entry))
				return;

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