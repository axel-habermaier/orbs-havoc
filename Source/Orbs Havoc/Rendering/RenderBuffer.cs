namespace OrbsHavoc.Rendering
{
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///     Manages the GPU memory that contains all quad data used for rendering a frame.
	/// </summary>
	internal sealed unsafe class RenderBuffer : DisposableObject
	{
		/// <summary>
		///     The vertex buffers that are used to store the quad data.
		/// </summary>
		private readonly VertexBuffer<Quad>[] _dataBuffers = new VertexBuffer<Quad>[GraphicsState.MaxFrameLag];

		/// <summary>
		///     The vertex input layouts that describe the vertex buffers.
		/// </summary>
		private readonly VertexLayout[] _vertexLayouts = new VertexLayout[GraphicsState.MaxFrameLag];

		/// <summary>
		///     The current index into the vertex layout and data buffer arrays.
		/// </summary>
		private int _index;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		public RenderBuffer()
		{
			Assert.That(sizeof(Quad) == Quad.SizeInBytes, "Unexpected quad size.");

			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				_dataBuffers[i] = VertexBuffer<Quad>.CreateVertexBuffer(ResourceUsage.Dynamic, QuadCollection.MaxQuads);
				_vertexLayouts[i] = new VertexLayout(
					VertexAttribute.Create(_dataBuffers[i], DataFormat.Float, 2, sizeof(float), Quad.SizeInBytes, false), // Positions
					VertexAttribute.Create(_dataBuffers[i], DataFormat.Float, 1, sizeof(float), Quad.SizeInBytes, false), // Orientations
					VertexAttribute.Create(_dataBuffers[i], DataFormat.UnsignedByte, 4, sizeof(byte), Quad.SizeInBytes, true), // Colors
					VertexAttribute.Create(_dataBuffers[i], DataFormat.UnsignedShort, 2, sizeof(ushort), Quad.SizeInBytes, false), // Sizes
					VertexAttribute.Create(_dataBuffers[i], DataFormat.UnsignedShort, 4, sizeof(ushort), Quad.SizeInBytes, true)); // Tex Coords
			}
		}

		/// <summary>
		///     Binds the render buffer for rendering.
		/// </summary>
		public void Bind()
		{
			_vertexLayouts[_index].Bind();
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_dataBuffers.SafeDisposeAll();
			_vertexLayouts.SafeDisposeAll();
		}

		/// <summary>
		///     Maps the buffer for data upload to the GPU.
		/// </summary>
		/// <param name="quadCount">The number of quads that can be written.</param>
		public Quad* Map(int quadCount)
		{
			_index = (_index + 1) % GraphicsState.MaxFrameLag;
			return _dataBuffers[_index].Map(quadCount);
		}

		/// <summary>
		///     Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			_dataBuffers[_index].Unmap();
		}
	}
}