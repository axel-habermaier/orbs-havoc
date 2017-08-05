namespace OrbsHavoc.UserInterface.Input
{
	using System;
	using System.Numerics;
	using Platform;
	using Platform.Memory;
	using Utilities;
	using static Platform.SDL2;

	/// <summary>
	///     Represents the state of the mouse.
	/// </summary>
	public sealed class Mouse : DisposableObject
	{
		/// <summary>
		///     Stores whether a button is currently being double-clicked.
		/// </summary>
		private readonly bool[] _doubleClicked = new bool[Enum.GetValues(typeof(MouseButton)).Length + 1];

		/// <summary>
		///     The mouse button states.
		/// </summary>
		private readonly InputState[] _states = new InputState[Enum.GetValues(typeof(MouseButton)).Length + 1];

		/// <summary>
		///     The window that generates the mouse events.
		/// </summary>
		private readonly Window _window;

		/// <summary>
		///     The direction the mouse wheel was turned in.
		/// </summary>
		private MouseWheelDirection? _wheelDirection;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="window">The window that generates the mouse events.</param>
		internal Mouse(Window window)
		{
			Assert.ArgumentNotNull(window, nameof(window));

			_window = window;
			_window.MousePressed += ButtonPressed;
			_window.MouseReleased += ButtonReleased;
			_window.MouseWheel += WheelTurned;
		}

		/// <summary>
		///     Gets the position of the mouse.
		/// </summary>
		public Vector2 Position
		{
			get
			{
				SDL_GetMouseState(out var x, out var y);
				return new Vector2(x, y);
			}
		}

		/// <summary>
		///     Gets a value indicating whether the mouse is currently within the window.
		/// </summary>
		public unsafe bool InsideWindow => SDL_GetMouseFocus() != null;

		/// <summary>
		///     Gets the input state for the given button.
		/// </summary>
		/// <param name="button">The button the input state should be returned for.</param>
		public InputState this[MouseButton button]
		{
			get
			{
				Assert.ArgumentInRange(button, nameof(button));
				return _states[(int)button];
			}
		}

		/// <summary>
		///     Invoked when a button has been pressed.
		/// </summary>
		private void ButtonPressed(MouseButton button, Vector2 position, bool doubleClicked)
		{
			_states[(int)button].Pressed();
			_doubleClicked[(int)button] |= doubleClicked;
		}

		/// <summary>
		///     Invoked when a button has been released.
		/// </summary>
		private void ButtonReleased(MouseButton button, Vector2 position)
		{
			_states[(int)button].Released();
		}

		/// <summary>
		///     Invoked when the mouse wheel has been turned.
		/// </summary>
		private void WheelTurned(MouseWheelDirection direction)
		{
			_wheelDirection = direction;
		}

		/// <summary>
		///     Updates the mouse state.
		/// </summary>
		internal void Update()
		{
			for (var i = 0; i < _states.Length; ++i)
			{
				_wheelDirection = null;
				_states[i].Update();
				_doubleClicked[i] = false;
			}
		}

		/// <summary>
		///     Gets a value indicating whether the button is currently being pressed down.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool IsPressed(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].IsPressed;
		}

		/// <summary>
		///     Gets a value indicating whether the button was pressed during the current frame. WentDown is
		///     only true during the single frame when IsPressed changed from false to true.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentDown(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].WentDown;
		}

		/// <summary>
		///     Gets a value indicating whether the button was released during the current frame. WentUp is
		///     only true during the single frame when IsPressed changed from true to false.
		/// </summary>
		/// <param name="button">The button that should be checked.</param>
		public bool WentUp(MouseButton button)
		{
			Assert.ArgumentInRange(button, nameof(button));
			return _states[(int)button].WentUp;
		}

		/// <summary>
		///     Gets a value indicating whether the mouse wheel has been turned into the indicated direction.
		/// </summary>
		/// <param name="direction">The direction that should be checked.</param>
		public bool WasTurned(MouseWheelDirection direction)
		{
			Assert.ArgumentInRange(direction, nameof(direction));
			return _wheelDirection == direction;
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_window.MousePressed -= ButtonPressed;
			_window.MouseReleased -= ButtonReleased;
			_window.MouseWheel -= WheelTurned;
		}
	}
}