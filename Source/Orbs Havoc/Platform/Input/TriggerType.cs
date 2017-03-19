﻿// The MIT License (MIT)
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

namespace OrbsHavoc.Platform.Input
{
	/// <summary>
	///   Determines the type of a key or mouse button input trigger.
	/// </summary>
	public enum TriggerType
	{
		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button is released.
		/// </summary>
		Released,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button is pressed.
		/// </summary>
		Pressed,

		/// <summary>
		///   Indicates that the trigger triggers when the key is repeated.
		/// </summary>
		Repeated,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button went down.
		/// </summary>
		WentDown,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button went up.
		/// </summary>
		WentUp
	}
}