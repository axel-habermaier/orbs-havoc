namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using SharpDX.DXGI;

	public static class FormatExtensions
	{
		public static int ComponentCount(this Format format)
		{
			switch (format)
			{
				case Format.R8G8B8A8_UInt:
					return 4;
				default:
					throw new ArgumentOutOfRangeException(nameof(format));
			}
		}
	}
}