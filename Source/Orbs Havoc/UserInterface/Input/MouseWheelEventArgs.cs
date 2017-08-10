namespace OrbsHavoc.UserInterface.Input
{
	using Input;
	using Utilities;

	/// <summary>
	///   Provides information about mouse wheel events.
	/// </summary>
	internal sealed class MouseWheelEventArgs : MouseEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseWheelEventArgs _cachedInstance = new MouseWheelEventArgs();

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
		/// <param name="keyboard">The keyboard that raised the event.</param>
		internal static MouseWheelEventArgs Create(Mouse mouse, Keyboard keyboard, MouseWheelDirection direction)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentInRange(direction, nameof(direction));

			_cachedInstance.Handled = false;
			_cachedInstance.Mouse = mouse;
			_cachedInstance.Keyboard = keyboard;
			_cachedInstance.Direction = direction;

			return _cachedInstance;
		}
	}
}