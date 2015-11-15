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

namespace OrbsHavoc.Utilities
{
	using System;
	using Scripting;

	/// <summary>
	///   Represents a timer that periodically raises a timeout event.
	/// </summary>
	public struct Timer
	{
		/// <summary>
		///   The timeout in milliseconds after which the timeout event is raised.
		/// </summary>
		private readonly double _timeout;

		/// <summary>
		///   The clock that is used to determine when the timeout event should be raised.
		/// </summary>
		private Clock _clock;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="timeout">The timeout in milliseconds after which the timeout event should be raised.</param>
		/// <param name="scale">Scales the passing of time. If null, time advances in constant steps.</param>
		public Timer(double timeout, Cvar<double> scale = null)
			: this()
		{
			_clock = new Clock(scale);
			_timeout = timeout;
		}

		/// <summary>
		///   Raised when a timeout occurred.
		/// </summary>
		public event Action Timeout;

		/// <summary>
		///   Updates the timer, raising the timeout event if enough time has passed.
		/// </summary>
		public void Update()
		{
			Assert.NotNull(Timeout, "Timeout event is not observed.");

			if (_clock.Milliseconds < _timeout)
				return;

			_clock.Reset();
			Timeout();
		}
	}
}