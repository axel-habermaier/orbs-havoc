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
		/// <param name="modifiers">The key modifiers that were pressed when the event was raised.</param>
		internal static MouseWheelEventArgs Create(Mouse mouse, MouseWheelDirection direction, KeyModifiers modifiers)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentInRange(direction, nameof(direction));

			_cachedInstance.Handled = false;
			_cachedInstance.Mouse = mouse;
			_cachedInstance.Direction = direction;
			_cachedInstance.Modifiers = modifiers;

			return _cachedInstance;
		}
	}
}