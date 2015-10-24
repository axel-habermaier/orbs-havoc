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

namespace PointWars.Utilities
{
	using System;

	/// <summary>
	///   Provides time measurements and raises the update event whenever necessary. In variable
	///   timestep mode, the update event is raised as often as possible, i.e., once with every call to
	///   Tick(). In fixed timestep mode, the timer attempts to raise the update event approximately at
	///   intervals of the given target elapsed time. Therefore, if the application is calling Tick() faster
	///   than the target elapsed time, not every call to Tick() raises the update event. On the other hand,
	///   if calls to Tick() are infrequent, the update event might be raised several times to catch up.
	///   However, MaxDelta limits the maximum elapsed time between two calls to Tick(), as unreasonably long
	///   pauses are usually due to the app being inactive for some time or due to the debugger pausing the app.
	/// </summary>
	/// <remarks>Adopted from a Visual Studio C++ WinRT template.</remarks>
	public class StepTimer
	{
		/// <summary>
		///   The maximum time delta in seconds that is allowed between two consecutive calls to Update.
		/// </summary>
		private const double MaxDelta = 0.1;

		/// <summary>
		///   The time of the last update.
		/// </summary>
		private double _lastTime;

		/// <summary>
		///   The time left over from the last frame.
		/// </summary>
		private double _leftOverTime;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public StepTimer()
		{
			TargetElapsedSeconds = 1.0 / 60.0;
			ResetElapsedTime();
		}

		/// <summary>
		///   Gets the elapsed time in seconds since the previous update.
		/// </summary>
		public double ElapsedSeconds { get; private set; }

		/// <summary>
		///   Gets the elapsed time in seconds since the creation of the timer.
		/// </summary>
		public double TotalSeconds { get; private set; }

		/// <summary>
		///   Gets or sets a value indicating whether the fixed or the variable timestep mode should be used.
		/// </summary>
		public bool UseFixedTimeStep { get; set; }

		/// <summary>
		///   Gets or sets the target elapsed time in seconds between two consecutive updates in fixed timestep mode.
		/// </summary>
		public double TargetElapsedSeconds { get; set; }

		/// <summary>
		///   Resets all left-over time in fixed timestep mode, preventing the fixed timestep logic from attempting a set of catch-up
		///   updates.
		/// </summary>
		public void ResetElapsedTime()
		{
			_lastTime = Clock.GetTime();
			_leftOverTime = 0;
		}

		/// <summary>
		///   Raised when the step timer determined that an update is required.
		/// </summary>
		public event Action UpdateRequired;

		/// <summary>
		///   Updates the timer state, raising the update event the appropriate number of times.
		/// </summary>
		public void Update()
		{
			Assert.That(UpdateRequired != null, "UpdateRequired event has no listeners.");
			Assert.That(TargetElapsedSeconds > 0 || !UseFixedTimeStep, "Invalid target elapsed time.");

			var currentTime = Clock.GetTime();
			var timeDelta = currentTime - _lastTime;

			_lastTime = currentTime;

			// Clamp large time deltas, as the application might have been paused in the debugger, etc.
			if (timeDelta > MaxDelta)
				timeDelta = MaxDelta;

			if (UseFixedTimeStep)
			{
				// If the app is running very close to the target elapsed time (within 1/4 of a millisecond) just clamp
				// the clock to exactly match the target value. This prevents tiny and irrelevant errors
				// from accumulating over time. Without this clamping, a game that requested a 60 fps
				// fixed update, running with vsync enabled on a 59.94 NTSC display, would eventually
				// accumulate enough tiny errors that it would drop a frame. It is better to just round 
				// small deviations down to zero to leave things running smoothly.
				if (Math.Abs(timeDelta - TargetElapsedSeconds) < 1.0 / 4000.0)
					timeDelta = TargetElapsedSeconds;

				_leftOverTime += timeDelta;

				while (_leftOverTime >= TargetElapsedSeconds)
				{
					ElapsedSeconds = TargetElapsedSeconds;
					TotalSeconds += TargetElapsedSeconds;
					_leftOverTime -= TargetElapsedSeconds;

					UpdateRequired();
				}
			}
			else
			{
				ElapsedSeconds = timeDelta;
				TotalSeconds += timeDelta;
				_leftOverTime = 0;

				UpdateRequired();
			}
		}
	}
}