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

namespace PointWars.UserInterfaceOld
{
	using System;
	using System.Linq;
	using System.Text;
	using Platform;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using Utilities;

	/// <summary>
	///   The input prompt of the console.
	/// </summary>
	internal class ConsolePrompt : DisposableObject
	{
		/// <summary>
		///   The prompt token.
		/// </summary>
		internal const string Prompt = "]";

		/// <summary>
		///   The maximum history size.
		/// </summary>
		private const int MaxHistory = 64;

		/// <summary>
		///   The name of the console history file.
		/// </summary>
		private const string HistoryFileName = "console.txt";

		/// <summary>
		///   The input history.
		/// </summary>
		private readonly string[] _history;

		/// <summary>
		///   The input text box.
		/// </summary>
		private readonly TextBox _input;

		/// <summary>
		///   The prompt label.
		/// </summary>
		private readonly Label _prompt;

		/// <summary>
		///   The area of the prompt.
		/// </summary>
		private Rectangle _area;

		/// <summary>
		///   The current auto-completion index.
		/// </summary>
		private int _autoCompletionIndex;

		/// <summary>
		///   The currently active auto-completion list.
		/// </summary>
		private string[] _autoCompletionList;

		/// <summary>
		///   Stores the current index into the input history.
		/// </summary>
		private int _historyIndex;

		/// <summary>
		///   The number of history slots that are currently used.
		/// </summary>
		private int _numHistory;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="font">The font that should be used to draw the prompt.</param>
		/// <param name="color">The text color of the prompt.</param>
		public ConsolePrompt(Font font, Color color)
		{
			Assert.ArgumentNotNull(font, nameof(font));

			_history = new string[MaxHistory];
			_input = new TextBox(font, Console.MaxLength) { Color = color };
			_prompt = new Label(Prompt) { Font = font, Color = color };

			try
			{
				var content = FileSystem.ReadAllText(HistoryFileName);
				var history = content.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
				history = history.Where(h => h.Length <= Console.MaxLength).ToArray();

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
				Log.Error("Failed to load console history: {0}", e.Message);
			}
		}

		/// <summary>
		///   Gets the actual area of the prompt.
		/// </summary>
		public Rectangle ActualArea => new Rectangle(_area.Left, _area.Top, _area.Width, _input.ContentArea.Height);

		/// <summary>
		///   Changes the focus of the prompt.
		/// </summary>
		/// <param name="focus">Indicates whether the prompt has the focus.</param>
		public void SetFocus(bool focus)
		{
			if (focus)
				_input.Focus();
			else
				_input.Unfocus();
		}

		/// <summary>
		///   Injects a key press event.
		/// </summary>
		public void InjectKeyPress(Key key, KeyModifiers modifiers)
		{
			_input.InjectKeyPress(key, modifiers);
		}

		/// <summary>
		///   Draws the prompt.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the prompt.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			_prompt.Draw(spriteBatch);
			_input.Draw(spriteBatch);
		}

		/// <summary>
		///   Resizes the prompt area.
		/// </summary>
		/// <param name="area">The new prompt area.</param>
		public void Resize(Rectangle area)
		{
			_area = area;

			_input.ContentArea = new Rectangle(area.Left + _prompt.ContentArea.Width, area.Top, area.Width - _prompt.ContentArea.Width, 0);
			_prompt.ContentArea = new Rectangle(area.Left, area.Top, Int16.MaxValue, 0);
		}

		/// <summary>
		///   Inserts the given character at the current caret position.
		/// </summary>
		/// <param name="c">The character that should be inserted.</param>
		public void InsertCharacter(char c)
		{
			_input.InsertCharacter(c);
			_autoCompletionList = null;
		}

		/// <summary>
		///   Submits the current user input to the input history, returns the current input, and
		///   resets the input.
		/// </summary>
		public string Submit()
		{
			if (!String.IsNullOrWhiteSpace(_input.Text))
			{
				// Store the input in the input history, if it differs from the last entry
				if (_numHistory == 0 || _history[_numHistory - 1] != _input.Text)
				{
					// If the history is full, remove the oldest entry; this could be implemented more efficiently
					if (_numHistory == MaxHistory)
					{
						Array.Copy(_history, 1, _history, 0, MaxHistory - 1);
						--_numHistory;
					}

					_history[_numHistory++] = _input.Text;
					_historyIndex = _numHistory;
				}
			}

			// Reset and return the input
			var input = _input.Text;
			Clear();
			return input;
		}

		/// <summary>
		///   Clears the user input.
		/// </summary>
		public void Clear()
		{
			_input.Text = String.Empty;
			_historyIndex = _numHistory;
			_autoCompletionList = null;
		}

		/// <summary>
		///   Shows the next newer history entry, if any.
		/// </summary>
		public void ShowNewerHistoryEntry()
		{
			ShowHistory(_historyIndex + 1);
		}

		/// <summary>
		///   Shows the next older history entry, if any.
		/// </summary>
		public void ShowOlderHistoryEntry()
		{
			ShowHistory(_historyIndex - 1);
		}

		/// <summary>
		///   Shows the next auto-completed value if completion is possible.
		/// </summary>
		public void AutoCompleteNext()
		{
			AutoComplete(true);
		}

		/// <summary>
		///   Shows the previous auto-completed value if completion is possible.
		/// </summary>
		public void AutoCompletePrevious()
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

			_input.Text = _autoCompletionList[_autoCompletionIndex] + " ";
		}

		/// <summary>
		///   Gets the auto completion list for the current input.
		/// </summary>
		private string[] GetAutoCompletionList()
		{
			if (String.IsNullOrWhiteSpace(_input.Text))
				return null;

			var commands = Commands
				.All
				.Where(command => command.Name.ToLower().StartsWith(_input.Text.ToLower()))
				.Select(command => command.Name);

			var cvars = Cvars
				.All
				.Where(cvar => cvar.Name.ToLower().StartsWith(_input.Text.ToLower()))
				.Select(cvar => cvar.Name);

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
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_input.SafeDispose();
			_prompt.SafeDispose();

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
				Log.Error("Failed to persist console history: {0}", e.Message);
			}
		}
	}
}