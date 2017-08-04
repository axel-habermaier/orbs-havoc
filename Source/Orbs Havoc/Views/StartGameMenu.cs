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
	using Gameplay.Server;
	using Network;
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal sealed class StartGameMenu : View<StartGameMenuUI>
	{
		private string ServerName => Encoding.UTF8.GetByteCount(UI.Name.Text) > NetworkProtocol.ServerNameLength ? null : UI.Name.Text;
		private ushort? ServerPort => UInt16.TryParse(UI.Port.Text, out var port) ? port : (ushort?)null;

		public override void InitializeUI()
		{
			base.InitializeUI();

			UI.InputBindings.AddRange(
				new KeyBinding(() =>
				{
					Hide();
					Views.MainMenu.Show();
				}, Key.Escape),
				new KeyBinding(StartGame, Key.Enter),
				new KeyBinding(StartGame, Key.NumpadEnter)
			);

			UI.Name.TextChanged = OnNameChanged;
			UI.Port.TextChanged = OnPortChanged;
			UI.StartGame.Click = StartGame;
			UI.Return.Click = () =>
			{
				Hide();
				Views.MainMenu.Show();
			};
		}

		protected override void Activate()
		{
			UI.Name.Text = GameSessionHost.DefaultServerName;
			UI.Port.Text = NetworkProtocol.DefaultServerPort.ToString();
		}

		private void OnNameChanged(string address)
		{
			UI.InvalidName.Visibility = TextString.IsNullOrWhiteSpace(ServerName) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void OnPortChanged(string port)
		{
			UI.InvalidPort.Visibility = ServerPort == null ? Visibility.Visible : Visibility.Collapsed;
		}

		private void StartGame()
		{
			if (TextString.IsNullOrWhiteSpace(ServerName) || !(ServerPort is ushort port))
				return;

			if (Views.TryStartHost(ServerName, port))
				Commands.Connect("::1", port);
		}
	}
}