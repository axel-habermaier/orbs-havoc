namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents an input binding that is triggered by a key event, identifying the key by its scan code.
	/// </summary>
	internal sealed class ScanCodeBinding : InputBinding
	{
		/// <summary>
		///   The key modifiers that must be pressed for the binding to trigger.
		/// </summary>
		private readonly KeyModifiers _modifiers;

		/// <summary>
		///   The scan code of the key that activates the binding.
		/// </summary>
		private readonly ScanCode _scanCode;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="scanCode">The scan code of the key that should activate the binding.</param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		public ScanCodeBinding(Action callback, ScanCode scanCode, KeyModifiers modifiers = KeyModifiers.None)
			: base(callback)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));

			_scanCode = scanCode;
			_modifiers = modifiers;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected override bool IsTriggered(InputEventArgs args)
		{
			if (!(args is KeyEventArgs keyEventArgs))
				return false;

			switch (TriggerMode)
			{
				case TriggerMode.OnActivation:
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.ScanCode == _scanCode &&
						   keyEventArgs.Modifiers == _modifiers && keyEventArgs.KeyState.WentDown;
				case TriggerMode.Repeatedly:
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.ScanCode == _scanCode &&
						   keyEventArgs.Modifiers == _modifiers && keyEventArgs.KeyState.IsRepeated;
				case TriggerMode.OnDeactivation:
					return keyEventArgs.Kind == InputEventKind.Up && keyEventArgs.ScanCode == _scanCode && keyEventArgs.Modifiers == _modifiers;
				default:
					Assert.NotReached("Unknown trigger mode.");
					return false;
			}
		}
	}
}