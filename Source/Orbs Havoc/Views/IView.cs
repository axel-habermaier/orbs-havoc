namespace OrbsHavoc.Views
{
	using System;
	using UserInterface;

	internal interface IView : IDisposable
	{
		bool IsShown { get; set; }

		UIElement UI { get; }
		void Hide();
		void Show();

		void HandleActivationChange();
		void Initialize(ViewCollection views);
		void Update();
	}
}