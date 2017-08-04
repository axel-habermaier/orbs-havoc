// The MIT License (MIT)
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

	internal abstract class View<TRootElement> : DisposableObject, IView
		where TRootElement : UIElement, new()
	{
		private bool _activationChanged;
		private bool _isShown;

		public ViewCollection Views { get; set; }
		public TRootElement UI { get; private set; }
		protected Window Window => Views.Window;
		protected LogicalInputDevice InputDevice => Views.InputDevice;

		UIElement IView.UI => UI;

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

		public void Show()
		{
			if (IsShown)
				return;

			_isShown = true;
			if (UI != null)
				UI.Visibility = Visibility.Visible;

			_activationChanged = !_activationChanged;
		}

		public void Hide()
		{
			if (!IsShown)
				return;

			_isShown = false;

			if (UI != null)
				UI.Visibility = Visibility.Collapsed;

			_activationChanged = !_activationChanged;
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

		public void Initialize(ViewCollection views)
		{
			Views = views;
			UI = new TRootElement { Visibility = Visibility.Collapsed };

			Views.RootElement.Add(UI);

			Initialize();
			InitializeUI();
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

		protected override void OnDisposing()
		{
		}
	}
}