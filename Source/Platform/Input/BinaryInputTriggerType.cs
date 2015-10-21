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

namespace PointWars.Platform.Input
{
	/// <summary>
	///   Determines the type of a binary input trigger.
	/// </summary>
	internal enum BinaryInputTriggerType
	{
		/// <summary>
		///   Indicates that the binary input trigger represents a chord, i.e., a trigger that triggers if and only
		///   if both of its constituting triggers trigger.
		/// </summary>
		Chord,

		/// <summary>
		///   Indicates that the binary input trigger represents a chord that triggers only for the first frame in which both of
		///   its sub-triggers trigger. The chord triggers again only after at least one of its two sub-triggers has not
		///   triggered for the duration of at least one frame.
		/// </summary>
		ChordOnce,

		/// <summary>
		///   Indicates that the binary input trigger represents an input alias, i.e., a trigger that triggers if and
		///   only if at least one of its two constituting triggers triggers.
		/// </summary>
		Alias
	}
}