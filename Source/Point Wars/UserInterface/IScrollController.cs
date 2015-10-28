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
	/// <summary>
	///   Provides methods that can be used to control a scroll handler.
	/// </summary>
	public interface IScrollController
	{
		/// <summary>
		///   Scrolls up by a step.
		/// </summary>
		void ScrollUp();

		/// <summary>
		///   Scrolls down by a step.
		/// </summary>
		void ScrollDown();

		/// <summary>
		///   Scrolls left by a step.
		/// </summary>
		void ScrollLeft();

		/// <summary>
		///   Scrolls right by a step.
		/// </summary>
		void ScrollRight();

		/// <summary>
		///   Scrolls to the top of the content area.
		/// </summary>
		void ScrollToTop();

		/// <summary>
		///   Scrolls to the bottom of the content area.
		/// </summary>
		void ScrollToBottom();

		/// <summary>
		///   Scrolls to the left of the content area.
		/// </summary>
		void ScrollToLeft();

		/// <summary>
		///   Scrolls to the right of the content area.
		/// </summary>
		void ScrollToRight();
	}
}