namespace OrbsHavoc.Platform.Graphics
{
	using Memory;
	using Utilities;

	/// <summary>
	///     Represents a base class for a uniform, vertex or index buffer accessible by the GPU.
	/// </summary>
	internal abstract class Buffer : DisposableObject
	{
		/// <summary>
		///     The underlying OpenGL handle of the buffer.
		/// </summary>
		protected readonly int GpuBuffer;

		protected Buffer(int buffer)
		{
			GpuBuffer = buffer;
		}

		/// <summary>
		///     Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(Buffer buffer)
		{
			Assert.ArgumentNotNull(buffer, nameof(buffer));
			return buffer.GpuBuffer;
		}
	}
}