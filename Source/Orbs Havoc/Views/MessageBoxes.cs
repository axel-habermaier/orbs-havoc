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