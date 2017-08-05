namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Input;
	using Utilities;

	/// <summary>
	///   Represents an input binding that is triggered by turning the mouse wheel.
	/// </summary>
	public sealed class MouseWheelBinding : InputBinding
	{
		/// <summary>
		///   The direction the mouse wheel must be turned in to trigger the binding. If null, both directions trigger the binding.
		/// </summary>
		private readonly MouseWheelDirection? _direction;

		/// <summary>
		///   The key modifiers that must be pressed for the binding to trigger.
		/// </summary>
		private readonly KeyModifiers _modifiers;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		/// <param name="direction">
		///   The direction the mouse wheel must be turned in to trigger the binding. If null, both directions
		///   trigger the binding.
		/// </param>
		/// <param name="modifiers">The key modifiers that must be pressed for the binding to trigger.</param>
		public MouseWheelBinding(Action callback, MouseWheelDirection? direction, KeyModifiers modifiers = KeyModifiers.None)
			: base(callback)
		{
			_direction = direction;
			_modifiers = modifiers;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected override bool IsTriggered(InputEventArgs args)
		{
			if (!(args is MouseWheelEventArgs wheelEventArgs))
				return false;

			switch (_direction)
			{
				case MouseWheelDirection.Up:
					return wheelEventArgs.Direction == MouseWheelDirection.Up && wheelEventArgs.Modifiers == _modifiers;
				case MouseWheelDirection.Down:
					return wheelEventArgs.Direction == MouseWheelDirection.Down && wheelEventArgs.Modifiers == _modifiers;
				case null:
					return wheelEventArgs.Modifiers == _modifiers;
				default:
					Assert.NotReached("Unknown mouse wheel direction.");
					return false;
			}
		}
	}
}