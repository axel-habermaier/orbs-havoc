namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using SharpDX;
	using SharpDX.Direct3D;
	using SharpDX.Direct3D11;
	using SharpDX.DXGI;
	using Utilities;

	public sealed unsafe class Texture : GraphicsObject
	{
		private ShaderResourceView _resourceView;
		private Texture2D _texture;

		public Texture(GraphicsDevice device)
			: base(device)
		{
		}

		public Texture(GraphicsDevice device, Size size, Format format, void* data)
			: base(device)
		{
			Initialize(size, format, data);
		}

		public Size Size { get; private set; }
		public float Width => Size.Width;
		public float Height => Size.Height;

		public static implicit operator ShaderResourceView(Texture obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._resourceView;
		}

		public void Load(ref BufferReader buffer)
		{
			var size = new Size(buffer.ReadUInt32(), buffer.ReadUInt32());
			var sizeInBytes = buffer.ReadInt32();

			using (var data = buffer.Pointer)
			{
				buffer.Skip(sizeInBytes);
				Initialize(size, Format.R8G8B8A8_UInt, data);
			}
		}

		private void Initialize(Size size, Format format, void* data)
		{
			Assert.ArgumentInRange(format, nameof(format));
			Assert.That(size.Width > 0 && size.Height > 0, "Invalid render target size.");
			Assert.ArgumentNotNull(data, nameof(data));

			OnDisposing();
			Size = size;

			var textureDesc = new Texture2DDescription
			{
				Width = Size.IntegralWidth,
				Height = Size.IntegralHeight,
				MipLevels = 1,
				ArraySize = 1,
				SampleDescription = new SampleDescription(1, 0),
				Usage = ResourceUsage.Default,
				BindFlags = BindFlags.ShaderResource,
				CpuAccessFlags = 0,
				OptionFlags = 0,
				Format = format
			};

			var length = Size.IntegralWidth * Size.IntegralHeight * format.ComponentCount();
			var stream = new DataStream(new IntPtr(data), length, canRead: true, canWrite: true);

			var textureData = new DataRectangle(stream.DataPointer, Size.IntegralWidth * format.ComponentCount());
			_texture = new Texture2D(Device, textureDesc, textureData);

			var viewDesc = new ShaderResourceViewDescription
			{
				Format = format,
				Dimension = ShaderResourceViewDimension.Texture2D,
				Texture2D = new ShaderResourceViewDescription.Texture2DResource()
			};

			_resourceView = new ShaderResourceView(Device, _texture, viewDesc);
		}

		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.That(_resourceView != null, "The texture has not been initialized.");

			if (!GraphicsState.Change(State.Textures, slot, this))
				return;

			Context.VertexShader.SetShaderResource(slot, _resourceView);
			Context.PixelShader.SetShaderResource(slot, _resourceView);
		}

		protected override void OnDisposing()
		{
			_texture.SafeDispose();
			_resourceView.SafeDispose();

			GraphicsState.Unset(State.Textures, this);
		}
	}
}