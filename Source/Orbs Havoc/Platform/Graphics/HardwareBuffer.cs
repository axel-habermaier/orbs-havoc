namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using SharpDX;
	using SharpDX.Direct3D11;
	using Utilities;
	using Buffer = SharpDX.Direct3D11.Buffer;

	public sealed unsafe class HardwareBuffer : GraphicsObject
	{
		private readonly Buffer _buffer;
		private bool _isMapped;
		private uint _lastChanged;

		public HardwareBuffer(GraphicsDevice device, BindFlags bindFlags, ResourceUsage usage, int sizeInBytes, void* data = null)
			: base(device)
		{
			Assert.ArgumentInRange(bindFlags, nameof(bindFlags));
			Assert.ArgumentInRange(usage, nameof(usage));

			var desc = new BufferDescription
			{
				BindFlags = bindFlags,
				CpuAccessFlags = usage == ResourceUsage.Dynamic ? CpuAccessFlags.Write : CpuAccessFlags.None,
				OptionFlags = ResourceOptionFlags.None,
				SizeInBytes = sizeInBytes,
				Usage = usage
			};

			if (data != null)
			{
				var dataStream = new DataStream(new IntPtr(data), sizeInBytes, canRead: true, canWrite: false);
				_buffer = new Buffer(Device, dataStream, desc);
			}
			else
				_buffer = new Buffer(Device, desc);

			SizeInBytes = sizeInBytes;
		}

		public int SizeInBytes { get; }

		public static implicit operator Buffer(HardwareBuffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(data, nameof(data));

			var buffer = Map(MapMode.WriteDiscard);
			Interop.Copy(buffer, data, SizeInBytes);
			Unmap();
		}

		public void* Map(MapMode mode)
		{
			Assert.That(!_isMapped, "The buffer has already been mapped.");
			Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");
			Assert.ArgumentInRange(mode, nameof(mode));

			_isMapped = true;
			_lastChanged = State.FrameNumber;

			return Context.MapSubresource(_buffer, 0, mode, MapFlags.None).DataPointer.ToPointer();
		}

		public void Unmap()
		{
			if (!_isMapped)
				return;

			Context.UnmapSubresource(_buffer, 0);
			_isMapped = false;
		}

		protected override void OnDisposing()
		{
			Unmap();
			_buffer.SafeDispose();
		}
	}
}