namespace OrbsHavoc.Platform
{
	/// <summary>
	///   Indicates the whether the window is minimized, maximized, or neither minimized nor maximized.
	/// </summary>
	public enum WindowMode
	{
		/// <summary>
		///   Indicates that the window is neither minimized nor maximized.
		/// </summary>
		Normal,

		/// <summary>
		///   Indicates that the window is maximized, filling the entire screen.
		/// </summary>
		Maximized,

		/// <summary>
		///   Indicates that the window is minimized and invisible.
		/// </summary>
		Minimized,

		/// <summary>
		///   Indicates that the window is in borderless fullscreen mode, filling the entire screen.
		/// </summary>
		Fullscreen
	}
}