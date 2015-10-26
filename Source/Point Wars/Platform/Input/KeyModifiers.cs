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

namespace PointWars.Platform.Input
{
	using System;

	/// <summary>
	///   Specifies a set of key modifiers.
	/// </summary>
	[Flags]
	public enum KeyModifiers : ushort
	{
		None = 0x0000,
		LeftShift = 0x0001,
		RightShift = 0x0002,
		LeftControl = 0x0040,
		RightControl = 0x0080,
		LeftAlt = 0x0100,
		RightAlt = 0x0200,
		LeftGui = 0x0400,
		RightGui = 0x0800,
		Num = 0x1000,
		Caps = 0x2000,
		Mode = 0x4000,
		Reserved = 0x8000,

		Control = LeftControl | RightControl,
		Shift = LeftShift | RightShift,
		Alt = LeftAlt | RightAlt,
		Gui = LeftGui | RightGui
	}
}