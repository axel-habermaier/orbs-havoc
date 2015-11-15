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

namespace OrbsHavoc.Platform.Input
{
	using Utilities;

	/// <summary>
	///   Represents an input trigger that is triggered if a logical input device is in a certain state.
	/// </summary>
	public abstract class InputTrigger
	{
		/// <summary>
		///   Evaluates the trigger, returning true to indicate that the trigger has fired.
		/// </summary>
		/// <param name="device">The logical input device that should be used to evaluate the trigger.</param>
		internal abstract bool Evaluate(LogicalInputDevice device);

		/// <summary>
		///   Sets the logical input device the logical input is currently registered on.
		/// </summary>
		/// <param name="device">
		///   The logical input device the logical input is currently registered on. Null should be passed to
		///   indicate that the logical input is currently not registered on any device.
		/// </param>
		internal virtual void SetLogicalDevice(LogicalInputDevice device)
		{
		}

		/// <summary>
		///   Constructs a chord, i.e., a trigger that triggers if and only if both of its constituting triggers trigger.
		/// </summary>
		/// <param name="left">The first sub-trigger.</param>
		/// <param name="right">The second sub-trigger.</param>
		public static InputTrigger operator &(InputTrigger left, InputTrigger right)
		{
			Assert.ArgumentNotNull(left, nameof(left));
			Assert.ArgumentNotNull(right, nameof(right));

			return new BinaryInputTrigger(BinaryInputTriggerType.Chord, left, right);
		}

		/// <summary>
		///   Constructs a chord that triggers only for the first frame in which both of its sub-triggers trigger. The chord
		///   triggers again only after at least one of its two sub-triggers has not triggered for the duration of at least
		///   one frame.
		/// </summary>
		/// <param name="left">The first sub-trigger.</param>
		/// <param name="right">The second sub-trigger.</param>
		public static InputTrigger operator +(InputTrigger left, InputTrigger right)
		{
			Assert.ArgumentNotNull(left, nameof(left));
			Assert.ArgumentNotNull(right, nameof(right));

			return new BinaryInputTrigger(BinaryInputTriggerType.ChordOnce, left, right);
		}

		/// <summary>
		///   Constructs an input alias, i.e., a trigger that triggers if and only if at least one of its two
		///   constituting triggers trigger.
		/// </summary>
		/// <param name="left">The first sub-trigger.</param>
		/// <param name="right">The second sub-trigger.</param>
		public static InputTrigger operator |(InputTrigger left, InputTrigger right)
		{
			Assert.ArgumentNotNull(left, nameof(left));
			Assert.ArgumentNotNull(right, nameof(right));

			return new BinaryInputTrigger(BinaryInputTriggerType.Alias, left, right);
		}
	}
}