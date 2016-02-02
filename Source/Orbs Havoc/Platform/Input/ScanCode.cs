﻿// The MIT License (MIT)
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

namespace OrbsHavoc.Platform.Input
{
	using JetBrains.Annotations;

	/// <summary>
	///   Identifies a layout-independent key of a keyboard.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	public enum ScanCode
	{
		A = 4,
		B = 5,
		C = 6,
		D = 7,
		E = 8,
		F = 9,
		G = 10,
		H = 11,
		I = 12,
		J = 13,
		K = 14,
		L = 15,
		M = 16,
		N = 17,
		O = 18,
		P = 19,
		Q = 20,
		R = 21,
		S = 22,
		T = 23,
		U = 24,
		V = 25,
		W = 26,
		X = 27,
		Y = 28,
		Z = 29,
		Num1 = 30,
		Num2 = 31,
		Num3 = 32,
		Num4 = 33,
		Num5 = 34,
		Num6 = 35,
		Num7 = 36,
		Num8 = 37,
		Num9 = 38,
		Num0 = 39,
		Return = 40,
		Escape = 41,
		Backspace = 42,
		Tab = 43,
		Space = 44,
		Minus = 45,
		Equals = 46,
		LeftBracket = 47,
		RightBracket = 48,
		BackSlash = 49,
		NonUsHash = 50,
		Semicolon = 51,
		Apostrophe = 52,
		Grave = 53,
		Comma = 54,
		Period = 55,
		Slash = 56,
		Capslock = 57,
		F1 = 58,
		F2 = 59,
		F3 = 60,
		F4 = 61,
		F5 = 62,
		F6 = 63,
		F7 = 64,
		F8 = 65,
		F9 = 66,
		F10 = 67,
		F11 = 68,
		F12 = 69,
		PrintScreen = 70,
		ScrollLock = 71,
		Pause = 72,
		Insert = 73,
		Home = 74,
		PageUp = 75,
		Delete = 76,
		End = 77,
		PageDown = 78,
		Right = 79,
		Left = 80,
		Down = 81,
		Up = 82,
		NumLock = 83,
		NumpadDivide = 84,
		NumpadMultiply = 85,
		NumpadMinus = 86,
		NumpadPlus = 87,
		NumpadEnter = 88,
		Numpad1 = 89,
		Numpad2 = 90,
		Numpad3 = 91,
		Numpad4 = 92,
		Numpad5 = 93,
		Numpad6 = 94,
		Numpad7 = 95,
		Numpad8 = 96,
		Numpad9 = 97,
		Numpad0 = 98,
		NumpadPeriod = 99,
		NonUsBackSlash = 100,
		Application = 101,
		Power = 102,
		NumpadEquals = 103,
		F13 = 104,
		F14 = 105,
		F15 = 106,
		F16 = 107,
		F17 = 108,
		F18 = 109,
		F19 = 110,
		F20 = 111,
		F21 = 112,
		F22 = 113,
		F23 = 114,
		F24 = 115,
		Execute = 116,
		Help = 117,
		Menu = 118,
		Select = 119,
		Stop = 120,
		Again = 121,
		Undo = 122,
		Cut = 123,
		Copy = 124,
		Paste = 125,
		Find = 126,
		Mute = 127,
		VolumeUp = 128,
		VolumeDown = 129,
		NumpadComma = 133,
		NumpadEqualsAs400 = 134,
		International1 = 135,
		International2 = 136,
		International3 = 137,
		International4 = 138,
		International5 = 139,
		International6 = 140,
		International7 = 141,
		International8 = 142,
		International9 = 143,
		Language1 = 144,
		Language2 = 145,
		Language3 = 146,
		Language4 = 147,
		Language5 = 148,
		Language6 = 149,
		Language7 = 150,
		Language8 = 151,
		Language9 = 152,
		AltErase = 153,
		SysReq = 154,
		Cancel = 155,
		Clear = 156,
		Prior = 157,
		Return2 = 158,
		Separator = 159,
		Out = 160,
		Oper = 161,
		ClearAgain = 162,
		Crsel = 163,
		Exsel = 164,
		Numpad00 = 176,
		Numpad000 = 177,
		ThousandsSeparator = 178,
		DecimalSeparator = 179,
		CurrencyUnit = 180,
		CurrencySubUnit = 181,
		NumpadLeftParen = 182,
		NumpadRightParen = 183,
		NumpadLeftBrace = 184,
		NumpadRightBrace = 185,
		NumpadTab = 186,
		NumpadBackspace = 187,
		NumpadA = 188,
		NumpadB = 189,
		NumpadC = 190,
		NumpadD = 191,
		NumpadE = 192,
		NumpadF = 193,
		NumpadXor = 194,
		NumpadPower = 195,
		NumpadPercent = 196,
		NumpadLess = 197,
		NumpadGreater = 198,
		NumpadAmpersand = 199,
		NumpadDblAmpersand = 200,
		NumpadVerticalBar = 201,
		NumpadDblVerticalBar = 202,
		NumpadColon = 203,
		NumpadHash = 204,
		NumpadSpace = 205,
		NumpadAt = 206,
		NumpadExclam = 207,
		NumpadMemStore = 208,
		NumpadMemRecall = 209,
		NumpadMemClear = 210,
		NumpadMemAdd = 211,
		NumpadMemSubtract = 212,
		NumpadMemMultiply = 213,
		NumpadMemDivide = 214,
		NumpadPlusMinus = 215,
		NumpadClear = 216,
		NumpadClearEntry = 217,
		NumpadBinary = 218,
		NumpadOctal = 219,
		NumpadDecimal = 220,
		NumpadHexadecimal = 221,
		LeftControl = 224,
		LeftShift = 225,
		LeftAlt = 226,
		LeftSystem = 227,
		RightControl = 228,
		RightShift = 229,
		RightAlt = 230,
		RightSystem = 231,
		Mode = 257,
		AudioNext = 258,
		AudioPrev = 259,
		AudioStop = 260,
		AudioPlay = 261,
		AudioMute = 262,
		MediaSelect = 263,
		Www = 264,
		Mail = 265,
		Calculator = 266,
		Computer = 267,
		ActionSearch = 268,
		ActionHome = 269,
		ActionBack = 270,
		ActionForward = 271,
		ActionStop = 272,
		ActionRefresh = 273,
		ActionBookmarks = 274,
		BrightnessDown = 275,
		BrightnessUp = 276,
		DisplaySwitch = 277,
		KbdillumToggle = 278,
		KbdillumDown = 279,
		KbdillumUp = 280,
		Eject = 281,
		Sleep = 282,
		App1 = 283,
		App2 = 284
	}
}