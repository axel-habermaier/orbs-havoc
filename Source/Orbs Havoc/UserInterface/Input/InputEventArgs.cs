namespace OrbsHavoc.UserInterface.Input
{
	using Input;

	/// <summary>
	///     Provides information about input events.
	/// </summary>
	public class InputEventArgs
	{
		/// <summary>
		///     Gets or sets a value indicating whether the event has been handled.
		/// </summary>
		public bool Handled { get; set; }

		/// <summary>
		///     Gets the set of key modifiers that was pressed when the event was raised.
		/// </summary>
		public KeyModifiers Modifiers => Keyboard.Modifiers;

		/// <summary>
		///     Gets the keyboard that generated the event.
		/// </summary>
		public Keyboard Keyboard { get; protected set; }

		/// <summary>
		///     Gets the mouse that generated the event.
		/// </summary>
		public Mouse Mouse { get; protected set; }
	}
}