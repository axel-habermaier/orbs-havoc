﻿namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a vertex or index buffer accessible by the GPU.
	/// </summary>
	internal sealed unsafe class Buffer : DisposableObject
	{
		private readonly int _buffer;
		private readonly int _type;

		/// <summary>
		///   Indicates whether the buffer is currently mapped.
		/// </summary>
		private bool _isMapped;

		/// <summary>
		///   The GPU frame number when the buffer was last changed.
		/// </summary>
		private uint _lastChanged;

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		/// <param name="bufferType">The OpenGL type of the buffer.</param>
		/// <param name="usage">The OpenGL buffer usage flag.</param>
		/// <param name="sizeInBytes">The size of the buffer in bytes.</param>
		/// <param name="data">The data that should be copied into the buffer, or null if no data should be copied.</param>
		private Buffer(int bufferType, ResourceUsage usage, int sizeInBytes, void* data = null)
		{
			Assert.ArgumentInRange(usage, nameof(usage));

			_buffer = Allocate(glGenBuffers, nameof(Buffer));
			_type = bufferType;
			SizeInBytes = sizeInBytes;

			glBindBuffer(_type, _buffer);
			glBufferData(_type, SizeInBytes, data, (int)usage);
		}

		/// <summary>
		///   Gets the size of the buffer in bytes.
		/// </summary>
		public int SizeInBytes { get; }

		/// <summary>
		///   Initializes a new buffer.
		/// </summary>
		/// <param name="usage">The OpenGL buffer usage flag.</param>
		/// <param name="sizeInBytes">The size of the buffer in bytes.</param>
		/// <param name="data">The data that should be copied into the buffer, or null if no data should be copied.</param>
		public static Buffer CreateVertexBuffer(ResourceUsage usage, int sizeInBytes, void* data = null)
		{
			return new Buffer(GL_ARRAY_BUFFER, usage, sizeInBytes, data);
		}

		/// <summary>
		///   Casts the buffer to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator int(Buffer obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj._buffer;
		}

		/// <summary>
		///   Copies the given data to the buffer, overwriting all previous data.
		/// </summary>
		/// <param name="data">The data that should be copied.</param>
		public void Copy(void* data)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(new IntPtr(data), nameof(data));

			glBindBuffer(_type, _buffer);
			glBufferSubData(_type, null, SizeInBytes, data);
		}

		/// <summary>
		///   Gets a pointer to the first byte of the buffer.
		/// </summary>
		/// <param name="sizeInBytes">The number of bytes that can be written.</param>
		public void* Map(int sizeInBytes)
		{
			Assert.That(!_isMapped, "The buffer has already been mapped.");
			Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");
			Assert.That(sizeInBytes <= SizeInBytes, "Invalid size.");

			_isMapped = true;
			_lastChanged = State.FrameNumber;

			glBindBuffer(_type, _buffer);
			var pointer = glMapBufferRange(_type, null, sizeInBytes, GL_MAP_WRITE_BIT | GL_MAP_INVALIDATE_RANGE_BIT);

			return pointer;
		}

		/// <summary>
		///   Unmaps the buffer.
		/// </summary>
		public void Unmap()
		{
			if (!_isMapped)
				return;

			glBindBuffer(_type, _buffer);
			glUnmapBuffer(_type);

			_isMapped = false;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unmap();
			Deallocate(glDeleteBuffers, _buffer);
		}
	}
}