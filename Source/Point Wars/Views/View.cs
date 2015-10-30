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

namespace PointWars.Views
{
	using Platform;
	using Platform.Input;
	using Platform.Memory;
	using Rendering;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Represents a view of the application.
	/// </summary>
	internal abstract class View : DisposableObject
	{
		private bool _isActive;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="inputLayer">The input layer that should be activated and deactivated with the view.</param>
		protected View(InputLayer inputLayer = InputLayer.None)
		{
			Assert.ArgumentInRange(inputLayer, nameof(inputLayer));
			InputLayer = inputLayer;
		}

		/// <summary>
		///   Gets the input layer used by the view.
		/// </summary>
		public InputLayer InputLayer { get; }

		/// <summary>
		///   Gets or sets the view collection the view belongs to.
		/// </summary>
		public ViewCollection Views { get; set; }

		/// <summary>
		///   Gets the context for the view's UI elements.
		/// </summary>
		public RootUIElement RootElement { get; } = new RootUIElement();

		/// <summary>
		///   Gets the window the view is drawn in.
		/// </summary>
		public Window Window => Views.Application.Window;

		/// <summary>
		///   Gets the input device that should be used by the view.
		/// </summary>
		public LogicalInputDevice InputDevice => Views.Application.InputDevice;

		/// <summary>
		///   Gets or sets a value indicating whether the view is currently active.
		/// </summary>
		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				if (_isActive == value)
					return;

				_isActive = value;
				RootElement.IsActive = value;

				if (_isActive)
				{
					Activate();
					InputDevice.ActivateLayer(InputLayer);
				}
				else
				{
					Deactivate();
					InputDevice.DeactivateLayer(InputLayer);
				}
			}
		}

		/// <summary>
		///   Changes the size available to the view.
		/// </summary>
		/// <param name="size">The new size available to the view.</param>
		public virtual void Resize(Size size)
		{
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		///   Invoked when the view should be activated.
		/// </summary>
		protected virtual void Activate()
		{
		}

		/// <summary>
		///   Invoked when the view should be deactivated.
		/// </summary>
		protected virtual void Deactivate()
		{
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public virtual void Update()
		{
			if (IsActive)
				RootElement.Update(Window.Size);
		}

		/// <summary>
		///   Draws the view's contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public virtual void Draw(SpriteBatch spriteBatch)
		{
			RootElement.Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			RootElement.SafeDispose();
		}
	}
}