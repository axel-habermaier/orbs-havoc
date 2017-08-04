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
	using System.Collections.Generic;
	using System.Text;
	using Network;
	using Platform.Input;
	using Scripting;
	using UI;
	using UserInterface;
	using UserInterface.Input;
	using Utilities;

	internal sealed class OptionsMenu : View<OptionsMenuUI>
	{
		private string PlayerName => Encoding.UTF8.GetByteCount(UI.Name.Text) > NetworkProtocol.PlayerNameLength ? null : UI.Name.Text;

		public override void InitializeUI()
		{
			UI.InputBindings.Add(new KeyBinding(ShowPreviousMenu, Key.Escape));
			UI.Name.TextChanged = OnNameChanged;
			UI.Save.Click = Save;
			UI.Return.Click = ShowPreviousMenu;
		}

		protected override void Activate()
		{
			UI.Name.Text = Cvars.PlayerName;
			UI.Vsync.IsChecked = Cvars.Vsync;
			UI.DebugOverlay.IsChecked = Cvars.ShowDebugOverlay;
			UI.Bloom.IsChecked = Cvars.BloomEnabled;
		}

		private void OnNameChanged(string name)
		{
			UI.InvalidName.Visibility = TextString.IsNullOrWhiteSpace(PlayerName) ? Visibility.Visible : Visibility.Collapsed;
		}

		private void Save()
		{
			if (TextString.IsNullOrWhiteSpace(PlayerName))
				return;

			ChangeValue(Cvars.PlayerNameCvar, PlayerName);
			ChangeValue(Cvars.VsyncCvar, UI.Vsync.IsChecked);
			ChangeValue(Cvars.ShowDebugOverlayCvar, UI.DebugOverlay.IsChecked);
			ChangeValue(Cvars.BloomEnabledCvar, UI.Bloom.IsChecked);

			ShowPreviousMenu();
		}

		private static void ChangeValue<T>(Cvar<T> cvar, T value)
		{
			if (!EqualityComparer<T>.Default.Equals(cvar.Value, value))
				cvar.Value = value;
		}

		private void ShowPreviousMenu()
		{
			Hide();

			if (Views.Game.IsShown)
				Views.InGameMenu.Show();
			else
				Views.MainMenu.Show();
		}
	}
}