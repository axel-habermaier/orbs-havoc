namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Platform.Input;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Represents a configurable input binding.
	/// </summary>
	public sealed class ConfigurableBinding : InputBinding
	{
		/// <summary>
		///   The configurable input cvar that triggers the binding.
		/// </summary>
		private readonly Cvar<InputTrigger> _cvar;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="cvar">The configurable input cvar that should trigger the binding.</param>
		public ConfigurableBinding(Action callback, Cvar<InputTrigger> cvar)
			: base(callback)
		{
			Assert.ArgumentNotNull(cvar, nameof(cvar));
			_cvar = cvar;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected override bool IsTriggered(InputEventArgs args)
		{
			if (_cvar.Value.Key is Key key)
			{
				if (!(args is KeyEventArgs keyEventArgs))
					return false;

				switch (TriggerMode)
				{
					case TriggerMode.OnActivation:
						return keyEventArgs.Key == key && keyEventArgs.Modifiers == _cvar.Value.Modifiers &&
							   keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.KeyState.WentDown;
					case TriggerMode.Repeatedly:
						return keyEventArgs.Key == key && keyEventArgs.Modifiers == _cvar.Value.Modifiers &&
							   keyEventArgs.Kind == InputEventKind.Down && keyEventArgs.KeyState.IsRepeated;
					case TriggerMode.OnDeactivation:
						return keyEventArgs.Kind == InputEventKind.Up && keyEventArgs.Key == key &&
							   keyEventArgs.Modifiers == _cvar.Value.Modifiers;
					default:
						Assert.NotReached("Unknown trigger mode.");
						return false;
				}
			}

			if (_cvar.Value.MouseButton is MouseButton button)
			{
				if (!(args is MouseButtonEventArgs mouseEventArgs))
					return false;

				switch (TriggerMode)
				{
					case TriggerMode.OnActivation:
					case TriggerMode.Repeatedly:
						return mouseEventArgs.Kind == InputEventKind.Down &&
							   (mouseEventArgs.Button == button && mouseEventArgs.Modifiers == _cvar.Value.Modifiers);
					case TriggerMode.OnDeactivation:
						return mouseEventArgs.Kind == InputEventKind.Up &&
							   (mouseEventArgs.Button == button || mouseEventArgs.Modifiers != _cvar.Value.Modifiers);
					default:
						Assert.NotReached("Unknown trigger mode.");
						return false;
				}
			}

			return false;
		}
	}
}