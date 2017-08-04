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
	using Scripting;
	using UI;
	using UserInterface;
	using Utilities;

	internal sealed class DebugOverlay : View<DebugOverlayUI>
	{
		/// <summary>
		///     The update frequency of the statistics in Hz.
		/// </summary>
		private const int UpdateFrequency = 30;

		/// <summary>
		///     The number of measurements that are used to calculate an average.
		/// </summary>
		private const int AverageSampleCount = 32;

		private AveragedDouble _cpuFrameTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuRenderTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _cpuUpdateTime = new AveragedDouble(AverageSampleCount);
		private AveragedDouble _drawCalls = new AveragedDouble(AverageSampleCount / 2);
		private AveragedDouble _gpuFrameTime = new AveragedDouble(AverageSampleCount / 2);
		private Timer _timer = new Timer(1000.0 / UpdateFrequency);
		private AveragedDouble _vertexCount = new AveragedDouble(AverageSampleCount);

		public DebugOverlay()
		{
			_cpuFrameTime.AddMeasurement(0.016);
			_cpuRenderTime.AddMeasurement(0.016);
			_cpuUpdateTime.AddMeasurement(0.016);
			_gpuFrameTime.AddMeasurement(0.016);
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

		public override void Initialize()
		{
			Cvars.VsyncChanged += OnVsyncChanged;

			Show();
			UpdateStatistics();

			_timer.Timeout += UpdateStatistics;
		}

		public override void Update()
		{
			_timer.Update();
			RootElement.Visibility = Cvars.ShowDebugOverlay ? Visibility.Visible : Visibility.Collapsed;
		}

		private void UpdateStatistics()
		{
			if (!Cvars.ShowDebugOverlay)
				return;

			UI.FpsLabel.Text = StringCache.GetString((int)Math.Round(1.0 / _cpuFrameTime.Average * 1000));
			UI.GpuTimeLabel.Text = StringCache.GetString(_gpuFrameTime.Average);
			UI.CpuTimeLabel.Text = StringCache.GetString(_cpuFrameTime.Average);
			UI.UpdateTimeLabel.Text = StringCache.GetString(_cpuUpdateTime.Average);
			UI.RenderTimeLabel.Text = StringCache.GetString(_cpuRenderTime.Average);
			UI.DrawCallsLabel.Text = StringCache.GetString((int)Math.Round(_drawCalls.Average));
			UI.VertexCountLabel.Text = StringCache.GetString((int)Math.Round(_vertexCount.Average));

			for (var i = 0; i < UI.GcLabels.Length; ++i)
				UI.GcLabels[i].Text = StringCache.GetString(GC.CollectionCount(i));
		}

		private void OnVsyncChanged()
		{
			UI.VsyncLabel.Text = Cvars.Vsync.ToString().ToLower();
		}

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

			private double _max;
			private double _min;
			private double _lastValue;

			public AveragedDouble(int sampleCount)
				: this()
			{
				_values = new double[sampleCount];
				_max = Double.MinValue;
				_min = Double.MaxValue;
			}

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