// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using Scripting;
	using static Platform.SDL2;

	/// <summary>
	///   Represents a clock that can be used to query the time that has elapsed since the creation of the clock.
	/// </summary>
	public struct Clock
	{
		/// <summary>
		///   Scales the passing of time. If null, time advances in constant steps.
		/// </summary>
		private readonly Cvar<double> _scale;

		/// <summary>
		///   A value indicating whether the clock has been fully initialized. If false, indicates that the clock was created via the
		///   default constructor and has not yet been initialized.
		/// </summary>
		private bool _isInitialized;

		/// <summary>
		///   The offset that is applied to all times returned by this instance.
		/// </summary>
		private double _offset;

		/// <summary>
		///   The current time in seconds.
		/// </summary>
		private double _time;

		/// <summary>
		///   The application start time.
		/// </summary>
		private static readonly double _startTime = GetTime();

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="scale">Scales the passing of time. If null, time advances in constant steps.</param>
		public Clock(Cvar<double> scale)
			: this()
		{
			_scale = scale;
			Reset();
		}

		/// <summary>
		///   Gets the current time in seconds.
		/// </summary>
		public double Seconds
		{
			get
			{
				if (!_isInitialized)
					Reset();

				Update();
				return _time;
			}
		}

		/// <summary>
		///   Gets the current time in milliseconds.
		/// </summary>
		public double Milliseconds => Seconds * 1000;

		/// <summary>
		///   Gets the time since the start of the application in seconds.
		/// </summary>
		public static double GetTime()
		{
			return SDL_GetPerformanceCounter() / (double)SDL_GetPerformanceFrequency() - _startTime;
		}

		/// <summary>
		///   Resets the clock to zero.
		/// </summary>
		public void Reset()
		{
			_isInitialized = true;
			_offset = GetTime();
			_time = 0;
		}

		/// <summary>
		///   Updates the internal time.
		/// </summary>
		private void Update()
		{
			// Get the elapsed system time since the last update
			var systemTime = GetTime();
			var elapsedTime = systemTime - _offset;
			_offset = systemTime;

			// Scale the elapsedTime with the current scaling factor and add it to the internal time
			var scale = _scale?.Value ?? 1;
			_time += elapsedTime * scale;
		}
	}
}