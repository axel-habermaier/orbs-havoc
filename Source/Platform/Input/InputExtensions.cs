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
	using Utilities;

	/// <summary>
	///   Provides extension methods for the input system.
	/// </summary>
	public static class InputExtensions
	{
		/// <summary>
		///   Creates a trigger that triggers when the key is released.
		/// </summary>
		public static InputTrigger IsReleased(this Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new KeyTrigger(KeyTriggerType.Released, key);
		}

		/// <summary>
		///   Creates a trigger that triggers when the key is pressed.
		/// </summary>
		public static InputTrigger IsPressed(this Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new KeyTrigger(KeyTriggerType.Pressed, key);
		}

		/// <summary>
		///   Creates a trigger that triggers when the key is repeated.
		/// </summary>
		public static InputTrigger IsRepeated(this Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new KeyTrigger(KeyTriggerType.Repeated, key);
		}

		/// <summary>
		///   Creates a trigger that triggers when the key went down.
		/// </summary>
		public static InputTrigger WentDown(this Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new KeyTrigger(KeyTriggerType.WentDown, key);
		}

		/// <summary>
		///   Creates a trigger that triggers when the key went up.
		/// </summary>
		public static InputTrigger WentUp(this Key key)
		{
			Assert.ArgumentInRange(key, nameof(key));
			return new KeyTrigger(KeyTriggerType.WentUp, key);
		}

		/// <summary>
		///   Creates a trigger that triggers when the mouse button is released.
		/// </summary>
		public static InputTrigger IsReleased(this MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return new MouseTrigger(MouseTriggerType.Released, button);
		}

		/// <summary>
		///   Creates a trigger that triggers when the mouse button is pressed.
		/// </summary>
		public static InputTrigger IsPressed(this MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return new MouseTrigger(MouseTriggerType.Pressed, button);
		}

		/// <summary>
		///   Creates a trigger that triggers when the mouse button went down.
		/// </summary>
		public static InputTrigger WentDown(this MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return new MouseTrigger(MouseTriggerType.WentDown, button);
		}

		/// <summary>
		///   Creates trigger that triggers when the mouse button went up.
		/// </summary>
		public static InputTrigger WentUp(this MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return new MouseTrigger(MouseTriggerType.WentUp, button);
		}

		/// <summary>
		///   Creates trigger that triggers when the mouse has been double-clicked.
		/// </summary>
		public static InputTrigger DoubleClicked(this MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return new MouseTrigger(MouseTriggerType.DoubleClicked, button);
		}

		/// <summary>
		///   Converts the given binary input trigger type into its corresponding expression string.
		/// </summary>
		/// <param name="triggerType">The binary input trigger type that should be converted.</param>
		internal static string ToExpressionString(this BinaryInputTriggerType triggerType)
		{
			Assert.ArgumentInRange(triggerType, nameof(triggerType));
			switch (triggerType)
			{
				case BinaryInputTriggerType.ChordOnce:
					return "+";
				case BinaryInputTriggerType.Chord:
					return "&";
				case BinaryInputTriggerType.Alias:
					return "|";
				default:
					return "<unknown>";
			}
		}
	}
}