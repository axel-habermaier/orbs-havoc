// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using Memory;
	using Utilities;
	using static GraphicsHelpers;
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
			CheckErrors();
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
			CheckErrors();

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
			CheckErrors();
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
			CheckErrors();
		}
	}
}