namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///     Represents a uniform buffer that provides constant data to shaders.
	/// </summary>
	internal sealed unsafe class UniformBuffer<T> : Buffer
		where T : unmanaged
	{
		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		public UniformBuffer()
			: base(Allocate(glGenBuffers, nameof(UniformBuffer<T>)))
		{
			var data = stackalloc byte[sizeof(T)];
			for (var i = 0; i < sizeof(T); ++i)
				data[i] = 0;

			glBindBuffer(GL_UNIFORM_BUFFER, GpuBuffer);
			glBufferData(GL_UNIFORM_BUFFER, sizeof(T), data, GL_DYNAMIC_DRAW);
		}

		/// <summary>
		///     Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Deallocate(glDeleteBuffers, GpuBuffer);
			Unset(State.UniformBuffers, this);
		}

		/// <summary>
		///     Binds the uniform buffer to the given slot.
		/// </summary>
		public void Bind(int slot)
		{
			Assert.NotDisposed(this);
			Assert.InRange(slot, 0, GraphicsState.UniformBufferSlotCount);

			if (!Change(State.UniformBuffers, slot, this))
				return;

			glBindBufferBase(GL_UNIFORM_BUFFER, slot, GpuBuffer);
		}

		/// <summary>
		///     Copies the given data to the buffer, overwriting all previous data.
		/// </summary>
		/// <param name="data">The data that should be copied.</param>
		public void Copy(T* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			glBindBuffer(GL_UNIFORM_BUFFER, GpuBuffer);
			glBufferSubData(GL_UNIFORM_BUFFER, null, sizeof(T), data);
		}
	}
}