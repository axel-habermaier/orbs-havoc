namespace OrbsHavoc.Platform.Input
{
	using Memory;
	using Utilities;

	/// <summary>
	///     Manages logical inputs that are triggered by input triggers.
	/// </summary>
	public class LogicalInputDevice : DisposableObject
	{
		/// <summary>
		///     Initializes a new instance.
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
		///     Gets the window the input device belongs to.
		/// </summary>
		public Window Window { get; }

		/// <summary>
		///     Gets the keyboard that is associated with this logical device.
		/// </summary>
		public Keyboard Keyboard { get; }

		/// <summary>
		///     Gets the mouse that is associated with this logical device.
		/// </summary>
		public Mouse Mouse { get; }

		/// <summary>
		///     Updates the device state.
		/// </summary>
		internal void Update()
		{
			Keyboard.Update();
			Mouse.Update();
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Keyboard.SafeDispose();
			Mouse.SafeDispose();
		}
	}
}