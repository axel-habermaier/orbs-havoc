﻿// The MIT License (MIT)
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

namespace PointWars.Platform.Input
{
	using System;

	/// <summary>
	///   Represents the state of an input key or button.
	/// </summary>
	public struct InputState : IEquatable<InputState>
	{
		/// <summary>
		///   Gets a value indicating whether the key or button is currently being pressed down.
		/// </summary>
		public bool IsPressed { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		public bool WentDown { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		public bool WentUp { get; private set; }

		/// <summary>
		///   Gets a value indicating whether a key or button repeat event occurred. IsRepeated is also true
		///   when the key or button is pressed, i.e., when WentDown is true.
		/// </summary>
		public bool IsRepeated { get; private set; }

		/// <summary>
		///   Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		///   true if the current object is equal to other; otherwise, false.
		/// </returns>
		public bool Equals(InputState other)
		{
			return IsPressed == other.IsPressed && IsRepeated == other.IsRepeated && WentDown == other.WentDown &&
				   WentUp == other.WentUp;
		}

		/// <summary>
		///   Updates the input state when the key or button has been pressed.
		/// </summary>
		internal void Pressed()
		{
			WentDown = !IsPressed;
			IsPressed = true;
			WentUp = false;
			IsRepeated = true;
		}

		/// <summary>
		///   Updates the input state when the key or button has been released.
		/// </summary>
		internal void Released()
		{
			WentUp = IsPressed;
			IsPressed = false;
			IsRepeated = false;
		}

		/// <summary>
		///   Ensures that WentDown, WentUp, and IsRepeated only remain true for one single frame, even if the actual
		///   key or button state has not changed.
		/// </summary>
		internal void Update()
		{
			WentDown = false;
			WentUp = false;
			IsRepeated = false;
		}
	}
}