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
	///   Indicates where an UI element should be displayed vertically relative to its parent element.
	/// </summary>
	public enum VerticalAlignment : byte
	{
		/// <summary>
		///   Indicates that the entire vertical space of the parent element is consumed.
		/// </summary>
		Stretch,

		/// <summary>
		///   Indicates that the element is aligned to the top of the parent element's space.
		/// </summary>
		Top,

		/// <summary>
		///   Indicates that the element is centered vertically in the parent element's space.
		/// </summary>
		Center,

		/// <summary>
		///   Indicates that the element is aligned to the bottom of the parent element's space.
		/// </summary>
		Bottom
	}
}