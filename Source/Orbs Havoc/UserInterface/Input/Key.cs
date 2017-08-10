﻿namespace OrbsHavoc.UserInterface.Input
{
	using JetBrains.Annotations;

	/// <summary>
	///   Identifies a layout-dependent keyboard key.
	/// </summary>
	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	internal enum Key
	{
		Enter = '\r',
		Escape = 27,
		Backspace = '\b',
		Tab = '\t',
		Space = ' ',
		Exclaim = '!',
		DoubleQuote = '"',
		Hash = '#',
		Percent = '%',
		Dollar = '$',
		Ampersand = '&',
		Quote = '\'',
		LeftParen = '(',
		RightParen = ')',
		Asterisk = '*',
		Plus = '+',
		Comma = ',',
		Minus = '-',
		Period = '.',
		Slash = '/',
		Num0 = '0',
		Num1 = '1',
		Num2 = '2',
		Num3 = '3',
		Num4 = '4',
		Num5 = '5',
		Num6 = '6',
		Num7 = '7',
		Num8 = '8',
		Num9 = '9',
		Colon = ':',
		Semicolon = ';',
		Less = '<',
		Equals = '=',
		Greater = '>',
		Question = '?',
		At = '@',
		LeftBracket = '[',
		BackSlash = '\\',
		RightBracket = ']',
		Caret = '^',
		Underscore = '_',
		Backquote = '`',
		A = 'a',
		B = 'b',
		C = 'c',
		D = 'd',
		E = 'e',
		F = 'f',
		G = 'g',
		H = 'h',
		I = 'i',
		J = 'j',
		K = 'k',
		L = 'l',
		M = 'm',
		N = 'n',
		O = 'o',
		P = 'p',
		Q = 'q',
		R = 'r',
		S = 's',
		T = 't',
		U = 'u',
		V = 'v',
		W = 'w',
		X = 'x',
		Y = 'y',
		Z = 'z',
		Capslock = ScanCode.Capslock | (1 << 30),
		F1 = ScanCode.F1 | (1 << 30),
		F2 = ScanCode.F2 | (1 << 30),
		F3 = ScanCode.F3 | (1 << 30),
		F4 = ScanCode.F4 | (1 << 30),
		F5 = ScanCode.F5 | (1 << 30),
		F6 = ScanCode.F6 | (1 << 30),
		F7 = ScanCode.F7 | (1 << 30),
		F8 = ScanCode.F8 | (1 << 30),
		F9 = ScanCode.F9 | (1 << 30),
		F10 = ScanCode.F10 | (1 << 30),
		F11 = ScanCode.F11 | (1 << 30),
		F12 = ScanCode.F12 | (1 << 30),
		PrintScreen = ScanCode.PrintScreen | (1 << 30),
		ScrollLock = ScanCode.ScrollLock | (1 << 30),
		Pause = ScanCode.Pause | (1 << 30),
		Insert = ScanCode.Insert | (1 << 30),
		Home = ScanCode.Home | (1 << 30),
		PageUp = ScanCode.PageUp | (1 << 30),
		Delete = 127,
		End = ScanCode.End | (1 << 30),
		PageDown = ScanCode.PageDown | (1 << 30),
		Right = ScanCode.Right | (1 << 30),
		Left = ScanCode.Left | (1 << 30),
		Down = ScanCode.Down | (1 << 30),
		Up = ScanCode.Up | (1 << 30),
		NumLock = ScanCode.NumLock | (1 << 30),
		NumpadDivide = ScanCode.NumpadDivide | (1 << 30),
		NumpadMultiply = ScanCode.NumpadMultiply | (1 << 30),
		NumpadMinus = ScanCode.NumpadMinus | (1 << 30),
		NumpadPlus = ScanCode.NumpadPlus | (1 << 30),
		NumpadEnter = ScanCode.NumpadEnter | (1 << 30),
		Numpad1 = ScanCode.Numpad1 | (1 << 30),
		Numpad2 = ScanCode.Numpad2 | (1 << 30),
		Numpad3 = ScanCode.Numpad3 | (1 << 30),
		Numpad4 = ScanCode.Numpad4 | (1 << 30),
		Numpad5 = ScanCode.Numpad5 | (1 << 30),
		Numpad6 = ScanCode.Numpad6 | (1 << 30),
		Numpad7 = ScanCode.Numpad7 | (1 << 30),
		Numpad8 = ScanCode.Numpad8 | (1 << 30),
		Numpad9 = ScanCode.Numpad9 | (1 << 30),
		Numpad0 = ScanCode.Numpad0 | (1 << 30),
		NumpadPeriod = ScanCode.NumpadPeriod | (1 << 30),
		Application = ScanCode.Application | (1 << 30),
		Power = ScanCode.Power | (1 << 30),
		NumpadEquals = ScanCode.NumpadEquals | (1 << 30),
		F13 = ScanCode.F13 | (1 << 30),
		F14 = ScanCode.F14 | (1 << 30),
		F15 = ScanCode.F15 | (1 << 30),
		F16 = ScanCode.F16 | (1 << 30),
		F17 = ScanCode.F17 | (1 << 30),
		F18 = ScanCode.F18 | (1 << 30),
		F19 = ScanCode.F19 | (1 << 30),
		F20 = ScanCode.F20 | (1 << 30),
		F21 = ScanCode.F21 | (1 << 30),
		F22 = ScanCode.F22 | (1 << 30),
		F23 = ScanCode.F23 | (1 << 30),
		F24 = ScanCode.F24 | (1 << 30),
		Execute = ScanCode.Execute | (1 << 30),
		Help = ScanCode.Help | (1 << 30),
		Menu = ScanCode.Menu | (1 << 30),
		Select = ScanCode.Select | (1 << 30),
		Stop = ScanCode.Stop | (1 << 30),
		Again = ScanCode.Again | (1 << 30),
		Undo = ScanCode.Undo | (1 << 30),
		Cut = ScanCode.Cut | (1 << 30),
		Copy = ScanCode.Copy | (1 << 30),
		Paste = ScanCode.Paste | (1 << 30),
		Find = ScanCode.Find | (1 << 30),
		Mute = ScanCode.Mute | (1 << 30),
		VolumeUp = ScanCode.VolumeUp | (1 << 30),
		VolumeDown = ScanCode.VolumeDown | (1 << 30),
		NumpadComma = ScanCode.NumpadComma | (1 << 30),
		NumpadEqualsAs400 = ScanCode.NumpadEqualsAs400 | (1 << 30),
		AltErase = ScanCode.AltErase | (1 << 30),
		SysReq = ScanCode.SysReq | (1 << 30),
		Cancel = ScanCode.Cancel | (1 << 30),
		Clear = ScanCode.Clear | (1 << 30),
		Prior = ScanCode.Prior | (1 << 30),
		Return2 = ScanCode.Return2 | (1 << 30),
		Separator = ScanCode.Separator | (1 << 30),
		Out = ScanCode.Out | (1 << 30),
		Oper = ScanCode.Oper | (1 << 30),
		ClearAgain = ScanCode.ClearAgain | (1 << 30),
		Crsel = ScanCode.Crsel | (1 << 30),
		Exsel = ScanCode.Exsel | (1 << 30),
		Numpad00 = ScanCode.Numpad00 | (1 << 30),
		Numpad000 = ScanCode.Numpad000 | (1 << 30),
		ThousandsSeparator = ScanCode.ThousandsSeparator | (1 << 30),
		DecimalSeparator = ScanCode.DecimalSeparator | (1 << 30),
		CurrencyUnit = ScanCode.CurrencyUnit | (1 << 30),
		CurrencySubUnit = ScanCode.CurrencySubUnit | (1 << 30),
		NumpadLeftParen = ScanCode.NumpadLeftParen | (1 << 30),
		NumpadRightParen = ScanCode.NumpadRightParen | (1 << 30),
		NumpadLeftBrace = ScanCode.NumpadLeftBrace | (1 << 30),
		NumpadRightBrace = ScanCode.NumpadRightBrace | (1 << 30),
		NumpadTab = ScanCode.NumpadTab | (1 << 30),
		NumpadBackspace = ScanCode.NumpadBackspace | (1 << 30),
		NumpadA = ScanCode.NumpadA | (1 << 30),
		NumpadB = ScanCode.NumpadB | (1 << 30),
		NumpadC = ScanCode.NumpadC | (1 << 30),
		NumpadD = ScanCode.NumpadD | (1 << 30),
		NumpadE = ScanCode.NumpadE | (1 << 30),
		NumpadF = ScanCode.NumpadF | (1 << 30),
		NumpadXor = ScanCode.NumpadXor | (1 << 30),
		NumpadPower = ScanCode.NumpadPower | (1 << 30),
		NumpadPercent = ScanCode.NumpadPercent | (1 << 30),
		NumpadLess = ScanCode.NumpadLess | (1 << 30),
		NumpadGreater = ScanCode.NumpadGreater | (1 << 30),
		NumpadAmpersand = ScanCode.NumpadAmpersand | (1 << 30),
		NumpadDblAmpersand = ScanCode.NumpadDblAmpersand | (1 << 30),
		NumpadVerticalBar = ScanCode.NumpadVerticalBar | (1 << 30),
		NumpadDblVerticalBar = ScanCode.NumpadDblVerticalBar | (1 << 30),
		NumpadColon = ScanCode.NumpadColon | (1 << 30),
		NumpadHash = ScanCode.NumpadHash | (1 << 30),
		NumpadSpace = ScanCode.NumpadSpace | (1 << 30),
		NumpadAt = ScanCode.NumpadAt | (1 << 30),
		NumpadExclam = ScanCode.NumpadExclam | (1 << 30),
		NumpadMemStore = ScanCode.NumpadMemStore | (1 << 30),
		NumpadMemRecall = ScanCode.NumpadMemRecall | (1 << 30),
		NumpadMemClear = ScanCode.NumpadMemClear | (1 << 30),
		NumpadMemAdd = ScanCode.NumpadMemAdd | (1 << 30),
		NumpadMemSubtract = ScanCode.NumpadMemSubtract | (1 << 30),
		NumpadMemMultiply = ScanCode.NumpadMemMultiply | (1 << 30),
		NumpadMemDivide = ScanCode.NumpadMemDivide | (1 << 30),
		NumpadPlusMinus = ScanCode.NumpadPlusMinus | (1 << 30),
		NumpadClear = ScanCode.NumpadClear | (1 << 30),
		NumpadClearEntry = ScanCode.NumpadClearEntry | (1 << 30),
		NumpadBinary = ScanCode.NumpadBinary | (1 << 30),
		NumpadOctal = ScanCode.NumpadOctal | (1 << 30),
		NumpadDecimal = ScanCode.NumpadDecimal | (1 << 30),
		NumpadHexadecimal = ScanCode.NumpadHexadecimal | (1 << 30),
		LeftControl = ScanCode.LeftControl | (1 << 30),
		LeftShift = ScanCode.LeftShift | (1 << 30),
		LeftAlt = ScanCode.LeftAlt | (1 << 30),
		LeftSystem = ScanCode.LeftSystem | (1 << 30),
		RightControl = ScanCode.RightControl | (1 << 30),
		RightShift = ScanCode.RightShift | (1 << 30),
		RightAlt = ScanCode.RightAlt | (1 << 30),
		RightSystem = ScanCode.RightSystem | (1 << 30),
		Mode = ScanCode.Mode | (1 << 30),
		AudioNext = ScanCode.AudioNext | (1 << 30),
		AudioPrev = ScanCode.AudioPrev | (1 << 30),
		AudioStop = ScanCode.AudioStop | (1 << 30),
		AudioPlay = ScanCode.AudioPlay | (1 << 30),
		AudioMute = ScanCode.AudioMute | (1 << 30),
		MediaSelect = ScanCode.MediaSelect | (1 << 30),
		Www = ScanCode.Www | (1 << 30),
		Mail = ScanCode.Mail | (1 << 30),
		Calculator = ScanCode.Calculator | (1 << 30),
		Computer = ScanCode.Computer | (1 << 30),
		ActionSearch = ScanCode.ActionSearch | (1 << 30),
		ActionHome = ScanCode.ActionHome | (1 << 30),
		ActionBack = ScanCode.ActionBack | (1 << 30),
		ActionForward = ScanCode.ActionForward | (1 << 30),
		ActionStop = ScanCode.ActionStop | (1 << 30),
		ActionRefresh = ScanCode.ActionRefresh | (1 << 30),
		ActionBookmarks = ScanCode.ActionBookmarks | (1 << 30),
		BrightnessDown = ScanCode.BrightnessDown | (1 << 30),
		BrightnessUp = ScanCode.BrightnessUp | (1 << 30),
		DisplaySwitch = ScanCode.DisplaySwitch | (1 << 30),
		KbdillumToggle = ScanCode.KbdillumToggle | (1 << 30),
		KbdillumDown = ScanCode.KbdillumDown | (1 << 30),
		KbdillumUp = ScanCode.KbdillumUp | (1 << 30),
		Eject = ScanCode.Eject | (1 << 30),
		Sleep = ScanCode.Sleep | (1 << 30)
	}
}