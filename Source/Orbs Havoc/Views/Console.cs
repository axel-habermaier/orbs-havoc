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

namespace OrbsHavoc.Views
{
	using System;
	using System.Collections.Concurrent;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using UI;
	using UserInterface.Input;
	using Utilities;

	internal sealed class Console : View<ConsoleUI>
	{
		private readonly ConsoleAutoCompletion _autoCompletion = new ConsoleAutoCompletion();
		private readonly ConsoleHistory _history = new ConsoleHistory();

		/// <summary>
		///     The queued log entries that have not yet been added to the console. Log entries can be generated on any thread, but only
		///     the main thread is allowed to actually change the console's labels.
		/// </summary>
		private readonly ConcurrentQueue<LogEntry> _queuedLogEntries = new ConcurrentQueue<LogEntry>();

		private bool _settingAutoCompletedInput;

		public Console()
		{
			_history.CurrentHistoryEntryChanged += ShowHistoryEntry;
			_autoCompletion.InputAutoCompleted += AutoComplete;
		}

		protected override void Activate()
		{
			ClearInput();
		}

		public override void Initialize()
		{
			foreach (var logEntry in LogEntryCache.LogEntries)
				AddLogEntry(logEntry);

			LogEntryCache.DisableCaching();

			Commands.OnShowConsole += ShowConsole;
			Log.OnLog += AddLogEntry;
		}

		public override void InitializeUI()
		{
			base.InitializeUI();

			UI.Prompt.TextChanged += OnTextChanged;
			UI.Prompt.InputBindings.AddRange(
				new KeyBinding(ClearInput, Key.Escape),
				new KeyBinding(_history.SelectNewerHistoryEntry, Key.Down) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(_history.SelectOlderHistoryEntry, Key.Up) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(() => _autoCompletion.Next(UI.Prompt.Text), Key.Tab) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(() => _autoCompletion.Previous(UI.Prompt.Text), Key.Tab, KeyModifiers.Shift) { TriggerMode = TriggerMode.Repeatedly }
			);

			UI.InputBindings.AddRange(
				new KeyBinding(SubmitInput, Key.Enter),
				new KeyBinding(SubmitInput, Key.NumpadEnter)
			);
		}

		public override void Update()
		{
			UI.Update(Window.Size);

			// Add all queued log entries to the console now that we're on the main thread
			while (_queuedLogEntries.TryDequeue(out var logEntry))
				UI.AddLogEntry(logEntry);
		}

		protected override void OnDisposing()
		{
			Commands.OnShowConsole -= ShowConsole;
			Log.OnLog -= AddLogEntry;

			_history.SafeDispose();
		}

		private void SubmitInput()
		{
			_history.AddToHistory(UI.Prompt.Text);

			if (!TextString.IsNullOrWhiteSpace(UI.Prompt.Text))
			{
				Log.Info(ConsoleUI.PromptToken + UI.Prompt.Text);
				Commands.Execute(UI.Prompt.Text);

				// Show the result of the user's input
				UI.ScrollViewer.ScrollToBottom();
			}

			ClearInput();
		}

		private void ClearInput()
		{
			UI.Prompt.Text = String.Empty;

			_history.ResetSelectedHistoryEntry();
			_autoCompletion.Reset();
		}

		private void OnTextChanged(string text)
		{
			// Reset the auto completion list if an input was made other than setting the current completion value
			if (!_settingAutoCompletedInput)
				_autoCompletion.Reset();
		}

		private void ShowConsole(bool show)
		{
			if (show)
				Show();
			else
				Hide();
		}

		private void AddLogEntry(LogEntry logEntry)
		{
			_queuedLogEntries.Enqueue(logEntry);
		}

		private void AutoComplete(string entry)
		{
			_settingAutoCompletedInput = true;
			UI.Prompt.Text = entry;
			_history.ResetSelectedHistoryEntry();
			_settingAutoCompletedInput = false;
		}

		private void ShowHistoryEntry(string entry)
		{
			UI.Prompt.Text = entry;
			_autoCompletion.Reset();
		}
	}
}