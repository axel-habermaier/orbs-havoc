namespace OrbsHavoc.UserInterface.Controls
{
	using System;
	using System.Collections.Generic;
	using System.Numerics;
	using Assets;
	using Input;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Utilities;

	/// <summary>
	///     Represents the root element of all visual trees within an application.
	/// </summary>
	internal sealed class RootUIElement : AreaPanel, IDisposable
	{
		private readonly List<UIElement> _focusedElements = new List<UIElement>();
		private readonly LogicalInputDevice _inputDevice;
		private UIElement _focusedElement;
		private UIElement _hoveredElement;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="inputDevice">The input device that should be used for input handling.</param>
		public RootUIElement(LogicalInputDevice inputDevice)
		{
			Assert.ArgumentNotNull(inputDevice, nameof(inputDevice));

			IsVisible = true;
			IsFocusable = false;
			IsAttachedToRoot = true;
			Font = AssetBundle.DefaultFont;
			Foreground = Colors.White;
			FocusedElement = null;

			_inputDevice = inputDevice;
			_inputDevice.Mouse.Pressed += MousePressed;
			_inputDevice.Mouse.Released += MouseReleased;
			_inputDevice.Mouse.Wheel += MouseWheel;
			_inputDevice.Keyboard.KeyPressed += KeyPressed;
			_inputDevice.Keyboard.KeyReleased += KeyReleased;
			_inputDevice.Keyboard.TextEntered += TextEntered;
		}

		/// <summary>
		///     Gets the UI element that currently has the keyboard focus. Unless the focus has been shifted to another UI
		///     element, it is the window itself.
		/// </summary>
		public UIElement FocusedElement
		{
			get
			{
				Assert.NotNull(_focusedElement);
				return _focusedElement;
			}
			internal set
			{
				if (value == null)
					value = this;

				if (_focusedElement == value)
					return;

				if (_focusedElement != null)
					_focusedElement.IsFocused = false;

				_focusedElement = value;

				if (_focusedElement != null)
				{
					_focusedElement.IsFocused = true;
					_focusedElements.Add(value);

					CleanFocusedElements();
				}

				Log.DebugIf(false, $"Focused element: {_focusedElement?.GetType().Name}");
				Log.DebugIf(_focusedElements.Count > 32, "Unusually large focused elements history stack.");
			}
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_inputDevice.Mouse.Pressed -= MousePressed;
			_inputDevice.Mouse.Released -= MouseReleased;
			_inputDevice.Mouse.Wheel -= MouseWheel;
			_inputDevice.Keyboard.KeyPressed -= KeyPressed;
			_inputDevice.Keyboard.KeyReleased -= KeyReleased;
			_inputDevice.Keyboard.TextEntered -= TextEntered;
		}

		/// <summary>
		///     Layouts the UI's contents for the given size.
		/// </summary>
		/// <param name="availableSize">The size available to the UI.</param>
		public void Update(Size availableSize)
		{
			// We have to check every frame for a new hovered element, as the UI might change at any time
			// (due to animations, UI elements becoming visible/invisible, etc.).
			UpdateHoveredElement(_inputDevice.Mouse.Position);

			// We have to check every frame whether the focused element must be reset; it could have been removed
			// or hidden since the last frame, among other things.
			if (FocusedElement != null && !FocusedElement.CanBeFocused)
				ResetFocusedElement();

			// Update the layout of the tree
			Measure(availableSize);
			Arrange(new Rectangle(0, 0, availableSize));
			UpdateVisualOffsets(Vector2.Zero);
		}

		/// <summary>
		///     Handles a mouse pressed event.
		/// </summary>
		private void MousePressed(MouseButton button, Vector2 position, bool doubleClicked)
		{
			if (_hoveredElement == null)
				return;

			var args = MouseButtonEventArgs.Create(_inputDevice.Mouse, _inputDevice.Keyboard, button, doubleClicked, InputEventKind.Down);
			OnMouseDown(_hoveredElement, args);
		}

		/// <summary>
		///     Handles a mouse released event.
		/// </summary>
		private void MouseReleased(MouseButton button, Vector2 position)
		{
			if (_hoveredElement == null)
				return;

			var args = MouseButtonEventArgs.Create(_inputDevice.Mouse, _inputDevice.Keyboard, button, false, InputEventKind.Up);
			OnMouseUp(_hoveredElement, args);
		}

		/// <summary>
		///     Handles a mouse wheel event.
		/// </summary>
		private void MouseWheel(MouseWheelDirection direction)
		{
			if (_hoveredElement == null)
				return;

			var args = MouseWheelEventArgs.Create(_inputDevice.Mouse, _inputDevice.Keyboard, direction);
			OnMouseWheel(_hoveredElement, args);
		}

		/// <summary>
		///     Handles a key pressed event.
		/// </summary>
		private void KeyPressed(Key key, ScanCode scanCode, KeyModifiers modifiers)
		{
			if (FocusedElement == null)
				return;

			var args = KeyEventArgs.Create(_inputDevice.Keyboard, _inputDevice.Mouse, key, scanCode, InputEventKind.Down);
			OnKeyDown(FocusedElement, args);
		}

		/// <summary>
		///     Handles a key released event.
		/// </summary>
		private void KeyReleased(Key key, ScanCode scanCode, KeyModifiers modifiers)
		{
			if (FocusedElement == null)
				return;

			var args = KeyEventArgs.Create(_inputDevice.Keyboard, _inputDevice.Mouse, key, scanCode, InputEventKind.Up);
			OnKeyUp(FocusedElement, args);
		}

		/// <summary>
		///     Handles a text entered event.
		/// </summary>
		private void TextEntered(string text)
		{
			if (FocusedElement == null)
				return;

			var args = TextInputEventArgs.Create(text);
			OnTextEntered(FocusedElement, args);
		}

		/// <summary>
		///     Updates the hovered UI element, if necessary.
		/// </summary>
		/// <param name="position">The position of the mouse.</param>
		private void UpdateHoveredElement(Vector2 position)
		{
			UIElement hoveredElement = this;
			if (_inputDevice.Mouse.InsideWindow)
				hoveredElement = HitTest(new Vector2(position.X, position.Y), boundsTestOnly: false) ?? this;

			if (hoveredElement == _hoveredElement)
				return;

			var args = MouseEventArgs.Create(_inputDevice.Mouse, _inputDevice.Keyboard);

			if (_hoveredElement != null)
				OnMouseLeave(_hoveredElement, args);

			_hoveredElement = hoveredElement;
			Log.DebugIf(false, $"Hovered element: {_hoveredElement?.GetType().Name}");

			if (_hoveredElement != null)
				OnMouseEnter(_hoveredElement, args);
		}

		/// <summary>
		///     Resets the focused element to the first focusable previously focused element. If none exists, the
		///     closest focusable element in the tree is focused. Otherwise, the focus is set to the window.
		/// </summary>
		private void ResetFocusedElement()
		{
			// Focus the first previously focused element, if one exists
			while (_focusedElements.Count > 0)
			{
				var last = _focusedElements.Count - 1;
				var focusedElement = _focusedElements[last];
				_focusedElements.RemoveAt(last);

				if (!focusedElement.CanBeFocused)
					continue;

				FocusedElement = focusedElement;
				return;
			}

			// Otherwise, just focus to root element
			FocusedElement = this;
		}

		/// <summary>
		///     Removes all previously focused elements that are no longer part of the visual tree.
		/// </summary>
		private void CleanFocusedElements()
		{
			for (var i = _focusedElements.Count; i > 0; --i)
			{
				if (!_focusedElements[i - 1].IsAttachedToRoot)
					_focusedElements.RemoveAt(i - 1);
			}
		}
	}
}