namespace OrbsHavoc.Views
{
	using System;
	using System.Collections.Concurrent;
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
			if (!TextString.IsNullOrWhiteSpace(UI.Prompt.Text))
			{
				_history.AddToHistory(UI.Prompt.Text);

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