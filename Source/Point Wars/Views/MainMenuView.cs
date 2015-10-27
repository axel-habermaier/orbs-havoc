﻿// The MIT License (MIT)
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
	using System.Numerics;
	using Platform.Input;
	using Rendering;
	using Scripting;
	using UserInterface;
	using Utilities;

	/// <summary>
	///   Represents the application's main menu when no game session is active.
	/// </summary>
	internal sealed class MainMenuView : View
	{
		private UIElement[] _elements;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public MainMenuView()
			: base(InputLayer.Menu)
		{
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			IsActive = true;

			var buttonArea = new Rectangle(Vector2.Zero, 200, 0);
			_elements = new UIElement[]
			{
				new Label(Application.Name) { Font = Assets.Moonhouse80, Margin = new Thickness(0, 0, 0, 30) },
				new Button("Start Game") { Font = Assets.Moonhouse24, Area = buttonArea },
				new Button("Join Game") { Font = Assets.Moonhouse24, Area = buttonArea },
				new Button("Options") { Font = Assets.Moonhouse24, Area = buttonArea },
				new Button("Exit") { Font = Assets.Moonhouse24, Area = buttonArea, Clicked = OnExit }
			};

			UIContext.Add(_elements);
		}

		/// <summary>
		///   Handles clicks on the Exit button.
		/// </summary>
		private void OnExit()
		{
			Views.MessageBoxes.Show(new MessageBox("Are you sure?", "Do you really want to quit the application?").WithButton("Yes", Commands.Exit));
		}

		/// <summary>
		///   Changes the size available to the view.
		/// </summary>
		/// <param name="size">The new size available to the view.</param>
		public override void Resize(Size size)
		{
			var area = new Rectangle(Vector2.Zero, size);

			Layout.ResetLayoutedPositions(_elements);
			Layout.StackVertically(margin: 6, elements: _elements);
			Layout.CenterVertically(area, _elements);
			Layout.CenterEachHorizontally(area, _elements);
		}
	}
}