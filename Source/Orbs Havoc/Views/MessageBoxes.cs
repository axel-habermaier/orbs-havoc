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
	using Platform.Logging;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Controls;

	/// <summary>
	///     Shows open message boxes.
	/// </summary>
	internal sealed class MessageBoxes : View<AreaPanel>
	{
		/// <summary>
		///     Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Show();
		}

		/// <summary>
		///     Closes all shown message boxes without running their continuations.
		/// </summary>
		public void CloseAll()
		{
			UI.Children.Clear();
		}

		/// <summary>
		///     Shows the given message box.
		/// </summary>
		/// <param name="messageBox">The message box that should be shown.</param>
		private void Show(MessageBoxUI messageBox)
		{
			UI.Add(messageBox);
			Commands.ShowConsole(false);
		}

		/// <summary>
		///     Shows a confirmation message box with an OK and a Cancel button.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="onConfirmed">The continuation that should be executed when the user confirmed.</param>
		public void ShowOkCancel(string title, string message, Action onConfirmed)
		{
			var messageBox = new MessageBoxUI(title, message, "OK", "Cancel") { Button1Clicked = onConfirmed };
			Show(messageBox);
		}

		/// <summary>
		///     Shows a confirmation message box with a Yes and a No button.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		/// <param name="yesContinuation">The continuation that should be executed when the user pressed the Yes button.</param>
		/// <param name="noContinuation">The continuation that should be executed when the user pressed the No button.</param>
		public void ShowYesNo(string title, string message, Action yesContinuation, Action noContinuation = null)
		{
			var messageBox = new MessageBoxUI(title, message, "Yes", "No") { Button1Clicked = yesContinuation, Button2Clicked = noContinuation };
			Show(messageBox);
		}

		/// <summary>
		///     Shows a error message box.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		public void ShowError(string title, string message)
		{
			var messageBox = new MessageBoxUI(title, message, "OK");
			Show(messageBox);

			Log.Error(message);
		}

		/// <summary>
		///     Updates the view's state.
		/// </summary>
		public override void Update()
		{
			UI.Visibility = UI.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}