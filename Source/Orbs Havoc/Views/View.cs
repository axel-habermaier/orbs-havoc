﻿// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Views
{
	using Platform;
	using Platform.Input;
	using Platform.Memory;
	using UserInterface;

	/// <summary>
	///     Represents a view of the application.
	/// </summary>
	internal abstract class View : DisposableObject
	{
		private bool _activationChanged;
		private bool _isShown;
		private UIElement _rootElement;

		/// <summary>
		///     Gets or sets the view collection the view belongs to.
		/// </summary>
		public ViewCollection Views { get; set; }

		/// <summary>
		///     Gets or sets the root UI element of the view.
		/// </summary>
		public UIElement RootElement
		{
			get => _rootElement;
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
		///     Gets the window the view is drawn in.
		/// </summary>
		protected Window Window => Views.Window;

		/// <summary>
		///     Gets the input device that should be used by the view.
		/// </summary>
		protected LogicalInputDevice InputDevice => Views.InputDevice;

		/// <summary>
		///     Gets or sets a value indicating whether the view is currently shown.
		/// </summary>
		public bool IsShown
		{
			get => _isShown;
			set
			{
				if (value)
					Show();
				else
					Hide();
			}
		}

		/// <summary>
		///     Shows the view by making it active.
		/// </summary>
		public void Show()
		{
			if (IsShown)
				return;

			_isShown = true;
			if (RootElement != null)
				RootElement.Visibility = Visibility.Visible;

			_activationChanged = !_activationChanged;
		}

		/// <summary>
		///     Hides the view by making it inactive.
		/// </summary>
		public void Hide()
		{
			if (!IsShown)
				return;

			_isShown = false;

			if (RootElement != null)
				RootElement.Visibility = Visibility.Collapsed;

			_activationChanged = !_activationChanged;
		}

		public virtual void Initialize()
		{
		}

		public virtual void InitializeUI()
		{
		}

		protected virtual void Activate()
		{
		}

		protected virtual void Deactivate()
		{
		}

		public void HandleActivationChange()
		{
			if (!_activationChanged)
				return;

			_activationChanged = false;

			if (IsShown)
				Activate();
			else
				Deactivate();
		}

		public virtual void Update()
		{
		}

		protected override void OnDisposing()
		{
		}
	}
}