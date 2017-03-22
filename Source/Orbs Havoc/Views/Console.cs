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
	using System.Linq;
	using System.Text;
	using Platform;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///   Represents the console.
	/// </summary>
	internal sealed class Console : View
	{
		/// <summary>
		///   The maximum length of all console input or output.
		/// </summary>
		private const int MaxLength = 2048;

		/// <summary>
		///   The maximum number of labels that the console can display. If all labels are used and another label is
		///   added, the oldest label is removed.
		/// </summary>
		private const int MaxLabels = 2048;

		/// <summary>
		///   The maximum history size.
		/// </summary>
		private const int MaxHistory = 64;

		/// <summary>
		///   The prompt token.
		/// </summary>
		private const string PromptToken = "]";

		/// <summary>
		///   The name of the console history file.
		/// </summary>
		private const string HistoryFileName = "console.txt";

		private static readonly Color _errorColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		private static readonly Color _warningColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		private static readonly Color _infoColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		private static readonly Color _debugInfoColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);

		private readonly StackPanel _contentPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, MinWidth = 200 };
		private readonly string[] _history = new string[MaxHistory];
		private readonly TextBox _input = new TextBox { MaxLength = MaxLength, Dock = Dock.Bottom, AutoFocus = true };

		/// <summary>
		///   The queued log entries that have not yet been added to the console. Log entries can be generated on any thread, but only
		///   the main thread is allowed to actually change the console's labels.
		/// </summary>
		private readonly ConcurrentQueue<LogEntry> _queuedLogEntries = new ConcurrentQueue<LogEntry>();

		private int _autoCompletionIndex;
		private string[] _autoCompletionList;
		private int _historyIndex;
		private UIElement _layoutRoot;
		private int _numHistory;
		private ScrollViewer _scrollViewer;
		private bool _settingAutoCompletedInput;

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected override void Activate()
		{
			ClearInput();
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			foreach (var logEntry in LogEntryCache.LogEntries)
				AddLogEntry(logEntry);

			LogEntryCache.DisableCaching();

			Commands.OnShowConsole += ShowConsole;
			Log.OnLog += AddLogEntry;

			LoadHistory();
			InitializeUI();
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			_layoutRoot.Height = MathUtils.Round(Window.Size.Height / 2);

			// Make sure the text box never loses focus while the console is active
			_input.Focus();

			// Add all queued log entries to the console now that we're on the main thread
			while (_queuedLogEntries.TryDequeue(out var logEntry))
			{
				var color = _infoColor;
				switch (logEntry.LogType)
				{
					case LogType.Error:
						color = _errorColor;
						break;
					case LogType.Warning:
						color = _warningColor;
						break;
					case LogType.Debug:
						color = _debugInfoColor;
						break;
				}

				if (_contentPanel.Children.Count < MaxLabels)
				{
					_contentPanel.Children.Add(new Label
					{
						Text = logEntry.Message,
						Foreground = color,
						TextWrapping = TextWrapping.Wrap,
						Margin = new Thickness(0, 2, 0, 0)
					});
				}
				else
				{
					// If all labels are used, remove the oldest one by shifting the entire children collection up one
					// index and add the oldest label to the end of the collection (in order to re-use the label instance for 
					// the new message); this way, we only copy MaxLabels * ReferenceSize bytes and avoid relayouting all lines.

					var label = (Label)_contentPanel.Children[0];
					_contentPanel.Children.RemoveAt(0);

					label.Text = logEntry.Message;
					label.Foreground = color;
					_contentPanel.Children.Add(label);
				}
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnShowConsole -= ShowConsole;
			Log.OnLog -= AddLogEntry;

			var builder = new StringBuilder();
			for (var i = 0; i < _numHistory; ++i)
				builder.Append(_history[i]).Append("\n");

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

		/// <summary>
		///   Tries to load the console history.
		/// </summary>
		private void LoadHistory()
		{
			try
			{
				var content = FileSystem.ReadAllText(HistoryFileName);
				var history = content.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				history = history.Where(h => h.Length <= MaxLength).ToArray();

				var count = history.Length;
				var offset = 0;

				if (count > _history.Length)
				{
					offset = count - _history.Length;
					count = _history.Length;
				}

				Array.Copy(history, offset, _history, 0, count);
				_numHistory = count;
				_historyIndex = _numHistory;
			}
			catch (FileSystemException e)
			{
				Log.Error($"Failed to load console history: {e.Message}");
			}
		}

		/// <summary>
		///   Initializes the console's UI elements.
		/// </summary>
		private void InitializeUI()
		{
			_input.TextChanged += OnTextChanged;
			_input.Template = (out UIElement templateRoot, out ContentPresenter contentPresenter) =>
				templateRoot = contentPresenter = new ContentPresenter();
			_input.InputBindings.AddRange(new[]
			{
				new KeyBinding(ClearInput, Key.Escape),
				new KeyBinding(ShowNewerHistoryEntry, Key.Down) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(ShowOlderHistoryEntry, Key.Up) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(AutoCompleteNext, Key.Tab) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(AutoCompletePrevious, Key.Tab, KeyModifiers.Shift) { TriggerMode = TriggerMode.Repeatedly },
			});

			_scrollViewer = new ScrollViewer
			{
				Margin = new Thickness(0, 0, 0, 3),
				Dock = Dock.Bottom,
				VerticalScrollStep = 100,
				Content = _contentPanel
			};

			_layoutRoot = new Border
			{
				Foreground = Colors.White,
				Background = new Color(0xEE222222),
				VerticalAlignment = VerticalAlignment.Top,
				Child = new DockPanel
				{
					Margin = new Thickness(5),
					Children =
					{
						new DockPanel
						{
							Dock = Dock.Bottom,
							Children =
							{
								new Label { Text = PromptToken, Dock = Dock.Left },
								_input
							}
						},
						_scrollViewer
					}
				}
			};

			RootElement = new Border
			{
				Child = _layoutRoot,
				CapturesInput = true,
				InputBindings =
				{
					new KeyBinding(Submit, Key.Enter),
					new KeyBinding(Submit, Key.NumpadEnter),
					new KeyBinding(Clear, Key.L, KeyModifiers.Control),
					new KeyBinding(_scrollViewer.ScrollUp, Key.PageUp) { TriggerMode = TriggerMode.Repeatedly },
					new KeyBinding(_scrollViewer.ScrollDown, Key.PageDown) { TriggerMode = TriggerMode.Repeatedly },
					new KeyBinding(_scrollViewer.ScrollToTop, Key.PageUp, KeyModifiers.Control),
					new KeyBinding(_scrollViewer.ScrollToBottom, Key.PageDown, KeyModifiers.Control),
					new MouseWheelBinding(_scrollViewer.ScrollUp, MouseWheelDirection.Up),
					new MouseWheelBinding(_scrollViewer.ScrollDown, MouseWheelDirection.Down)
				}
			};
		}

		/// <summary>
		///   Submits the console input.
		/// </summary>
		private void Submit()
		{
			AddInputToHistory();

			if (!String.IsNullOrWhiteSpace(_input.Text))
			{
				Log.Info(PromptToken + _input.Text);
				Commands.Execute(_input.Text);

				// Show the result of the user's input
				_scrollViewer.ScrollToBottom();
			}

			ClearInput();
		}

		/// <summary>
		///   Clears the prompt, removing all current input.
		/// </summary>
		private void ClearInput()
		{
			_input.Text = String.Empty;
			_historyIndex = _numHistory;
			_autoCompletionList = null;
		}

		/// <summary>
		///   Handles changes to the input text.
		/// </summary>
		private void OnTextChanged(string text)
		{
			// Reset the auto completion list if an input was made other than setting the current completion value
			if (_settingAutoCompletedInput)
				return;

			_autoCompletionList = null;
		}

		/// <summary>
		///   Submits the current user input to the input history.
		/// </summary>
		private void AddInputToHistory()
		{
			if (String.IsNullOrWhiteSpace(_input.Text))
				return;

			// Store the input in the input history, if it differs from the last entry
			if (_numHistory != 0 && _history[_numHistory - 1] == _input.Text)
				return;

			// If the history is full, remove the oldest entry
			if (_numHistory == MaxHistory)
			{
				Array.Copy(_history, 1, _history, 0, MaxHistory - 1);
				--_numHistory;
			}

			_history[_numHistory++] = _input.Text;
			_historyIndex = _numHistory;
		}

		/// <summary>
		///   Shows or hides the console.
		/// </summary>
		private void ShowConsole(bool show)
		{
			if (show)
				Show();
			else
				Hide();
		}

		/// <summary>
		///   Adds a the given log entry to the console's content.
		/// </summary>
		private void AddLogEntry(LogEntry logEntry)
		{
			_queuedLogEntries.Enqueue(logEntry);
		}

		/// <summary>
		///   Shows the next newer history entry, if any.
		/// </summary>
		private void ShowNewerHistoryEntry()
		{
			ShowHistory(_historyIndex + 1);
		}

		/// <summary>
		///   Shows the next older history entry, if any.
		/// </summary>
		private void ShowOlderHistoryEntry()
		{
			ShowHistory(_historyIndex - 1);
		}

		/// <summary>
		///   Shows the next auto-completed value if completion is possible.
		/// </summary>
		private void AutoCompleteNext()
		{
			AutoComplete(true);
		}

		/// <summary>
		///   Shows the previous auto-completed value if completion is possible.
		/// </summary>
		private void AutoCompletePrevious()
		{
			AutoComplete(false);
		}

		/// <summary>
		///   Shows the next or previous auto-completed value if completion is possible.
		/// </summary>
		/// <param name="next">If true, the next auto-completed entry is shown; otherwise, the previous one is shown.</param>
		private void AutoComplete(bool next)
		{
			if (_autoCompletionList == null)
			{
				_autoCompletionList = GetAutoCompletionList();

				// If auto-completion returned no results, we're done here
				if (_autoCompletionList == null)
					return;

				_autoCompletionIndex = next ? 0 : _autoCompletionList.Length - 1;
			}
			else
			{
				_autoCompletionIndex = (_autoCompletionIndex + (next ? 1 : -1)) % _autoCompletionList.Length;
				if (_autoCompletionIndex < 0)
					_autoCompletionIndex += _autoCompletionList.Length;
			}

			_settingAutoCompletedInput = true;
			_input.Text = _autoCompletionList[_autoCompletionIndex] + " ";
			_settingAutoCompletedInput = false;
		}

		/// <summary>
		///   Gets the auto completion list for the current input.
		/// </summary>
		private string[] GetAutoCompletionList()
		{
			if (String.IsNullOrWhiteSpace(_input.Text))
				return null;

			var commands = Commands.All.Where(command => command.Name.ToLower().StartsWith(_input.Text.ToLower())).Select(command => command.Name);
			var cvars = Cvars.All.Where(cvar => cvar.Name.ToLower().StartsWith(_input.Text.ToLower())).Select(cvar => cvar.Name);

			var list = cvars.Union(commands).OrderBy(item => item).ToArray();
			if (list.Length == 0)
				return null;

			return list;
		}

		/// <summary>
		///   Shows the history entry at the given index.
		/// </summary>
		/// <param name="index">The index of the entry that should be shown.</param>
		private void ShowHistory(int index)
		{
			if (_numHistory == 0)
				return;

			// Ensure the index is inside the bounds
			if (index < 0)
				index = 0;

			// Empty the input box when going past the newest entry
			if (index >= _numHistory)
			{
				_input.Text = String.Empty;
				index = _numHistory;
			}
			else
				_input.Text = _history[index];

			_historyIndex = index;
			_autoCompletionList = null;
		}

		/// <summary>
		///   Removes all log entries shown by the console.
		/// </summary>
		private void Clear()
		{
			_contentPanel.Children.Clear();
			_scrollViewer.ScrollToBottom();
		}
	}
}