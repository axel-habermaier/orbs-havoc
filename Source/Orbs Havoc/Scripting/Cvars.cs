// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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

namespace OrbsHavoc.Scripting
{
	using System.ComponentModel;
	using System.Numerics;
	using Network;
	using Platform;
	using Platform.Input;
	using Rendering;
	using Utilities;
	using Validators;

	/// <summary>
	///   Declares the cvars required by the framework.
	/// </summary>
	internal interface ICvars
	{
		/// <summary>
		///   The screen resolution used by the application in fullscreen mode.
		/// </summary>
		[DefaultValue("new Size(1024, 768)"), Persistent, WindowSize]
		Size Resolution { get; set; }

		/// <summary>
		///   The size in pixels of the application window in non-fullscreen mode.
		/// </summary>
		[DefaultValue("new Size(1024, 768)"), Persistent, WindowSize, SystemOnly]
		Size WindowSize { get; set; }

		/// <summary>
		///   The screen position of the application window's top left corner in non-fullscreen mode.
		/// </summary>
		[DefaultValue("new Vector2(100, 100)"), Persistent, WindowPosition, SystemOnly]
		Vector2 WindowPosition { get; set; }

		/// <summary>
		///   The width of the application's window in non-fullscreen mode.
		/// </summary>
		[DefaultValue(WindowMode.Fullscreen), Persistent, SystemOnly]
		WindowMode WindowMode { get; set; }

		/// <summary>
		///   Shows or hides the debug overlay.
		/// </summary>
		[DefaultValue(false), Persistent]
		bool ShowDebugOverlay { get; set; }

		/// <summary>
		///   Enables or disable vertical synchronization (vsync). Enabling vsync avoids screen tearing but increases latency.
		/// </summary>
		[DefaultValue(true), Persistent]
		bool Vsync { get; set; }

		/// <summary>
		///   The name of the player.
		/// </summary>
		[Persistent, NotEmpty, MaximumLength(NetworkProtocol.PlayerNameLength, checkUtf8Length: true)]
		[DefaultValue(@"String.IsNullOrWhiteSpace(Environment.UserName) ? ""UnnamedUser"" : Environment.UserName")]
		string PlayerName { get; set; }

		/// <summary>
		///   The display time (in seconds) of event messages such as 'X killed Y', 'X joined the game', etc.
		/// </summary>
		[DefaultValue(3), Persistent, Range(0.5, 60.0)]
		double EventMessageDisplayTime { get; set; }

		/// <summary>
		///   The display time (in seconds) of chat messages.
		/// </summary>
		[DefaultValue(6), Persistent, Range(0.5, 60.0)]
		double ChatMessageDisplayTime { get; set; }

		/// <summary>
		///   While the input is triggered in an active game session, shows the scoreboard.
		/// </summary>
		[DefaultValue(Key.Tab), Persistent]
		InputTrigger InputShowScoreboard { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, moves the player in the up direction.
		/// </summary>
		[DefaultValue(Key.W), Persistent]
		InputTrigger InputMoveUp { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, moves the player in the down direction.
		/// </summary>
		[DefaultValue(Key.S), Persistent]
		InputTrigger InputMoveDown { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, moves the player to the left.
		/// </summary>
		[DefaultValue(Key.A), Persistent]
		InputTrigger InputMoveLeft { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, moves the player to the right.
		/// </summary>
		[DefaultValue(Key.D), Persistent]
		InputTrigger InputMoveRight { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, fires the player's primary weapon.
		/// </summary>
		[DefaultValue(MouseButton.Left), Persistent]
		InputTrigger InputFirePrimary { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, fires the player's secondary weapon.
		/// </summary>
		[DefaultValue(MouseButton.Right), Persistent]
		InputTrigger InputFireSecondary { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, opens the chat input.
		/// </summary>
		[DefaultValue(Key.Enter), Persistent]
		InputTrigger InputChat { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, the next available primary weapon is selected.
		/// </summary>
		[DefaultValue(MouseWheelDirection.Up), Persistent]
		InputTrigger InputNextWeapon { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, the previous available primary weapon is selected.
		/// </summary>
		[DefaultValue(MouseWheelDirection.Down), Persistent]
		InputTrigger InputPreviousWeapon { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, selects the minigun.
		/// </summary>
		[DefaultValue(Key.Num1), Persistent]
		InputTrigger InputSelectMiniGun { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, selects the rocket launcher.
		/// </summary>
		[DefaultValue(Key.Num2), Persistent]
		InputTrigger InputSelectRocketLauncher { get; set; }

		/// <summary>
		///   When the input is triggered in an active game session, selects the lighting gun.
		/// </summary>
		[DefaultValue(Key.Num3), Persistent]
		InputTrigger InputSelectLightingGun { get; set; }

		/// <summary>
		///   Indicates whether the bloom effect is enabled.
		/// </summary>
		[DefaultValue(true), Persistent]
		bool BloomEnabled { get; set; }

		/// <summary>
		///   Determines the quality of the bloom effect, if enabled.
		/// </summary>
		[DefaultValue(QualityLevel.Medium), Persistent]
		QualityLevel BloomQuality { get; set; }
	}
}