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
	using UserInterface;
	using Utilities;

	/// <summary>
	///   Represents a view of the application.
	/// </summary>
	internal abstract class View : DisposableObject
	{
		private bool _isShown;
		private UIElement _rootElement;

		/// <summary>
		///   Gets or sets the view collection the view belongs to.
		/// </summary>
		public ViewCollection Views { get; set; }

		/// <summary>
		///   Gets or sets the root UI element of the view.
		/// </summary>
		public UIElement RootElement
		{
			get { return _rootElement; }
			protected set
			{
				_rootElement = value;

				if (_rootElement != null)
				{
					Views.RootElement.Add(_rootElement);
					_rootElement.Visibility = IsShown ? Visibility.Visible : Visibility.Collapsed;
				}
				else
					Views.RootElement.Remove(_rootElement);
			}
		}

		/// <summary>
		///   Gets the window the view is drawn in.
		/// </summary>
		protected Window Window => Views.Application.Window;

		/// <summary>
		///   Gets the input device that should be used by the view.
		/// </summary>
		protected LogicalInputDevice InputDevice => Views.Application.InputDevice;

		/// <summary>
		///   Gets or sets a value indicating whether the view is currently shown.
		/// </summary>
		public bool IsShown
		{
			get { return _isShown; }
			set
			{
				if (value)
					Show();
				else
					Hide();
			}
		}

		/// <summary>
		///   Shows the view by making it active.
		/// </summary>
		public void Show()
		{
			if (IsShown)
				return;

			_isShown = true;
			if (RootElement != null)
				RootElement.Visibility = Visibility.Visible;

			Activate();
		}

		/// <summary>
		///   Hides the view by making it inactive.
		/// </summary>
		public void Hide()
		{
			if (!IsShown)
				return;

			_isShown = false;

			if (RootElement != null)
				RootElement.Visibility = Visibility.Collapsed;

			Deactivate();
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
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
		}
	}
}