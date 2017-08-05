namespace OrbsHavoc.UserInterface.Input
{
	using System.Numerics;
	using Platform.Input;
	using Utilities;

	/// <summary>
	///     Provides information about mouse events.
	/// </summary>
	public class MouseEventArgs : InputEventArgs
	{
		/// <summary>
		///     A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly MouseEventArgs _cachedInstance = new MouseEventArgs();

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		protected MouseEventArgs()
		{
		}

		/// <summary>
		///     Gets the position of the mouse at the time the event was generated.
		/// </summary>
		public Vector2 Position => Mouse.Position;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="mouse">The mouse device that raised the event.</param>
		/// <param name="keyboard">The keyboard that raised the event.</param>
		internal static MouseEventArgs Create(Mouse mouse, Keyboard keyboard)
		{
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));

			_cachedInstance.Handled = false;
			_cachedInstance.Mouse = mouse;
			_cachedInstance.Keyboard = keyboard;

			return _cachedInstance;
		}
	}
}