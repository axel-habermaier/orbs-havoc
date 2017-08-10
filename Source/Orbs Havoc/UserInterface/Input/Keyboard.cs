namespace OrbsHavoc.UserInterface.Input
{
	using Platform;
	using Platform.Memory;
	using Utilities;
	using static Platform.SDL2;

	/// <summary>
	///     Represents the state of the keyboard.
	/// </summary>
	internal sealed class Keyboard : DisposableObject
	{
		/// <summary>
		///     The key states.
		/// </summary>
		private readonly InputState[] _states = new InputState[SDL_NUM_SCANCODES];

		/// <summary>
		///     The window that generates the key events.
		/// </summary>
		private readonly Window _window;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that generates the key events.</param>
		internal Keyboard(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.KeyPressed += OnKeyPressed;
			_window.KeyReleased += OnKeyReleased;
		}

		/// <summary>
		///     Gets the input state for the given button.
		/// </summary>
		/// <param name="scanCode">The scan code the input state should be returned for.</param>
		public InputState this[ScanCode scanCode]
		{
			get
			{
				Assert.ArgumentInRange(scanCode, nameof(scanCode));
				return _states[(int)scanCode];
			}
		}

		/// <summary>
		///     Get or sets a value indicating whether text input is enabled for the currently focused window.
		/// </summary>
		internal static bool TextInputEnabled
		{
			get => SDL_IsTextInputActive() != 0;
			set
			{
				if (value && !TextInputEnabled)
					SDL_StartTextInput();
				else if (!value && TextInputEnabled)
					SDL_StopTextInput();
			}
		}

		/// <summary>
		///     Gets the modifiers that are currently set.
		/// </summary>
		public KeyModifiers Modifiers { get; private set; }

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_window.KeyPressed -= OnKeyPressed;
			_window.KeyReleased -= OnKeyReleased;
		}

		/// <summary>
		///     Invoked when a key has been released.
		/// </summary>
		private void OnKeyReleased(Key key, ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			_states[(int)scanCode].Released();
		}

		/// <summary>
		///     Invoked when a key has been pressed.
		/// </summary>
		private void OnKeyPressed(Key key, ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			_states[(int)scanCode].Pressed();
		}

		/// <summary>
		///     Changes the text input area.
		/// </summary>
		/// <param name="area">The new text input area.</param>
		internal static void ChangeTextInputArea(Rectangle area)
		{
			SDL_Rect rect;
			rect.x = MathUtils.RoundIntegral(area.Left);
			rect.y = MathUtils.RoundIntegral(area.Top);
			rect.w = MathUtils.RoundIntegral(area.Width);
			rect.h = MathUtils.RoundIntegral(area.Height);

			SDL_SetTextInputRect(ref rect);
		}

		/// <summary>
		///     Updates the keyboard state.
		/// </summary>
		internal void Update()
		{
			for (var i = 0; i < _states.Length; ++i)
				_states[i].Update();

			Modifiers = GetModifiers();
		}

		/// <summary>
		///     Gets a value indicating whether the key is currently being pressed down.
		/// </summary>
		/// <param name="scanCode">The scan code of the key that should be checked.</param>
		public bool IsPressed(ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			return _states[(int)scanCode].IsPressed;
		}

		/// <summary>
		///     Gets a value indicating whether the key was pressed during the current frame. WentDown is
		///     only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="scanCode">The scan code of the key that should be checked.</param>
		public bool WentDown(ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			return _states[(int)scanCode].WentDown;
		}

		/// <summary>
		///     Gets a value indicating whether the key was released during the current frame. WentUp is
		///     only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="scanCode">The scan code of the key that should be checked.</param>
		public bool WentUp(ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			return _states[(int)scanCode].WentUp;
		}

		/// <summary>
		///     Gets a value indicating whether a system key repeat event occurred. IsRepeated is also true
		///     when the key is pressed, i.e., when WentDown is true.
		/// </summary>
		/// <param name="scanCode">The scan code of the key that should be checked.</param>
		public bool IsRepeated(ScanCode scanCode)
		{
			Assert.ArgumentInRange(scanCode, nameof(scanCode));
			return _states[(int)scanCode].IsRepeated;
		}

		/// <summary>
		///     Gets a value indicating whether the key is currently being pressed down.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsPressed(Key key)
		{
			return IsPressed(SDL_GetScancodeFromKey(key));
		}

		/// <summary>
		///     Gets a value indicating whether the key was pressed during the current frame. WentDown is
		///     only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentDown(Key key)
		{
			return WentDown(SDL_GetScancodeFromKey(key));
		}

		/// <summary>
		///     Gets a value indicating whether the key was released during the current frame. WentUp is
		///     only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool WentUp(Key key)
		{
			return WentUp(SDL_GetScancodeFromKey(key));
		}

		/// <summary>
		///     Gets a value indicating whether a system key repeat event occurred. IsRepeated is also true
		///     when the key is pressed, i.e., when WentDown is true.
		/// </summary>
		/// <param name="key">The key that should be checked.</param>
		public bool IsRepeated(Key key)
		{
			return IsRepeated(SDL_GetScancodeFromKey(key));
		}

		/// <summary>
		///     Gets the set of key modifiers that are currently pressed.
		/// </summary>
		private KeyModifiers GetModifiers()
		{
			var modifiers = KeyModifiers.None;

			if (IsPressed(Key.LeftAlt) || IsPressed(Key.RightAlt))
				modifiers |= KeyModifiers.Alt;

			if (IsPressed(Key.LeftControl) || IsPressed(Key.RightControl))
				modifiers |= KeyModifiers.Control;

			if (IsPressed(Key.LeftShift) || IsPressed(Key.RightShift))
				modifiers |= KeyModifiers.Shift;

			return modifiers;
		}
	}
}