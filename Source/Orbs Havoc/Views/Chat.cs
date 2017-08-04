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
			base.InitializeUI();

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