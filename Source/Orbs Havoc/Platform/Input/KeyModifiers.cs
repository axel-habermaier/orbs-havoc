namespace OrbsHavoc.Platform.Input
{
	using System;

	/// <summary>
	///     Specifies a set of key modifiers.
	/// </summary>
	[Flags]
	public enum KeyModifiers : ushort
	{
		None = 0x0000,
		Control = 0x0040 | 0x0080,
		Shift = 0x0001 | 0x0002,
		Alt = 0x0100 | 0x0200
	}
}