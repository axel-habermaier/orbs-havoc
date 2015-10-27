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
	using System.Collections.Generic;
	using Platform.Memory;
	using Rendering;
	using UserInterface;
	using Utilities;

	/// <summary>
	///   Shows open message boxes.
	/// </summary>
	internal sealed class MessageBoxesView : View
	{
		private readonly List<MessageBox> _messageBoxes = new List<MessageBox>();

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			IsActive = true;
		}

		/// <summary>
		///   Shows the given message box.
		/// </summary>
		/// <param name="messageBox">The message box that should be shown.</param>
		public void Show(MessageBox messageBox)
		{
			Assert.ArgumentNotNull(messageBox, nameof(messageBox));

			messageBox.Register(UIContext);
			messageBox.Resize(Window.Size);
			_messageBoxes.Add(messageBox);
		}

		/// <summary>
		///   Changes the size available to the view.
		/// </summary>
		/// <param name="size">The new size available to the view.</param>
		public override void Resize(Size size)
		{
			foreach (var messageBox in _messageBoxes)
				messageBox.Resize(size);
		}

		/// <summary>
		///   Draws the view's contents.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the view.</param>
		public override void Draw(SpriteBatch spriteBatch)
		{
			foreach (var messageBox in _messageBoxes)
				messageBox.Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_messageBoxes.SafeDisposeAll();
			base.OnDisposing();
		}
	}
}