// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Views
{
	using System;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///     Shows statistics about the performance of the application and other information useful for debugging.
	/// </summary>
	internal sealed class DebugOverlay : View
	{
		/// <summary>
		///     The update frequency of the statistics in Hz.
		/// </summary>
		private const int UpdateFrequency = 30;

		/// <summary>
		///     The number of measurements that are used to calculate an average.
		/// </summary>
		private const int AverageSampleCount = 32;

		private readonly Label _cpuTimeLabel = new Label();
		private readonly Label _drawCallsLabel = new Label();
		private readonly Label _fpsLabel = new Label();
		private readonly Label[] _gcLabels = new Label[GC.MaxGeneration + 1];
		private readonly Label _gpuTimeLabel = new Label();
		private readonly Label _renderTimeLabel = new Label();
		private readonly Label _updateTimeLabel = new Label();
		private readonly Label _vertexCountLabel = new Label();
		private readonly Label _vsyncLabel = new Label { Text = Cvars.Vsync.ToString().ToLower() };
		private AveragedDouble _cpuFrameTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuRenderTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuUpdateTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _drawCalls = new AveragedDouble(AverageSampleCount / 2);
		private AveragedDouble _gpuFrameTime = new AveragedDouble(AverageSampleCount / 2);
		private Timer _timer = new Timer(1000.0 / UpdateFrequency);
		private AveragedDouble _vertexCount = new AveragedDouble(AverageSampleCount);

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		public DebugOverlay()
		{
			_cpuFrameTime.AddMeasurement(0.016);
			_cpuRenderTime.AddMeasurement(0.016);
			_cpuUpdateTime.AddMeasurement(0.016);
			_gpuFrameTime.AddMeasurement(0.016);

			for (var i = 0; i < _gcLabels.Length; ++i)
				_gcLabels[i] = new Label { Text = "0" };
		}

		/// <summary>
		///     Sets number of draw calls drawn during the last frame that is displayed by the debug overlay.
		/// </summary>
		internal int DrawCalls
		{
			set => _drawCalls.AddMeasurement(value);
		}

		/// <summary>
		///     Sets the number of vertices drawn during the last frame that is displayed by the debug overlay.
		/// </summary>
		internal int VertexCount
		{
			set => _vertexCount.AddMeasurement(value);
		}

		/// <summary>
		///     Sets the GPU frame time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double GpuFrameTime
		{
			set => _gpuFrameTime.AddMeasurement(value);
		}

		/// <summary>
		///     Sets the CPU frame time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double CpuFrameTime
		{
			set => _cpuFrameTime.AddMeasurement(value);
		}

		/// <summary>
		///     Sets the CPU update time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double CpuUpdateTime
		{
			set => _cpuUpdateTime.AddMeasurement(value);
		}

		/// <summary>
		///     Sets the CPU render time in milliseconds that is displayed by the debug overlay.
		/// </summary>
		internal double CpuRenderTime
		{
			set => _cpuRenderTime.AddMeasurement(value);
		}

		/// <summary>
		///     Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			const float margin = 3;

			var gcPanel = new StackPanel { Orientation = Orientation.Horizontal };
			for (var i = 0; i < _gcLabels.Length; ++i)
			{
				gcPanel.Children.Add(_gcLabels[i]);

				if (i < _gcLabels.Length - 1)
					gcPanel.Children.Add(new Label { Text = "/" });
			}

			RootElement = new Border
			{
				MinWidth = 170,
				IsHitTestVisible = false,
				Margin = new Thickness(5),
				Background = new Color(0xAA000000),
				Padding = new Thickness(10),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Child = new StackPanel
				{
					Children =
					{
#if DEBUG
						CreateLine("Debug Build:  ", new Label { Text = "true" }, margin),
#else
						CreateLine("Debug Build:  ", new Label { Text = "false" }, margin),
#endif
						CreateLine("VSync:        ", _vsyncLabel, margin),
						CreateLine("# GCs:        ", gcPanel, margin),
						CreateLine("# Draw Calls: ", _drawCallsLabel, margin),
						CreateLine("# Vertices:   ", _vertexCountLabel, 5 * margin),
						CreateLine("FPS:          ", _fpsLabel, margin),
						CreateLine("GPU Time:     ", _gpuTimeLabel, "ms", margin),
						CreateLine("CPU Time:     ", _cpuTimeLabel, "ms", margin),
						CreateLine("Update Time:  ", _updateTimeLabel, "ms", margin),
						CreateLine("Render Time:  ", _renderTimeLabel, "ms", 0)
					}
				}
			};

			Cvars.VsyncChanged += OnVsyncChanged;

			Show();
			UpdateStatistics();

			_timer.Timeout += UpdateStatistics;
		}

		/// <summary>
		///     Creates a debug output line.
		/// </summary>
		private static UIElement CreateLine(string text, UIElement element, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label { Text = text }, element },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}

		/// <summary>
		///     Creates a debug output line.
		/// </summary>
		private static UIElement CreateLine(string text, UIElement element, string suffix, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label { Text = text }, element, new Label { Text = suffix } },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}

		/// <summary>
		///     Updates the view's state.
		/// </summary>
		public override void Update()
		{
			_timer.Update();
			RootElement.Visibility = Cvars.ShowDebugOverlay ? Visibility.Visible : Visibility.Collapsed;
		}

		/// <summary>
		///     Updates the statistics.
		/// </summary>
		private void UpdateStatistics()
		{
			if (!Cvars.ShowDebugOverlay)
				return;

			_fpsLabel.Text = StringCache.GetString((int)Math.Round(1.0 / _cpuFrameTime.Average * 1000));
			_gpuTimeLabel.Text = StringCache.GetString(_gpuFrameTime.Average);
			_cpuTimeLabel.Text = StringCache.GetString(_cpuFrameTime.Average);
			_updateTimeLabel.Text = StringCache.GetString(_cpuUpdateTime.Average);
			_renderTimeLabel.Text = StringCache.GetString(_cpuRenderTime.Average);
			_drawCallsLabel.Text = StringCache.GetString((int)Math.Round(_drawCalls.Average));
			_vertexCountLabel.Text = StringCache.GetString((int)Math.Round(_vertexCount.Average));

			for (var i = 0; i < _gcLabels.Length; ++i)
				_gcLabels[i].Text = StringCache.GetString(GC.CollectionCount(i));
		}

		/// <summary>
		///     Shows the new vsync value.
		/// </summary>
		private void OnVsyncChanged()
		{
			_vsyncLabel.Text = Cvars.Vsync.ToString().ToLower();
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Cvars.VsyncChanged -= OnVsyncChanged;
			_timer.Timeout -= UpdateStatistics;
		}

		/// <summary>
		///     Represents a measurement that is averaged over a certain number of samples.
		/// </summary>
		private struct AveragedDouble
		{
			/// <summary>
			///     The last couple of values for a more stable average.
			/// </summary>
			private readonly double[] _values;

			/// <summary>
			///     The current write index in the average array (circular writes).
			/// </summary>
			private int _averageIndex;

			/// <summary>
			///     A value indicating whether the entire average array has been filled at least once.
			/// </summary>
			private bool _averageIsFilled;

			/// <summary>
			///     The maximum supported value.
			/// </summary>
			private double _max;

			/// <summary>
			///     The minimum supported value.
			/// </summary>
			private double _min;

			/// <summary>
			///   The last value that has been measured.
			/// </summary>
			private double _lastValue;

			/// <summary>
			///     Initializes a new instance.
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
			///     Gets the averaged value.
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
			///     Adds the given measured value to the statistics.
			/// </summary>
			/// <param name="value">The value that should be added.</param>
			internal void AddMeasurement(double value)
			{
				_lastValue = value;

				if (_lastValue > _max)
					_max = _lastValue;
				if (_lastValue < _min)
					_min = _lastValue;

				_values[_averageIndex] = _lastValue;
				_averageIndex = (_averageIndex + 1) % _values.Length;

				if (_averageIndex == 0)
					_averageIsFilled = true;
			}
		}
	}
}