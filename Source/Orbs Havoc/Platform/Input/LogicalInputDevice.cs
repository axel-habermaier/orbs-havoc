namespace OrbsHavoc.Platform.Input
{
	using System.Collections.Generic;
	using Logging;
	using Memory;
	using Utilities;

	/// <summary>
	///   Manages logical inputs that are triggered by input triggers.
	/// </summary>
	public class LogicalInputDevice : DisposableObject
	{
		/// <summary>
		///   The logical inputs that are currently registered on the device.
		/// </summary>
		private readonly List<LogicalInput> _inputs = new List<LogicalInput>();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that should be associated with this logical device.</param>
		public LogicalInputDevice(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			Window = window;
			Keyboard = new Keyboard(window);
			Mouse = new Mouse(window);
		}

		/// <summary>
		///   Gets the window the input device belongs to.
		/// </summary>
		public Window Window { get; }

		/// <summary>
		///   Gets the keyboard that is associated with this logical device.
		/// </summary>
		public Keyboard Keyboard { get; }

		/// <summary>
		///   Gets the mouse that is associated with this logical device.
		/// </summary>
		public Mouse Mouse { get; }

		/// <summary>
		///   Registers a logical input on the logical input device. Subsequently, the logical input's IsTriggered
		///   property can be used to determine whether the logical input is currently triggered.
		/// </summary>
		/// <param name="input">The logical input that should be registered on the device.</param>
		public void Add(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(!input.IsRegistered, nameof(input), "The input is already registered on a device.");

			_inputs.Add(input);
			input.SetLogicalDevice(this);

			Log.Debug($"A logical input with trigger '{input.Trigger}' has been registered.");
		}

		/// <summary>
		///   Removes the logical input from the logical input device.
		/// </summary>
		/// <param name="input">The logical input that should be removed.</param>
		public void Remove(LogicalInput input)
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentSatisfies(input.IsRegistered, nameof(input), "The input trigger is not registered.");

			if (_inputs.Remove(input))
				input.SetLogicalDevice(null);
		}

		/// <summary>
		///   Updates the device state.
		/// </summary>
		internal void Update()
		{
			if (!Keyboard.TextInputEnabled)
			{
				foreach (var input in _inputs)
					input.Update();
			}

			Keyboard.Update();
			Mouse.Update();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			while (_inputs.Count > 0)
				Remove(_inputs[0]);

			Keyboard.SafeDispose();
			Mouse.SafeDispose();
		}
	}
}