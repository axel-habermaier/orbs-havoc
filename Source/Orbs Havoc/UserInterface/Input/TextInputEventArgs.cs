namespace OrbsHavoc.UserInterface.Input
{
	/// <summary>
	///   Provides information about text input.
	/// </summary>
	internal class TextInputEventArgs : InputEventArgs
	{
		/// <summary>
		///   A cached instance of the event argument class that should be used to reduce the pressure on the garbage collector.
		/// </summary>
		private static readonly TextInputEventArgs _cachedInstance = new TextInputEventArgs();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private TextInputEventArgs()
		{
		}

		/// <summary>
		///   Gets the text that was entered.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		///   Initializes a cached instance.
		/// </summary>
		/// <param name="text">The text that was entered.</param>
		internal static TextInputEventArgs Create(string text)
		{
			_cachedInstance.Handled = false;
			_cachedInstance.Text = text;

			return _cachedInstance;
		}
	}
}