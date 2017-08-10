namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Input;
	using Utilities;

	/// <summary>
	///   Represents an input binding that is triggered by a key event.
	/// </summary>
	internal sealed class KeyBinding : InputBinding
	{
		private readonly Key _key;
		private readonly KeyModifiers _modifiers;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="key">The key that should activate the binding.</param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		public KeyBinding(Action callback, Key key, KeyModifiers modifiers = KeyModifiers.None)
			: base(callback)
		{
			Assert.ArgumentInRange(key, nameof(key));

			_key = key;
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
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.KeyState.WentDown &&
						   keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers;
				case TriggerMode.Repeatedly:
					return keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.KeyState.IsRepeated &&
						   keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers;
				case TriggerMode.OnDeactivation:
					return keyEventArgs.Kind == InputEventKind.Up && keyEventArgs.Key == _key && keyEventArgs.Modifiers == _modifiers;
				default:
					Assert.NotReached("Unknown trigger mode.");
					return false;
			}
		}
	}
}