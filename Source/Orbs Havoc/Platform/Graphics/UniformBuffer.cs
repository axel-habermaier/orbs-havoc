namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a uniform buffer that provides constant data to shaders.
	/// </summary>
	public unsafe class UniformBuffer : DisposableObject
	{
		/// <summary>
		///   The underlying OpenGL handle of the buffer.
		/// </summary>
		private readonly int _buffer;

		/// <summary>
		///   The size in bytes of the buffer.
		/// </summary>
		private readonly int _sizeInBytes;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="sizeInBytes">The number of bytes the buffer should be able to store.</param>
		public UniformBuffer(int sizeInBytes)
		{
			_buffer = Allocate(glGenBuffers, nameof(UniformBuffer));
			_sizeInBytes = sizeInBytes;

			var data = stackalloc byte[sizeInBytes];
			for (var i = 0; i < sizeInBytes; ++i)
				data[i] = 0;

			glBindBuffer(GL_UNIFORM_BUFFER, _buffer);
			glBufferData(GL_UNIFORM_BUFFER, _sizeInBytes, data, GL_DYNAMIC_DRAW);
		}

		/// <summary>
		///   Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(UniformBuffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Deallocate(glDeleteBuffers, _buffer);
			Unset(State.UniformBuffers, this);
		}

		/// <summary>
		///   Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.UniformBufferSlotCount);

			if (!Change(State.UniformBuffers, slot, this))
				return;

			glBindBufferBase(GL_UNIFORM_BUFFER, slot, _buffer);
		}

		/// <summary>
		///   Copies the given data to the buffer, overwriting all previous data.
		/// </summary>
		/// <param name="data">The data that should be copied.</param>
		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			glBindBuffer(GL_UNIFORM_BUFFER, _buffer);
			glBufferSubData(GL_UNIFORM_BUFFER, null, _sizeInBytes, data);
		}
	}
}