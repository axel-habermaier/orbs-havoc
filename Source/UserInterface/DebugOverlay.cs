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

namespace PointWars.UserInterface
{
	using System;
	using System.Numerics;
	using System.Text;
	using Platform;
	using Platform.Memory;
	using Rendering;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Manages statistics about the performance of the application and other information useful for debugging.
	/// </summary>
	public sealed class DebugOverlay : DisposableObject
	{
		/// <summary>
		///   The update frequency of the statistics in Hz.
		/// </summary>
		private const int UpdateFrequency = 30;

		/// <summary>
		///   The number of measurements that are used to calculate an average.
		/// </summary>
		private const int AverageSampleCount = 16;

		/// <summary>
		///   The string builder that is used to construct the output.
		/// </summary>
		private readonly StringBuilder _builder = new StringBuilder();

		/// <summary>
		///   A weak reference to an object to which no strong reference exists. When the weak reference is no longer set to a
		///   valid instance of an object, it is an indication that a garbage collection has occurred.
		/// </summary>
		private readonly WeakReference _gcCheck = new WeakReference(new object());

		/// <summary>
		///   The label that is used to draw platform info and frame stats.
		/// </summary>
		private readonly Label _platformInfo;

		/// <summary>
		///   The static debug overlay output.
		/// </summary>
		private readonly string _staticOutput;

		/// <summary>
		///   The total CPU frame time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		private AveragedDouble _cpuFrameTime = new AveragedDouble(AverageSampleCount);

		/// <summary>
		///   The CPU render time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		private AveragedDouble _cpuRenderTime = new AveragedDouble(AverageSampleCount);

		/// <summary>
		///   The CPU update time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		private AveragedDouble _cpuUpdateTime = new AveragedDouble(AverageSampleCount);

		/// <summary>
		///   The approximate amount of garbage collections that have occurred since the application has been started.
		/// </summary>
		private int _garbageCollections;

		/// <summary>
		///   The GPU frame time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		private AveragedDouble _gpuFrameTime = new AveragedDouble(AverageSampleCount);

		/// <summary>
		///   The timer that is used to periodically update the statistics.
		/// </summary>
		private Timer _timer = new Timer(1000.0 / UpdateFrequency);

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		internal DebugOverlay()
		{
			_platformInfo = new Label(Assets.DefaultFont) { LineSpacing = 2, Alignment = TextAlignment.Bottom };
			_timer.Timeout += UpdateStatistics;

			_builder.Append("Platform:    ").Append(PlatformInfo.Platform).Append(" ").Append(IntPtr.Size * 8).Append("bit\n");
			_builder.Append("Debug Mode:  ").Append(PlatformInfo.IsDebug.ToString().ToLower()).Append("\n");
			_staticOutput = _builder.ToString();
			_builder.Clear();
		}

		/// <summary>
		///   Sets the GPU frame time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double GpuFrameTime
		{
			set { _gpuFrameTime.AddMeasurement(value); }
		}

		/// <summary>
		///   Sets the CPU update time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double CpuUpdateTime
		{
			set { _cpuUpdateTime.AddMeasurement(value); }
		}

		/// <summary>
		///   Sets the CPU render time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double CpuRenderTime
		{
			set { _cpuRenderTime.AddMeasurement(value); }
		}

		/// <summary>
		///   Updates the statistics.
		/// </summary>
		/// <param name="size">The size of the area that the statistics should be drawn on.</param>
		internal void Update(Size size)
		{
			if (!_gcCheck.IsAlive)
			{
				++_garbageCollections;
				_gcCheck.Target = new object();
			}

			const int padding = 5;
			_platformInfo.Area = new Rectangle(padding, padding, size.Width - 2 * padding, size.Height - 2 * padding);

			_cpuFrameTime.AddMeasurement(_cpuUpdateTime.LastValue + _cpuRenderTime.LastValue);
			_timer.Update();
		}

		/// <summary>
		///   Updates the statistics.
		/// </summary>
		private void UpdateStatistics()
		{
			if (!Cvars.ShowDebugOverlay)
				return;

			_builder.Append(_staticOutput);
			_builder.Append("VSync:       ").Append(Cvars.Vsync.ToString().ToLower()).Append("\n");
			_builder.Append("# of GCs:    ").Append(_garbageCollections).Append("\n\n");
			_builder.Append("GPU Time:    ").Append(_gpuFrameTime.Average.ToString("F2")).Append("ms\n");
			_builder.Append("CPU Time:    ").Append(_cpuFrameTime.Average.ToString("F2")).Append("ms\n");
			_builder.Append("Update Time: ").Append(_cpuUpdateTime.Average.ToString("F2")).Append("ms\n");
			_builder.Append("Render Time: ").Append(_cpuRenderTime.Average.ToString("F2")).Append("ms");

			_platformInfo.Text = _builder.ToString();
			_builder.Clear();
		}

		/// <summary>
		///   Draws the statistics.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used to draw the statistics.</param>
		internal void Draw(SpriteBatch spriteBatch)
		{
			spriteBatch.Layer = Int32.MaxValue - 2;
			spriteBatch.WorldMatrix = Matrix4x4.Identity;

			if (Cvars.ShowDebugOverlay)
				_platformInfo.Draw(spriteBatch);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_platformInfo.SafeDispose();
			_timer.Timeout -= UpdateStatistics;
		}

		/// <summary>
		///   Represents a measurement that is averaged over a certain number of samples.
		/// </summary>
		internal struct AveragedDouble
		{
			/// <summary>
			///   The last couple of values for a more stable average.
			/// </summary>
			private readonly double[] _values;

			/// <summary>
			///   The current write index in the average array (circular writes).
			/// </summary>
			private int _averageIndex;

			/// <summary>
			///   A value indicating whether the entire average array has been filled at least once.
			/// </summary>
			private bool _averageIsFilled;

			/// <summary>
			///   The maximum supported value.
			/// </summary>
			private double _max;

			/// <summary>
			///   The minimum supported value.
			/// </summary>
			private double _min;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="sampleCount">The number of samples for the computation of the average.</param>
			public AveragedDouble(int sampleCount)
				: this()
			{
				_values = new double[sampleCount];
				_max = Double.MinValue;
				_min = Double.MaxValue;
			}

			/// <summary>
			///   Gets the last value that has been measured.
			/// </summary>
			internal double LastValue { get; private set; }

			/// <summary>
			///   Gets the averaged value.
			/// </summary>
			internal double Average
			{
				get
				{
					double average = 0;
					var count = _averageIsFilled ? _values.Length : _averageIndex;

					for (var i = 0; i < count; ++i)
						average += _values[i];

					average /= count;
					return average;
				}
			}

			/// <summary>
			///   Adds the given measured value to the statistics.
			/// </summary>
			/// <param name="value">The value that should be added.</param>
			internal void AddMeasurement(double value)
			{
				LastValue = value;

				if (LastValue > _max)
					_max = LastValue;
				if (LastValue < _min)
					_min = LastValue;

				_values[_averageIndex] = LastValue;
				_averageIndex = (_averageIndex + 1) % _values.Length;

				if (_averageIndex == 0)
					_averageIsFilled = true;
			}
		}
	}
}