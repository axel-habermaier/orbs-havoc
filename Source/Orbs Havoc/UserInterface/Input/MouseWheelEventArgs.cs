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

namespace OrbsHavoc.UserInterface.Input
{
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides information about mouse wheel events.
	/// </summary>
	public sealed class MouseWheelEventArgs : MouseEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseWheelEventArgs CachedInstance = new MouseWheelEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private MouseWheelEventArgs()
		{
		}

		/// <summary>
		///   Gets a value indicating the direction the mouse wheel has been turned in.
		/// </summary>
		public MouseWheelDirection Direction { get; private set; }

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="mouse">The mouse device that raised the event.</param>
		/// <param name="direction">The direction the mouse wheel has been turned in.</param>
		/// <param name="modifiers">The key modifiers that were pressed when the event was raised.</param>
		internal static MouseWheelEventArgs Create(Mouse mouse, MouseWheelDirection direction, KeyModifiers modifiers)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentInRange(direction, nameof(direction));

			CachedInstance.Handled = false;
			CachedInstance.Mouse = mouse;
			CachedInstance.Direction = direction;
			CachedInstance.Modifiers = modifiers;

			return CachedInstance;
		}
	}
}