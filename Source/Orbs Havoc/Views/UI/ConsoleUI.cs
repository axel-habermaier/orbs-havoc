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

namespace OrbsHavoc.Views.UI
{
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	internal sealed class ConsoleUI : Border
	{
		public const int MaxLogEntryLength = 2048;
		public const string PromptToken = "]";
		private const int MaxLogEntryCount = 2048;

		private static readonly Color _errorColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
		private static readonly Color _warningColor = new Color(1.0f, 1.0f, 0.0f, 1.0f);
		private static readonly Color _infoColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		private static readonly Color _debugInfoColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);

		private readonly StackPanel _contentPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom, MinWidth = 200 };
		private readonly UIElement _layoutRoot;

		public ConsoleUI()
		{
			Prompt.Template = (out UIElement templateRoot, out ContentPresenter contentPresenter) =>
				templateRoot = contentPresenter = new ContentPresenter();

			ScrollViewer = new ScrollViewer
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
								Prompt
							}
						},
						ScrollViewer
					}
				}
			};

			Child = _layoutRoot;
			CapturesInput = true;
			InputBindings.AddRange(
				new KeyBinding(ClearLogEntries, Key.L, KeyModifiers.Control),
				new KeyBinding(ScrollViewer.ScrollUp, Key.PageUp) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(ScrollViewer.ScrollDown, Key.PageDown) { TriggerMode = TriggerMode.Repeatedly },
				new KeyBinding(ScrollViewer.ScrollToTop, Key.PageUp, KeyModifiers.Control),
				new KeyBinding(ScrollViewer.ScrollToBottom, Key.PageDown, KeyModifiers.Control),
				new MouseWheelBinding(ScrollViewer.ScrollUp, MouseWheelDirection.Up),
				new MouseWheelBinding(ScrollViewer.ScrollDown, MouseWheelDirection.Down)
			);
		}

		public TextBox Prompt { get; } = new TextBox { MaxLength = MaxLogEntryLength, Dock = Dock.Bottom, AutoFocus = true };
		public ScrollViewer ScrollViewer { get; }

		public void Update(Size windowSize)
		{
			_layoutRoot.Height = MathUtils.Round(windowSize.Height / 2);

			// Make sure the text box never loses focus while the console is active
			Prompt.Focus();
		}

		public void AddLogEntry(LogEntry logEntry)
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

			if (_contentPanel.Children.Count < MaxLogEntryCount)
			{
				_contentPanel.Children.Add(new Label
				{
					Text = logEntry.Message.Truncate(MaxLogEntryLength),
					Foreground = color,
					TextWrapping = TextWrapping.Wrap,
					Margin = new Thickness(0, 2, 0, 0)
				});
			}
			else
			{
				// If all labels are used, remove the oldest one by shifting the entire children collection up one
				// index and add the oldest label to the end of the collection (in order to re-use the label instance for 
				// the new message); this way, we only copy MaxLogEntryCount * ReferenceSize bytes and avoid relayouting all lines.

				var label = (Label)_contentPanel.Children[0];
				_contentPanel.Children.RemoveAt(0);

				label.Text = logEntry.Message.Truncate(MaxLogEntryLength);
				label.Foreground = color;
				_contentPanel.Children.Add(label);
			}
		}

		public void ClearLogEntries()
		{
			_contentPanel.Children.Clear();
			ScrollViewer.ScrollToBottom();
		}
	}
}