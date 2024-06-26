﻿namespace OrbsHavoc.UserInterface.Input
{
	using Utilities;

	/// <summary>
	///     Provides information about mouse button press and release events.
	/// </summary>
	internal sealed class MouseButtonEventArgs : MouseEventArgs
	{
		/// <summary>
		///     A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseButtonEventArgs _cachedInstance = new MouseButtonEventArgs();

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		private MouseButtonEventArgs()
		{
		}

		/// <summary>
		///     Gets the mouse button that was pressed or released.
		/// </summary>
		public MouseButton Button { get; private set; }

		/// <summary>
		///     Gets a value indicating whether the mouse press was a double click.
		/// </summary>
		public bool DoubleClick { get; private set; }

		/// <summary>
		///     Gets the state of the mouse button that was pressed or released.
		/// </summary>
		public InputState ButtonState => Mouse[Button];

		/// <summary>
		///     Indicates whether the event was raised because of a mouse button being released or pressed.
		/// </summary>
		public InputEventKind Kind { get; private set; }

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="mouse">The mouse device that raised the event.</param>
		/// <param name="keyboard">The keyboard that raised the event.</param>
		/// <param name="button">The mouse button that was pressed or released.</param>
		/// <param name="doubleClick">Indicates whether the mouse press was a double click.</param>
		/// <param name="kind">Indicates whether the event was raised because of a mouse button being released or pressed.</param>
		internal static MouseButtonEventArgs Create(Mouse mouse, Keyboard keyboard, MouseButton button, bool doubleClick, InputEventKind kind)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentInRange(button, nameof(button));
			Assert.ArgumentInRange(kind, nameof(kind));

			_cachedInstance.Handled = false;
			_cachedInstance.Mouse = mouse;
			_cachedInstance.Keyboard = keyboard;
			_cachedInstance.Button = button;
			_cachedInstance.DoubleClick = doubleClick;
			_cachedInstance.Kind = kind;

			return _cachedInstance;
		}
	}
}