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
	using System;
	using System.Numerics;
	using Platform.Memory;
	using Rendering;
	using Utilities;
	using Console = UserInterfaceOld.Console;

	/// <summary>
	///   Represents a collection of application views.
	/// </summary>
	internal class ViewCollection : DisposableObject
	{
		private const int LayersPerView = 1000000;
		private readonly View[] _views;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="app">The application the view collection belongs to.</param>
		/// <param name="console">The console that should be used by the console view.</param>
		public ViewCollection(Application app, Console console)
		{
			Assert.ArgumentNotNull(console, nameof(console));

			Console = new ConsoleView(console);
			Application = app;
			_views = new View[]
			{
				Console,
				DebugOverlay,
				MessageBoxes,
				MainMenu,
				GameSession
			};
		}

		/// <summary>
		///   Gets the application the view collection belongs to.
		/// </summary>
		public Application Application { get; }

		/// <summary>
		///   Gets the main menu view.
		/// </summary>
		public MainMenuView MainMenu { get; } = new MainMenuView();

		/// <summary>
		///   Gets the view containing the messages boxes.
		/// </summary>
		public MessageBoxesView MessageBoxes { get; } = new MessageBoxesView();

		/// <summary>
		///   Gets the console view.
		/// </summary>
		public ConsoleView Console { get; }

		/// <summary>
		///   Gets the debug overlay view.
		/// </summary>
		public DebugOverlayView DebugOverlay { get; } = new DebugOverlayView();

		/// <summary>
		///   Gets the game session view.
		/// </summary>
		public GameSessionView GameSession { get; } = new GameSessionView();

		/// <summary>
		///   Changes the size available to the views.
		/// </summary>
		/// <param name="size">The new size available to the views.</param>
		public void Resize(Size size)
		{
			foreach (var view in _views)
				view.Resize(size);
		}

		/// <summary>
		///   Initializes the views.
		/// </summary>
		public void Initialize()
		{
			foreach (var view in _views)
			{
				view.Views = this;
				view.RootElement.Initialize(view.InputDevice, view.InputLayer);
				view.Initialize();
			}
		}

		/// <summary>
		///   Updates the views' states.
		/// </summary>
		public void Update()
		{
			// We always update all views, regardless of whether they're currently active; this way,
			// the view can decide to update its own active state, for instance
			foreach (var view in _views)
				view.Update();
		}

		/// <summary>
		///   Draws the views' contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the views.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			var i = Byte.MaxValue;
			foreach (var view in _views)
			{
				if (!view.IsActive)
					continue;

				spriteBatch.Layer = i * LayersPerView;
				spriteBatch.PositionOffset = Vector2.Zero;
				spriteBatch.ScissorArea = null;

				view.Draw(spriteBatch);
				Assert.That(spriteBatch.Layer < i * LayersPerView + LayersPerView, "The view used too many layers.");

				--i;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_views.SafeDisposeAll();
		}
	}
}