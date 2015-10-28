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
	using System;
	using Platform;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Shows statistics about the performance of the application and other information useful for debugging.
	/// </summary>
	internal sealed class DebugOverlayView : View
	{
		/// <summary>
		///   The update frequency of the statistics in Hz.
		/// </summary>
		private const int UpdateFrequency = 30;

		/// <summary>
		///   The number of measurements that are used to calculate an average.
		/// </summary>
		private const int AverageSampleCount = 16;

		private readonly Label _cpuTimeLabel = new Label();
		private readonly WeakReference _gcCheck = new WeakReference(new object());
		private readonly Label _gcLabel = new Label("0");
		private readonly Label _gpuTimeLabel = new Label();
		private readonly Label _renderTimeLabel = new Label();
		private readonly Label _updateTimeLabel = new Label();
		private readonly Label _vsyncLabel = new Label(Cvars.Vsync.ToString().ToLower());
		private AveragedDouble _cpuFrameTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuRenderTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuUpdateTime = new AveragedDouble(AverageSampleCount);
		private int _garbageCollections;
		private AveragedDouble _gpuFrameTime = new AveragedDouble(AverageSampleCount);
		private Timer _timer = new Timer(1000.0 / UpdateFrequency);

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
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			const float margin = 3;
			RootElement.Child = new Border
			{
				Margin = new Thickness(5),
				Background = new Color(0xAA000000),
				Padding = new Thickness(10),
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Bottom,
				Child = new StackPanel
				{
					Children =
					{
						CreateLine("Platform:    ", new Label($"{PlatformInfo.Platform} {IntPtr.Size * 8}bit"), margin),
						CreateLine("Debug Mode:  ", new Label($"{PlatformInfo.IsDebug.ToString().ToLower()}"), margin),
						CreateLine("VSync:       ", _vsyncLabel, margin),
						CreateLine("# of GCs:    ", _gcLabel, 5 * margin),
						CreateLine("GPU Time:    ", _gpuTimeLabel, "ms", margin),
						CreateLine("CPU Time:    ", _cpuTimeLabel, "ms", margin),
						CreateLine("Update Time: ", _updateTimeLabel, "ms", margin),
						CreateLine("Render Time: ", _renderTimeLabel, "ms", 0)
					}
				}
			};

			Cvars.ShowDebugOverlayChanged += ChangeActivation;
			Cvars.VsyncChanged += OnVsyncChanged;

			ChangeActivation();
			UpdateStatistics();

			_timer.Timeout += UpdateStatistics;
		}

		/// <summary>
		///   Creates a debug output line.
		/// </summary>
		private static UIElement CreateLine(string text, Label label, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label(text), label },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}

		/// <summary>
		///   Creates a debug output line.
		/// </summary>
		private static UIElement CreateLine(string text, Label label, string suffix, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label(text), label, new Label(suffix) },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}

		/// <summary>
		///   Changes the view's activation state.
		/// </summary>
		private void ChangeActivation()
		{
			IsActive = Cvars.ShowDebugOverlay;
		}

		/// <summary>
		///   Updates the view's state.
		/// </summary>
		public override void Update()
		{
			base.Update();

			if (!IsActive)
				return;

			if (!_gcCheck.IsAlive)
			{
				++_garbageCollections;
				_gcLabel.Text = _garbageCollections.ToString();
				_gcCheck.Target = new object();
			}

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

			_gpuTimeLabel.Text = _gpuFrameTime.Average.ToString("F2");
			_cpuTimeLabel.Text = _cpuFrameTime.Average.ToString("F2");
			_updateTimeLabel.Text = _cpuUpdateTime.Average.ToString("F2");
			_renderTimeLabel.Text = _cpuRenderTime.Average.ToString("F2");
		}

		/// <summary>
		///   Shows the new vsync value.
		/// </summary>
		private void OnVsyncChanged()
		{
			_vsyncLabel.Text = Cvars.Vsync.ToString().ToLower();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			base.OnDisposing();

			Cvars.ShowDebugOverlayChanged -= ChangeActivation;
			Cvars.VsyncChanged -= OnVsyncChanged;
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