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

namespace PointWars.UserInterface
{
	using System;
	using System.Numerics;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a message box.
	/// </summary>
	public sealed class MessageBox : DisposableObject
	{
		private readonly Button[] _buttons = new Button[2];
		private readonly Frame _frame;
		private readonly Label _message;
		private readonly Label _title;
		private UIContext _context;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="title">The title of the message box.</param>
		/// <param name="message">The message of the message box.</param>
		public MessageBox(string title, string message)
		{
			Assert.ArgumentNotNullOrWhitespace(title, nameof(title));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			_title = new Label(title) { Font = Assets.LiberationSans16, Margin = new Thickness(3) };
			_message = new Label(message) { Font = Assets.LiberationSans16, Area = new Rectangle(0, 0, 600, 0), Margin = new Thickness(3) };
			_frame = new Frame { Padding = new Thickness(5) };
			_buttons[0] = new Button { IsVisible = false };
			_buttons[1] = new Button { IsVisible = false };
		}

		/// <summary>
		///   Adds a button with the given label to the message box, invoking the given continuation when the button is pressed.
		/// </summary>
		public MessageBox WithButton(string label, Action continuation = null)
		{
			Assert.That(!_buttons[1].IsVisible, "Cannot add more than two buttons.");

			var index = !_buttons[0].IsVisible ? 0 : 1;
			_buttons[index].Font = Assets.LiberationSans16;
			_buttons[index].Clicked = continuation;
			_buttons[index].IsVisible = true;

			return this;
		}

		/// <summary>
		///   Registers the message box on the given UI context.
		/// </summary>
		/// <param name="context">The UI context the message box should be registered on.</param>
		public void Register(UIContext context)
		{
			context.Add(_buttons);
			_context = context;
		}

		/// <summary>
		///   Resizes the message box.
		/// </summary>
		/// <param name="size">The new size of the area the message box is shown in.</param>
		public void Resize(Size size)
		{
			Layout.ResetLayoutedPositions(_title, _message, _buttons[0], _buttons[1]);

			var area = new Rectangle(Vector2.Zero, size);
			_message.CenterHorizontally(area);
			_title.CenterHorizontally(area);

			Layout.CenterHorizontally(area, _buttons);
			_frame.Area = Layout.GetArea(_title, _message, _buttons[0], _buttons[1]);

			Layout.StackVertically(_title, _message, _buttons[0], _buttons[1]);
			Layout.CenterVertically(area, _title, _message, _buttons[0], _buttons[1]);

			_frame.CenterHorizontally(area);
			_frame.CenterVertically(area);
		}

		/// <summary>
		///   Draws the UI element.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the UI element.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			_frame.Draw(spriteBatch);

			++spriteBatch.Layer;

			_message.Draw(spriteBatch);
			_title.Draw(spriteBatch);
			_buttons[0].Draw(spriteBatch);
			_buttons[1].Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_context.Remove(_buttons);

			_title.SafeDispose();
			_message.SafeDispose();
			_frame.SafeDispose();
			_buttons.SafeDisposeAll();
		}
	}
}