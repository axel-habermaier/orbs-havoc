namespace OrbsHavoc.UserInterface.Input
{
	/// <summary>
	///     Describes the trigger mode of an input binding.
	/// </summary>
	internal enum TriggerMode
	{
		/// <summary>
		///     The input binding is triggered when the input is activated.
		/// </summary>
		OnActivation,

		/// <summary>
		///     The input binding is triggered when the input is activated or repeated.
		/// </summary>
		Repeatedly,

		/// <summary>
		///     The input binding is triggered when the input is deactivated.
		/// </summary>
		OnDeactivation,
	}
}