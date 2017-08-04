﻿namespace OrbsHavoc.Utilities
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