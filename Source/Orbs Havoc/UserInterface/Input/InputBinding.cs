namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents an input binding that invokes a callback whenever it is triggered.
	/// </summary>
	internal abstract class InputBinding
	{
		/// <summary>
		///   The callback that is invoked when the binding is triggered.
		/// </summary>
		private readonly Action _callback;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="callback">The callback that should be invoked when the binding is triggered.</param>
		protected InputBinding(Action callback)
		{
			Assert.ArgumentNotNull(callback, nameof(callback));
			_callback = callback;
		}

		/// <summary>
		///   Gets or sets a value indicating whether the preview version of the corresponding input event should be used.
		/// </summary>
		public bool Preview { get; set; }

		/// <summary>
		///   Gets or sets the trigger mode of the input binding.
		/// </summary>
		public TriggerMode TriggerMode { get; set; } = TriggerMode.OnActivation;

		/// <summary>
		///   Handles the given event, invoking the target method if the input binding is triggered.
		/// </summary>
		/// <param name="args">The arguments of the event that should be handled.</param>
		/// <param name="isPreview">Indicates whether the event is a preview event.</param>
		internal void HandleEvent(InputEventArgs args, bool isPreview)
		{
			Assert.ArgumentNotNull(args, nameof(args));

			if (Preview != isPreview || !IsTriggered(args))
				return;

			_callback();
			args.Handled = true;
		}

		/// <summary>
		///   Checks whether the given event triggers the input binding.
		/// </summary>
		/// <param name="args">The arguments of the event that should be checked.</param>
		protected abstract bool IsTriggered(InputEventArgs args);
	}
}