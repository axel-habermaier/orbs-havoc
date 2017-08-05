namespace OrbsHavoc.Views
{
	using Platform;
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