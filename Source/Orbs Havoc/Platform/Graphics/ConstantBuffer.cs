namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using SharpDX.Direct3D11;
	using Utilities;
	using Buffer = SharpDX.Direct3D11.Buffer;

	public sealed unsafe class ConstantBuffer : GraphicsObject
	{
		private readonly HardwareBuffer _buffer;

		public ConstantBuffer(GraphicsDevice device, int sizeInBytes)
			: base(device)
		{
			_buffer = new HardwareBuffer(device, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, sizeInBytes);
		}

		public static implicit operator Buffer(ConstantBuffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		protected override void OnDisposing()
		{
			_buffer.SafeDispose();
			GraphicsState.Unset(State.ConstantBuffers, this);
		}

		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.UniformBufferSlotCount);

			if (!GraphicsState.Change(State.ConstantBuffers, slot, this))
				return;

			Context.VertexShader.SetConstantBuffer(slot, this);
			Context.PixelShader.SetConstantBuffer(slot, this);
		}

		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			_buffer.Copy(data);
		}
	}
}