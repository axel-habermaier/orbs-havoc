namespace OrbsHavoc.Platform.Graphics
{
	using Memory;
	using SharpDX.Direct3D11;
	using Utilities;

	public abstract class GraphicsObject : DisposableObject
	{
		protected GraphicsObject(GraphicsDevice device)
		{
			Assert.ArgumentNotNull(device, nameof(device));
			Device = device;
		}

		protected GraphicsDevice Device { get; }
		protected DeviceContext Context => null;
		protected GraphicsState State => null;
	}
}