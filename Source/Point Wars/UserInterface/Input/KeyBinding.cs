namespace PointWars.UserInterface.Input
{
	using System;
	using Platform.Input;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///     Represents an input binding that is triggered by a key event.
	/// </summary>
	public sealed class KeyBinding : InputBinding
	{
		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		public KeyBinding()
		{
		}

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="key">The key that should activate the binding.</param>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		public KeyBinding(Key key, Action callback, KeyModifiers modifiers = KeyModifiers.None)
		{
			Assert.ArgumentNotNull(callback, nameof(callback));

			Key = key;
			Modifiers = modifiers;
			Method = method;
		}

		/// <summary>
		///     Gets or sets the key that activates the binding.
		/// </summary>
		public Key Key { get; set; }

		/// <summary>
		///     Gets or sets the key modifiers that must be pressed for the binding to trigger.
		/// </summary>
		public KeyModifiers Modifiers { get; set; }

		/// <summary>
		///     Gets or sets a value indicating whether the binding is also be triggered when the OS reports a repeated key press.
		/// </summary>
		public bool TriggerOnRepeat { get; set; } = true;

		/// <summary>
		///     Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected override bool IsTriggered(RoutedEventArgs args)
		{
			var keyEventArgs = args as KeyEventArgs;
			if (keyEventArgs == null)
				return false;

			switch (TriggerMode)
			{
				case TriggerMode.Activated:
					var downEvent = Preview ? UIElement.PreviewKeyDownEvent : UIElement.KeyDownEvent;
					if (args.RoutedEvent == downEvent && keyEventArgs.Key == Key && keyEventArgs.Modifiers == Modifiers)
						return keyEventArgs.WentDown || (keyEventArgs.IsRepeated && TriggerOnRepeat);
					return false;
				case TriggerMode.Deactivated:
					var upEvent = Preview ? UIElement.PreviewKeyUpEvent : UIElement.KeyUpEvent;
					return args.RoutedEvent == upEvent && (keyEventArgs.Key == Key || keyEventArgs.Modifiers != Modifiers);
				default:
					Log.Die("Unknown trigger mode.");
					return false;
			}
		}
	}
}