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

namespace PointWars.UserInterface
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Platform.Input;
	using Platform.Logging;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using Utilities;

	/// <summary>
	///   A Quake-like in-game console.
	/// </summary>
	internal sealed class Console : DisposableObject
	{
		/// <summary>
		///   The maximum length of all console input or output.
		/// </summary>
		public const int MaxLength = 4096;

		/// <summary>
		///   The display color of error messages.
		/// </summary>
		private static readonly Color ErrorColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);

		/// <summary>
		///   The display color of warnings.
		/// </summary>
		private static readonly Color WarningColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);

		/// <summary>
		///   The display color of normal messages.
		/// </summary>
		private static readonly Color InfoColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

		/// <summary>
		///   The background color of the console.
		/// </summary>
		private static readonly Color BackgroundColor = new Color(0xEE333333);

		/// <summary>
		///   The display color of debug messages.
		/// </summary>
		private static readonly Color DebugInfoColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);

		/// <summary>
		///   Caches log entries until the console is fully initialized.
		/// </summary>
		private List<LogEntry> _cachedLogEntries = new List<LogEntry>();

		/// <summary>
		///   The console's content.
		/// </summary>
		private ConsoleContent _content;

		/// <summary>
		///   The font used by the console.
		/// </summary>
		private Font _font;

		/// <summary>
		///   The current input for the console.
		/// </summary>
		private ConsoleInput _input;

		/// <summary>
		///   The logical input device used by the console.
		/// </summary>
		private LogicalInputDevice _inputDevice;

		/// <summary>
		///   Indicates whether the console is currently active.
		/// </summary>
		private bool _isOpened;

		/// <summary>
		///   The margin between the console borders and the rows.
		/// </summary>
		private Size _margin = new Size(5, 5);

		/// <summary>
		///   The console's prompt.
		/// </summary>
		private ConsolePrompt _prompt;

		/// <summary>
		///   The current size of the console.
		/// </summary>
		private Size _size;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Console()
		{
			Log.OnLog += ShowLogEntry;
		}

		/// <summary>
		///   Gets or sets a value indicating whether the console is currently opened.
		/// </summary>
		public bool IsOpened
		{
			get { return _isOpened; }
			private set
			{
				if (value == _isOpened)
					return;

				_isOpened = value;
				_prompt.Clear();
				_input.OnActivationChanged(value);
			}
		}

		/// <summary>
		///   Initializes the console.
		/// </summary>
		/// <param name="size">The initial size of the console.</param>
		/// <param name="inputDevice">The input device that provides the user input.</param>
		/// <param name="font">The font that should be used for drawing.</param>
		public void Initialize(Size size, LogicalInputDevice inputDevice, Font font)
		{
			Assert.ArgumentNotNull(inputDevice, nameof(inputDevice));
			Assert.ArgumentNotNull(font, nameof(font));

			_font = font;
			_content = new ConsoleContent(_font);
			_prompt = new ConsolePrompt(_font, InfoColor);
			_input = new ConsoleInput(inputDevice);
			_inputDevice = inputDevice;

			Commands.OnShowConsole += ShowConsole;

			_input.TextEntered += OnTextEntered;
			_inputDevice.Keyboard.KeyPressed += OnKeyPressed;
			_inputDevice.Mouse.Wheel += OnMouseWheelMoved;

			ChangeSize(size);

			var logEntries = _cachedLogEntries;
			_cachedLogEntries = null;

			foreach (var logEntry in logEntries)
				ShowLogEntry(logEntry);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnShowConsole -= ShowConsole;
			Log.OnLog -= ShowLogEntry;

			if (_input != null)
			{
				_input.TextEntered -= OnTextEntered;
				_inputDevice.Keyboard.KeyPressed -= OnKeyPressed;
			}

			if (_inputDevice != null)
				_inputDevice.Mouse.Wheel -= OnMouseWheelMoved;

			_input.SafeDispose();
			_prompt.SafeDispose();
			_content.Dispose();
		}

		/// <summary>
		///   Invoked whenever a text is entered.
		/// </summary>
		private void OnTextEntered(string text)
		{
			if (!_isOpened)
				return;

			foreach (var c in text)
				_prompt.InsertCharacter(c);
		}

		/// <summary>
		///   Invoked whenever a key is pressed.
		/// </summary>
		private void OnKeyPressed(Key key, ScanCode scanCode, KeyModifiers modifiers)
		{
			if (_isOpened)
				_prompt.InjectKeyPress(key, modifiers);
		}

		/// <summary>
		///   Invoked when the user used the mouse wheel to scroll.
		/// </summary>
		/// <param name="direction">The direction that the mouse wheel has been moved.</param>
		private void OnMouseWheelMoved(MouseWheelDirection direction)
		{
			if (!_isOpened)
				return;

			if (direction == MouseWheelDirection.Down)
				_content.ScrollDown();
			else
				_content.ScrollUp();
		}

		/// <summary>
		///   Draws the console.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the console.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			// Only draw the console when it is currently active
			if (!_isOpened)
				return;

			// Draw the background
			spriteBatch.PositionOffset = Vector2.Zero;
			spriteBatch.ScissorArea = null;
			spriteBatch.Layer = Int32.MaxValue - 1;

			var consoleArea = new Rectangle(0, 0, _size.Width, _prompt.ActualArea.Bottom + _margin.Height);
			spriteBatch.Draw(consoleArea, BackgroundColor);

			// Draw the prompt and content
			spriteBatch.Layer = Int32.MaxValue;

			_prompt.Draw(spriteBatch);
			_content.Draw(spriteBatch);

			spriteBatch.Layer = 0;
			spriteBatch.PositionOffset = Vector2.Zero;
		}

		/// <summary>
		///   Updates the console.
		/// </summary>
		internal void Update()
		{
			// Check whether the console should be toggle on or off
			if (_input.Toggle.IsTriggered)
				IsOpened = !IsOpened;

			// Don't handle any other input if the console isn't active
			if (!IsOpened)
				return;

			if (_input.Submit.IsTriggered)
			{
				var input = _prompt.Submit();
				if (!String.IsNullOrWhiteSpace(input))
				{
					Log.Info("{0}{1}", ConsolePrompt.Prompt, input);
					Commands.Execute(input);

					// Show the result of the user's input
					_content.ScrollToBottom();
				}
			}

			if (_input.ClearPrompt.IsTriggered)
				_prompt.Clear();

			if (_input.ShowOlderHistory.IsTriggered)
				_prompt.ShowOlderHistoryEntry();

			if (_input.ShowNewerHistory.IsTriggered)
				_prompt.ShowNewerHistoryEntry();

			if (_input.Clear.IsTriggered)
			{
				_content.Clear();
				_prompt.Clear();
			}

			if (_input.ScrollUp.IsTriggered)
				_content.ScrollUp();

			if (_input.ScrollDown.IsTriggered)
				_content.ScrollDown();

			if (_input.ScrollToTop.IsTriggered)
				_content.ScrollToTop();

			if (_input.ScrollToBottom.IsTriggered)
				_content.ScrollToBottom();

			if (_input.AutoCompleteNext.IsTriggered)
				_prompt.AutoCompleteNext();

			if (_input.AutoCompletePrevious.IsTriggered)
				_prompt.AutoCompletePrevious();
		}

		/// <summary>
		///   Changes the size of the console.
		/// </summary>
		/// <param name="size">The size of the area that the console should be drawn on.</param>
		internal void ChangeSize(Size size)
		{
			size.Height = MathUtils.Round(size.Height / 2);
			if (_size == size)
				return;

			_size = size;

			// Calculate the prompt area
			var promptArea = new Rectangle(_margin.Width, _size.Height - _font.LineHeight - _margin.Height,
				_size.Width - 2 * _margin.Width, _font.LineHeight);
			_prompt.Resize(promptArea);

			// Resize the content area
			var contentArea = new Rectangle(_margin.Width, 0, _size.Width - 2 * _margin.Width, promptArea.Top);
			_content.Resize(contentArea);
		}

		/// <summary>
		///   Shows or hides the console.
		/// </summary>
		/// <param name="show">Indicates whether the console should be shown.</param>
		private void ShowConsole(bool show)
		{
			IsOpened = show;
		}

		/// <summary>
		///   Shows the given entry on the console.
		/// </summary>
		/// <param name="entry">The entry that should be shown.</param>
		private void ShowLogEntry(LogEntry entry)
		{
			if (_cachedLogEntries != null)
				_cachedLogEntries.Add(entry);
			else
			{
				var color = InfoColor;
				switch (entry.LogType)
				{
					case LogType.Error:
						color = ErrorColor;
						break;
					case LogType.Warning:
						color = WarningColor;
						break;
					case LogType.Debug:
						color = DebugInfoColor;
						break;
				}

				_content.Add(entry.Message, color);
			}
		}
	}
}