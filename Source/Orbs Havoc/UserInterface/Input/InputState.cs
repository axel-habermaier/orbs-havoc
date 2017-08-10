namespace OrbsHavoc.UserInterface.Input
{
	/// <summary>
	///   Represents the state of an input key or button.
	/// </summary>
	internal struct InputState
	{
		/// <summary>
		///   Gets a value indicating whether the key or button is currently being pressed down.
		/// </summary>
		public bool IsPressed { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button was pressed during the current frame. WentDown is
		///   only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		public bool WentDown { get; private set; }

		/// <summary>
		///   Gets a value indicating whether the key or button was released during the current frame. WentUp is
		///   only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		public bool WentUp { get; private set; }

		/// <summary>
		///   Gets a value indicating whether a key or button repeat event occurred. IsRepeated is also true
		///   when the key or button is pressed, i.e., when WentDown is true.
		/// </summary>
		public bool IsRepeated { get; private set; }

		/// <summary>
		///   Updates the input state when the key or button has been pressed.
		/// </summary>
		internal void Pressed()
		{
			WentDown = !IsPressed;
			IsPressed = true;
			WentUp = false;
			IsRepeated = true;
		}

		/// <summary>
		///   Updates the input state when the key or button has been released.
		/// </summary>
		internal void Released()
		{
			WentUp = IsPressed;
			IsPressed = false;
			IsRepeated = false;
		}

		/// <summary>
		///   Ensures that WentDown, WentUp, and IsRepeated only remain true for one single frame, even if the actual
		///   key or button state has not changed.
		/// </summary>
		internal void Update()
		{
			WentDown = false;
			WentUp = false;
			IsRepeated = false;
		}
	}
}