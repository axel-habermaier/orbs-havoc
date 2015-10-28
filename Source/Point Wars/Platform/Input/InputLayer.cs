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
	///   Represents an input layer. The active input layer of a logical input device determines which logical inputs are
	///   triggered. Input layers are prioritized, with higher-numbered layers having higher priorities.
	/// </summary>
	[Flags]
	public enum InputLayer : uint
	{
		/// <summary>
		///   Indicates that no input layer is active.
		/// </summary>
		None = 0,

		/// <summary>
		///   The input layer used by all input to the game.
		/// </summary>
		Game = 1,

		/// <summary>
		///   The input layer used by the chat input.
		/// </summary>
		Chat = 2,

		/// <summary>
		///   The input layer used by all menus.
		/// </summary>
		Menu = 4,

		/// <summary>
		///   The input layer used by all message boxes.
		/// </summary>
		MessageBox = 8,

		/// <summary>
		///   The input layer used by the console.
		/// </summary>
		Console = 16,

		/// <summary>
		///   Represents all input layers.
		/// </summary>
		All = 0xFFFFFFFF
	}
}