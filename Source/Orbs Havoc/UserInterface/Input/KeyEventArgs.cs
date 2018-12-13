namespace OrbsHavoc.UserInterface.Input
{
	using Utilities;

	/// <summary>
	///   Provides information about key press and release events.
	/// </summary>
	internal sealed class KeyEventArgs : InputEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly KeyEventArgs _cachedInstance = new KeyEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private KeyEventArgs()
		{
		}

		/// <summary>
		///   Gets the key that was pressed or released. The key depends on the keyboard layout.
		/// </summary>
		public Key Key { get; private set; }

		/// <summary>
		///   Gets the key's scan code. The scan code is independent of the keyboard layout but may differ between
		///   operating systems.
		/// </summary>
		public ScanCode ScanCode { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button is currently being pressed down.
		/// </summary>
		public InputState KeyState => Keyboard[ScanCode];

	

		/// <summary>
		///   Indicates whether the event was raised because of a key being released or pressed.
		/// </summary>
		public InputEventKind Kind { get; private set; }

		/// <summary>
		///   Initializes a cached instance.
		/// </summary>
		/// <param name="keyboard">The keyboard device that raised the event.</param>
		/// <param name="mouse">The mouse that belongs to the event.</param>
		/// <param name="key">The key that was pressed or released.</param>
		/// <param name="scanCode">The key's scan code.</param>
		/// <param name="kind">Indicates whether the event was raised because of a key being released or pressed.</param>
		internal static KeyEventArgs Create(Keyboard keyboard, Mouse mouse, Key key, ScanCode scanCode, InputEventKind kind)
		{
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentNotNull(mouse, nameof(mouse));
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			Assert.ArgumentInRange(kind, nameof(kind));

			_cachedInstance.Handled = false;
			_cachedInstance.Keyboard = keyboard;
			_cachedInstance.Mouse = mouse;
			_cachedInstance.Key = key;
			_cachedInstance.ScanCode = scanCode;
			_cachedInstance.Kind = kind;

			return _cachedInstance;
		}
	}
}