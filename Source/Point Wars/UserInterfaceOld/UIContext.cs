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

namespace PointWars.UserInterfaceOld
{
	using System.Collections.Generic;
	using System.Numerics;
	using Platform.Input;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a set of UI elements that logically belong together.
	/// </summary>
	public sealed class UIContext : DisposableObject
	{
		private readonly List<UIElement> _elements = new List<UIElement>();
		private UIElement _hoveredElement;
		private LogicalInputDevice _inputDevice;
		private InputLayer _inputLayer;

		/// <summary>
		///   Gets a value indicating whether the UI context is currently active.
		/// </summary>
		private bool _isActive;

		/// <summary>
		///   Adds the UI elements to the context.
		/// </summary>
		/// <param name="elements">The UI elements that should be added.</param>
		public void Add(params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));
			_elements.AddRange(elements);
		}

		/// <summary>
		///   Removes the UI elements from the context.
		/// </summary>
		/// <param name="elements">The UI elements that should be removed.</param>
		public void Remove(params UIElement[] elements)
		{
			Assert.ArgumentNotNull(elements, nameof(elements));

			foreach (var element in elements)
				_elements.Remove(element);
		}

		/// <summary>
		///   Draws the UI elements.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI elements.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			foreach (var element in _elements)
				element.Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_elements.SafeDisposeAll();

			_inputDevice.GainedFocus -= GainedFocus;
			_inputDevice.LostFocus -= LostFocus;
			_inputDevice.InputLayerChanged -= InputLayerChanged;
			_inputDevice.Mouse.Pressed -= MousePressed;
			_inputDevice.Mouse.Released -= MouseReleased;
			_inputDevice.Mouse.Moved -= MouseMoved;
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
			_inputDevice.GainedFocus += GainedFocus;
			_inputDevice.LostFocus += LostFocus;
			_inputDevice.InputLayerChanged += InputLayerChanged;
			_inputDevice.Mouse.Pressed += MousePressed;
			_inputDevice.Mouse.Released += MouseReleased;
			_inputDevice.Mouse.Moved += MouseMoved;
		}

		/// <summary>
		///   Handles focus lost events.
		/// </summary>
		private void LostFocus()
		{
			_hoveredElement?.OnMouseLeft();
			_hoveredElement = null;
		}

		/// <summary>
		///   Handles focus gained events.
		/// </summary>
		private void GainedFocus()
		{
			if (!_inputDevice.Mouse.InsideWindow)
				return;

			_hoveredElement = GetElementAt(_inputDevice.Mouse.Position);
			_hoveredElement?.OnMouseEntered();
		}

		/// <summary>
		///   Handles changes to the input layer.
		/// </summary>
		private void InputLayerChanged(InputLayer inputLayer)
		{
			_isActive = (inputLayer & _inputLayer) == _inputLayer;

			if (!_isActive)
				LostFocus();
			else
				GainedFocus();
		}

		/// <summary>
		///   Gets the UI element at the given position or null if there is none.
		/// </summary>
		private UIElement GetElementAt(Vector2 position)
		{
			foreach (var element in _elements)
			{
				if (element.Area.Intersects(position))
					return element;
			}

			return null;
		}

		/// <summary>
		///   Handles mouse movement.
		/// </summary>
		private void MouseMoved(Vector2 position)
		{
			if (!_isActive)
				return;

			var element = GetElementAt(position);
			if (element == _hoveredElement)
				return;

			_hoveredElement?.OnMouseLeft();
			_hoveredElement = element;
			_hoveredElement?.OnMouseEntered();
		}

		/// <summary>
		///   Handles a mouse click.
		/// </summary>
		private void MousePressed(MouseButton button, Vector2 position, bool doubleClicked)
		{
			if (!_isActive)
				return;

			var element = GetElementAt(position);
			element?.OnMouseDown(button, doubleClicked);
		}

		/// <summary>
		///   Handles a mouse click.
		/// </summary>
		private void MouseReleased(MouseButton button, Vector2 position)
		{
			if (!_isActive)
				return;

			var element = GetElementAt(position);
			element?.OnMouseUp(button);
		}
	}
}