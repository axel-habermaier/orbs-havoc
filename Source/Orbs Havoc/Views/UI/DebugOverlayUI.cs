namespace OrbsHavoc.Views.UI
{
	using System;
	using Rendering;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;

	internal sealed class DebugOverlayUI : Border
	{
		public DebugOverlayUI()
		{
			for (var i = 0; i < GcLabels.Length; ++i)
				GcLabels[i] = new Label { Text = "0" };

			const float margin = 3;

			var gcPanel = new StackPanel { Orientation = Orientation.Horizontal };
			for (var i = 0; i < GcLabels.Length; ++i)
			{
				gcPanel.Children.Add(GcLabels[i]);

				if (i < GcLabels.Length - 1)
					gcPanel.Children.Add(new Label { Text = "/" });
			}

			MinWidth = 170;
			IsHitTestVisible = false;
			Margin = new Thickness(5);
			Background = new Color(0xAA000000);
			Padding = new Thickness(10);
			HorizontalAlignment = HorizontalAlignment.Right;
			VerticalAlignment = VerticalAlignment.Bottom;
			Child = new StackPanel
			{
				Children =
				{
#if DEBUG
					CreateLine("Debug Build:  ", new Label { Text = "true" }, margin),
#else
					CreateLine("Debug Build:  ", new Label { Text = "false" }, margin),
#endif
					CreateLine("VSync:        ", VsyncLabel, margin),
					CreateLine("# GCs:        ", gcPanel, margin),
					CreateLine("# Draw Calls: ", DrawCallsLabel, margin),
					CreateLine("# Vertices:   ", VertexCountLabel, 5 * margin),
					CreateLine("FPS:          ", FpsLabel, margin),
					CreateLine("GPU Time:     ", GpuTimeLabel, "ms", margin),
					CreateLine("CPU Time:     ", CpuTimeLabel, "ms", margin),
					CreateLine("Update Time:  ", UpdateTimeLabel, "ms", margin),
					CreateLine("Render Time:  ", RenderTimeLabel, "ms", 0)
				}
			};
		}

		public Label CpuTimeLabel { get; } = new Label();
		public Label DrawCallsLabel { get; } = new Label();
		public Label FpsLabel { get; } = new Label();
		public Label[] GcLabels { get; } = new Label[GC.MaxGeneration + 1];
		public Label GpuTimeLabel { get; } = new Label();
		public Label RenderTimeLabel { get; } = new Label();
		public Label UpdateTimeLabel { get; } = new Label();
		public Label VertexCountLabel { get; } = new Label();
		public Label VsyncLabel { get; } = new Label { Text = Cvars.Vsync.ToString().ToLower() };

		private static UIElement CreateLine(string text, UIElement element, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label { Text = text }, element },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}

		private static UIElement CreateLine(string text, UIElement element, string suffix, float marginBottom)
		{
			return new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new Label { Text = text }, element, new Label { Text = suffix } },
				Margin = new Thickness(0, 0, 0, marginBottom)
			};
		}
	}
}