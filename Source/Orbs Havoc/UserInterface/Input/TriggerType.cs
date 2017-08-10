namespace OrbsHavoc.UserInterface.Input
{
	/// <summary>
	///   Determines the type of a key or mouse button input trigger.
	/// </summary>
	internal enum TriggerType
	{
		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button is released.
		/// </summary>
		Released,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button is pressed.
		/// </summary>
		Pressed,

		/// <summary>
		///   Indicates that the trigger triggers when the key is repeated.
		/// </summary>
		Repeated,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button went down.
		/// </summary>
		WentDown,

		/// <summary>
		///   Indicates that the trigger triggers when the key or mouse button went up.
		/// </summary>
		WentUp
	}
}