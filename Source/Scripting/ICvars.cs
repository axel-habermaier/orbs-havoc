// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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

namespace PointWars.Scripting
{
	using Math;
	using Network;
	using Platform;
	using Platform.Input;
	using Validators;

	/// <summary>
	///   Declares the cvars required by the framework.
	/// </summary>
	internal interface ICvars
	{
		/// <summary>
		///   The scaling factor that is applied to all time-scaling sensitive timing values.
		/// </summary>
		[Cvar(1.0), Range(0.1, 10.0)]
		double TimeScale { get; set; }

		/// <summary>
		///   The screen resolution used by the application in fullscreen mode.
		/// </summary>
		[Cvar("new Size(1024, 768)"), Persistent, WindowSize]
		Size Resolution { get; set; }

		/// <summary>
		///   The size in pixels of the application window in non-fullscreen mode.
		/// </summary>
		[Cvar("new Size(1024, 768)"), Persistent, WindowSize, SystemOnly]
		Size WindowSize { get; set; }

		/// <summary>
		///   Indicates whether the application is run in fullscreen mode.
		/// </summary>
		[Cvar(true), Persistent, SystemOnly]
		bool Fullscreen { get; set; }

		/// <summary>
		///   Shows or hides the debug overlay.
		/// </summary>
		[Cvar(PlatformInfo.IsDebug), Persistent]
		bool ShowDebugOverlay { get; set; }

		/// <summary>
		///   Enables or disable vertical synchronization (vsync). Enabling vsync avoids screen tearing but increases latency.
		/// </summary>
		[Cvar(true), Persistent]
		bool Vsync { get; set; }

		/// <summary>
		///   The name of the player.
		/// </summary>
		[Cvar("UnnamedPlayer"), Persistent, NotEmpty, MaximumLength(NetworkProtocol.PlayerNameLength, checkUtf8Length: true)]
		string PlayerName { get; set; }

		/// <summary>
		///   The display time (in seconds) of event messages such as 'X killed Y', 'X joined the game', etc.
		/// </summary>
		[Cvar(3), Persistent, Range(0.5, 60.0)]
		double EventMessageDisplayTime { get; set; }

		/// <summary>
		///   The display time (in seconds) of chat messages.
		/// </summary>
		[Cvar(6), Persistent, Range(0.5, 60.0)]
		double ChatMessageDisplayTime { get; set; }

		/// <summary>
		///   When triggered in an active game session, shows the scoreboard.
		/// </summary>
		[Cvar("Key.Tab"), Persistent]
		ConfigurableInput InputShowScoreboard { get; set; }

		/// <summary>
		///   When triggered in an active game session, respawns the player after death.
		/// </summary>
		[Cvar("MouseButton.Left"), Persistent]
		ConfigurableInput InputRespawn { get; set; }

		/// <summary>
		///   When triggered in an active game session, moves the player forwards.
		/// </summary>
		[Cvar("Key.W"), Persistent]
		ConfigurableInput InputForward { get; set; }

		/// <summary>
		///   When triggered in an active game session, moves the player backwards.
		/// </summary>
		[Cvar("Key.S"), Persistent]
		ConfigurableInput InputBackward { get; set; }

		/// <summary>
		///   When triggered in an active game session, moves the player to the left.
		/// </summary>
		[Cvar("Key.A"), Persistent]
		ConfigurableInput InputStrafeLeft { get; set; }

		/// <summary>
		///   When triggered in an active game session, moves the player to the right.
		/// </summary>
		[Cvar("Key.D"), Persistent]
		ConfigurableInput InputStrafeRight { get; set; }

		/// <summary>
		///   When triggered in an active game session, triggers the player's after burner.
		/// </summary>
		[Cvar("Key.LeftShift"), Persistent]
		ConfigurableInput InputAfterBurner { get; set; }

		/// <summary>
		///   When triggered in an active game session, fires the player's primary weapon.
		/// </summary>
		[Cvar("MouseButton.Left"), Persistent]
		ConfigurableInput InputPrimaryWeapon { get; set; }

		/// <summary>
		///   When triggered in an active game session, fires the player's secondary weapon.
		/// </summary>
		[Cvar("MouseButton.Right"), Persistent]
		ConfigurableInput InputSecondaryWeapon { get; set; }

		/// <summary>
		///   When triggered in an active game session, fires the player's tertiary weapon.
		/// </summary>
		[Cvar("Key.Q"), Persistent]
		ConfigurableInput InputTertiaryWeapon { get; set; }

		/// <summary>
		///   When triggered in an active game session, fires the player's quaternary weapon.
		/// </summary>
		[Cvar("Key.E"), Persistent]
		ConfigurableInput InputQuaternaryWeapon { get; set; }

		/// <summary>
		///   When triggered in an active game session, opens the chat input.
		/// </summary>
		[Cvar("Key.Return"), Persistent]
		ConfigurableInput InputChat { get; set; }
	}
}