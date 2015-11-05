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

namespace PointWars.Views
{
	using System;
	using System.Net;
	using Assets;
	using Platform.Input;
	using Platform.Logging;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using UserInterface.Input;
	using Utilities;

	/// <summary>
	///   Represents loading overlay at the start of a game session.
	/// </summary>
	internal sealed class LoadingOverlay : View
	{
		private readonly Label _infoLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
		private Clock _clock = new Clock();
		private IPEndPoint _serverEndPoint;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			RootElement = new Border
			{
				CapturesInput = true,
				IsFocusable = true,
				Font = AssetBundle.Roboto14,
				AutoFocus = true,
				InputBindings =
				{
					new KeyBinding(Commands.Disconnect, Key.Escape)
				},
				Child = new StackPanel
				{
					HorizontalAlignment = HorizontalAlignment.Center,
					VerticalAlignment = VerticalAlignment.Center,
					Children =
					{
						new Label
						{
							Text = "Loading",
							Font = AssetBundle.Moonhouse80,
							Margin = new Thickness(0, 0, 0, 30),
						},
						_infoLabel
					}
				}
			};
		}

		/// <summary>
		///   Loads the game session.
		/// </summary>
		public void Load(IPEndPoint serverEndPoint)
		{
			Assert.ArgumentNotNull(serverEndPoint, nameof(serverEndPoint));

			_serverEndPoint = serverEndPoint;
			_clock.Reset();

			Show();
			Log.Info("Connecting to {0}...", serverEndPoint);

			Views.Console.Hide();
			Views.MessageBoxes.CloseAll();
			Views.JoinGameMenu.Hide();
			Views.MainMenu.Hide();
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			UpdateInfoLabel();
		}

		/// <summary>
		///   Updates the info label.
		/// </summary>
		private void UpdateInfoLabel()
		{
			_infoLabel.Text = $"Connecting to {_serverEndPoint} ({Math.Round(_clock.Seconds)} seconds)...";
		}
	}
}