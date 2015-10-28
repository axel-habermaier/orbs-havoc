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

namespace PointWars.UserInterface.Input
{
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides information about mouse button press and release events.
	/// </summary>
	public sealed class MouseButtonEventArgs : MouseEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseButtonEventArgs CachedInstance = new MouseButtonEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private MouseButtonEventArgs()
		{
		}

		/// <summary>
		///   Gets the mouse button that was pressed or released.
		/// </summary>
		public MouseButton Button { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the mouse press was a double click.
		/// </summary>
		public bool DoubleClick { get; private set; }

		/// <summary>
		///   Gets the state of the mouse button that was pressed or released.
		/// </summary>
		public InputState ButtonState => Mouse[Button];

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="mouse">The mouse device that raised the event.</param>
		/// <param name="button">The mouse button that was pressed or released.</param>
		/// <param name="doubleClick">Indicates whether the mouse press was a double click.</param>
		/// <param name="modifiers">The key modifiers that were pressed when the event was raised.</param>
		internal static MouseButtonEventArgs Create(Mouse mouse, MouseButton button, bool doubleClick, KeyModifiers modifiers)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentInRange(button, nameof(button));

			CachedInstance.Handled = false;
			CachedInstance.Mouse = mouse;
			CachedInstance.Button = button;
			CachedInstance.DoubleClick = doubleClick;
			CachedInstance.Modifiers = modifiers;

			return CachedInstance;
		}
	}
}