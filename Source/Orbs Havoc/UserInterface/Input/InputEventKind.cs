namespace OrbsHavoc.UserInterface.Input
{
	/// <summary>
	///   Describes the kind of an input event.
	/// </summary>
	internal enum InputEventKind
	{
		/// <summary>
		///   Indicates that the event was raised because a key or mouse button was released.
		/// </summary>
		Up,

		/// <summary>
		///   Indicates that the event was raised because a key or mouse button was pressed.
		/// </summary>
		Down
	}
}