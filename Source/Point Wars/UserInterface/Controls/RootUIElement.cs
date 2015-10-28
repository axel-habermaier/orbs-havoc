// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace PointWars.UserInterface.Controls
{
	using System;
	using System.Numerics;
	using Assets;
	using Input;
	using Platform.Input;
	using Platform.Logging;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents the root element of all visual trees within an application.
	/// </summary>
	internal sealed class RootUIElement : Control, IDisposable
	{
		private UIElement _hoveredElement;
		private LogicalInputDevice _inputDevice;
		private InputLayer _inputLayer;
		private bool _isActive;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public RootUIElement()
		{
			IsAttachedToRoot = true;
			IsVisible = true;
			Font = Assets.DefaultFont;
			Foreground = Colors.White;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_inputDevice.InputLayerChanged -= InputLayerChanged;
			_inputDevice.Mouse.Pressed -= MousePressed;
			_inputDevice.Mouse.Released -= MouseReleased;
			_inputDevice.Keyboard.KeyPressed -= KeyPressed;
			_inputDevice.Keyboard.KeyReleased -= KeyReleased;
			_inputDevice.Keyboard.TextEntered -= TextEntered;
		}

		/// <summary>
		///   Layouts the UI's contents for the given size.
		/// </summary>
		/// <param name="availableSize">The size available to the UI.</param>
		public void Update(Size availableSize)
		{
			// We have to check every frame for a new hovered element, as the UI might change at any time
			// (due to animations, UI elements becoming visible/invisible, etc.).
			UpdateHoveredElement(_inputDevice.Mouse.Position);

			Measure(availableSize);
			Arrange(new Rectangle(0, 0, availableSize));
			UpdateVisualOffsets(Vector2.Zero);
		}

		/// <summary>
		///   Initializes the UI context.
		/// </summary>
		/// <param name="inputDevice">The input device that should be used for input handling.</param>
		/// <param name="inputLayer">The input layer that must be active for the UI to receive any input.</param>
		public void Initialize(LogicalInputDevice inputDevice, InputLayer inputLayer)
		{
			Assert.ArgumentNotNull(inputDevice, nameof(inputDevice));
			Assert.ArgumentInRange(inputLayer, nameof(inputLayer));

			_inputLayer = inputLayer;
			_inputDevice = inputDevice;

			_inputDevice.InputLayerChanged += InputLayerChanged;
			_inputDevice.Mouse.Pressed += MousePressed;
			_inputDevice.Mouse.Released += MouseReleased;
			_inputDevice.Keyboard.KeyPressed += KeyPressed;
			_inputDevice.Keyboard.KeyReleased += KeyReleased;
			_inputDevice.Keyboard.TextEntered += TextEntered;
		}

		/// <summary>
		///   Handles changes to the input layer.
		/// </summary>
		private void InputLayerChanged(InputLayer inputLayer)
		{
			_isActive = (inputLayer & _inputLayer) == _inputLayer;
		}

		/// <summary>
		///   Handles a text entered event.
		/// </summary>
		private void TextEntered(string text)
		{
			if (!_isActive || _hoveredElement == null)
				return;

			var args = TextInputEventArgs.Create(text);
			OnTextEntered(_hoveredElement, args);
		}

		/// <summary>
		///   Handles a mouse pressed event.
		/// </summary>
		private void MousePressed(MouseButton button, Vector2 position, bool doubleClicked)
		{
			if (!_isActive || _hoveredElement == null)
				return;

			var args = MouseButtonEventArgs.Create(_inputDevice.Mouse, button, doubleClicked, _inputDevice.Keyboard.GetModifiers());
			OnMousePressed(_hoveredElement, args);
		}

		/// <summary>
		///   Handles a mouse released event.
		/// </summary>
		private void MouseReleased(MouseButton button, Vector2 position)
		{
			if (!_isActive || _hoveredElement == null)
				return;

			var args = MouseButtonEventArgs.Create(_inputDevice.Mouse, button, false, _inputDevice.Keyboard.GetModifiers());
			OnMouseReleased(_hoveredElement, args);
		}

		/// <summary>
		///   Handles a key pressed event.
		/// </summary>
		private void KeyPressed(Key key, ScanCode scanCode, KeyModifiers modifiers)
		{
			if (!_isActive || _hoveredElement == null)
				return;

			var args = KeyEventArgs.Create(_inputDevice.Keyboard, key, scanCode);
			OnKeyPressed(_hoveredElement, args);
		}

		/// <summary>
		///   Handles a key released event.
		/// </summary>
		private void KeyReleased(Key key, ScanCode scanCode, KeyModifiers modifiers)
		{
			if (!_isActive || _hoveredElement == null)
				return;

			var args = KeyEventArgs.Create(_inputDevice.Keyboard, key, scanCode);
			OnKeyReleased(_hoveredElement, args);
		}

		/// <summary>
		///   Updates the hovered UI element, if necessary.
		/// </summary>
		/// <param name="position">The position of the mouse.</param>
		private void UpdateHoveredElement(Vector2 position)
		{
			UIElement hoveredElement = null;
			if (_isActive && _inputDevice.Mouse.InsideWindow)
				hoveredElement = HitTest(new Vector2(position.X, position.Y), boundsTestOnly: false);

			if (hoveredElement == _hoveredElement)
				return;

			var args = MouseEventArgs.Create(_inputDevice.Mouse, _inputDevice.Keyboard.GetModifiers());
			OnMouseLeft(_hoveredElement, args);

			_hoveredElement = hoveredElement;
			Log.DebugIf(false, "Hovered element: {0}", _hoveredElement?.GetType().Name ?? "None");

			if (_hoveredElement == null)
				return;

			OnMouseEntered(_hoveredElement, args);
		}
	}
}