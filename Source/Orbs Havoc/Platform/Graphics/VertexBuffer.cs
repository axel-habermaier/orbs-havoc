namespace OrbsHavoc.Platform.Graphics
{
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///     Represents a vertex buffer accessible by the GPU.
	/// </summary>
	internal sealed unsafe class VertexBuffer<T> : Buffer
		where T : unmanaged
	{
		private readonly int _elementCount;
		private readonly int _type;
		private bool _isMapped;
		private uint _lastChanged;

		/// <summary>
		///     Initializes a new buffer.
		/// </summary>
		/// <param name="bufferType">The OpenGL type of the buffer.</param>
		/// <param name="usage">The OpenGL buffer usage flag.</param>
		/// <param name="elementCount">The number of elements of type T that can be stored in the buffer.</param>
		/// <param name="data">The data that should be copied into the buffer, or null if no data should be copied.</param>
		private VertexBuffer(int bufferType, ResourceUsage usage, int elementCount, T* data = null)
			: base(Allocate(glGenBuffers, nameof(Buffer)))
		{
			Assert.ArgumentInRange(usage, nameof(usage));

			_type = bufferType;
			_elementCount = elementCount;

			glBindBuffer(_type, Handle);
			glBufferData(_type, _elementCount * sizeof(T), data, (int)usage);
		}

		/// <summary>
		///     Initializes a new buffer.
		/// </summary>
		/// <param name="usage">The OpenGL buffer usage flag.</param>
		/// <param name="elementCount">The number of elements of type T that can be stored in the buffer.</param>
		/// <param name="data">The data that should be copied into the buffer, or null if no data should be copied.</param>
		public static VertexBuffer<T> CreateVertexBuffer(ResourceUsage usage, int elementCount, T* data = null)
		{
			return new VertexBuffer<T>(GL_ARRAY_BUFFER, usage, elementCount, data);
		}

		/// <summary>
		///     Gets a pointer to the first element of the buffer.
		/// </summary>
		/// <param name="elementCount">The number of elements that can be written.</param>
		public T* Map(int elementCount)
		{
			Assert.That(!_isMapped, "The buffer has already been mapped.");
			Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");
			Assert.That(elementCount <= _elementCount, "Invalid element count.");

			_isMapped = true;
			_lastChanged = State.FrameNumber;

			glBindBuffer(_type, Handle);
			var pointer = glMapBufferRange(_type, null, elementCount * sizeof(T), GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT);

			return (T*)pointer;
		}

		/// <summary>
		///     Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			if (!_isMapped)
				return;

			glBindBuffer(_type, Handle);
			glUnmapBuffer(_type);

			_isMapped = false;
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unmap();
			Deallocate(glDeleteBuffers, Handle);
		}
	}
}