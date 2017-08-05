namespace OrbsHavoc.UserInterface.Input
{
	using System;

	/// <summary>
	///     Specifies a set of key modifiers.
	/// </summary>
	[Flags]
	public enum KeyModifiers
	{
		None = 0,
		Control = 1,
		Shift = 2,
		Alt = 4
	}
}