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
	using System.Numerics;
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides information about mouse events.
	/// </summary>
	public class MouseEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseEventArgs CachedInstance = new MouseEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		protected MouseEventArgs()
		{
		}

		/// <summary>
		///     Gets or sets a value indicating whether the event has been handled.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		///   Gets the mouse that generated the event.
		/// </summary>
		public Mouse Mouse { get; protected set; }

		/// <summary>
		///   Gets the position of the mouse at the time the event was generated.
		/// </summary>
		public Vector2 Position => Mouse.Position;

		/// <summary>
		///   Gets the set of key modifiers that was pressed when the event was raised.
		/// </summary>
		public KeyModifiers Modifiers { get; protected set; }

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="mouse">The mouse device that raised the event.</param>
		/// <param name="modifiers">The key modifiers that were pressed when the event was raised.</param>
		internal static MouseEventArgs Create(Mouse mouse, KeyModifiers modifiers)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));

			CachedInstance.Handled = false;
			CachedInstance.Mouse = mouse;
			CachedInstance.Modifiers = modifiers;

			return CachedInstance;
		}
	}
}