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
	using Assets;
	using Platform.Input;
	using Rendering;
	using UserInterfaceOld;
	using Utilities;

	/// <summary>
	///   Represents the console.
	/// </summary>
	internal sealed class ConsoleView : View
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="console">The console that should be used by the console view.</param>
		public ConsoleView(Console console)
			: base(InputLayer.Console)
		{
			Assert.ArgumentNotNull(console, nameof(console));
			Console = console;
		}

		/// <summary>
		///   Gets the console that is shown.
		/// </summary>
		public Console Console { get; }

		/// <summary>
		///   Changes the size available to the view.
		/// </summary>
		/// <param name="size">The new size available to the view.</param>
		public override void Resize(Size size)
		{
			Console.ChangeSize(size);
		}

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Console.Initialize(Window.Size, InputDevice, Assets.DefaultFont);
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			Console.Update();
			IsActive = Console.IsOpened;
		}

		/// <summary>
		///   Draws the view's contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public override void Draw(SpriteBatch spriteBatch)
		{
			Console.Draw(spriteBatch);
		}
	}
}