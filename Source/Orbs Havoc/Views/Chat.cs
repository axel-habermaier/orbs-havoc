namespace OrbsHavoc.Views
{
	using System;
	using System.Text;
	using Network;
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal class Chat : View<ChatUI>
	{
		public override void InitializeUI()
		{
			UI.ChatMessage.TextChanged += _ => CheckMessageLength();
			UI.InputBindings.AddRange(
				new KeyBinding(SendMessage, Key.Enter),
				new KeyBinding(SendMessage, Key.NumpadEnter),
				new KeyBinding(Hide, Key.Escape));
		}

		private bool CheckMessageLength()
		{
			var tooLong = Encoding.UTF8.GetByteCount(UI.ChatMessage.Text) > NetworkProtocol.ChatMessageLength;
			UI.ValidationLabel.Visibility = tooLong ? Visibility.Visible : Visibility.Collapsed;

			return !tooLong;
		}

		protected override void Activate()
		{
			UI.ChatMessage.Text = String.Empty;
			UI.ValidationLabel.Visibility = Visibility.Collapsed;
		}

		private void SendMessage()
		{
			if (!CheckMessageLength())
				return;

			if (!TextString.IsNullOrWhiteSpace(UI.ChatMessage.Text))
				Commands.Say(UI.ChatMessage.Text.Trim());

			Hide();
		}
	}
}