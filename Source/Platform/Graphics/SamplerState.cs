// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Platform.Graphics
{
	using Memory;
	using static OpenGL3;

	/// <summary>
	///   Describes a sampler state of a shader pipeline stage.
	/// </summary>
	public sealed unsafe class SamplerState : GraphicsObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private SamplerState(uint filter, uint addressMode)
		{
			Handle = Allocate(glGenSamplers, nameof(SamplerState));

			glSamplerParameteri(Handle, GL_TEXTURE_MIN_FILTER, (int)filter);
			glSamplerParameteri(Handle, GL_TEXTURE_MAG_FILTER, (int)filter);
			glSamplerParameteri(Handle, GL_TEXTURE_WRAP_S, (int)addressMode);
			glSamplerParameteri(Handle, GL_TEXTURE_WRAP_T, (int)addressMode);
			glSamplerParameteri(Handle, GL_TEXTURE_WRAP_R, (int)addressMode);

			CheckErrors();
		}

		/// <summary>
		///   Gets a sampler state with point-filtering and clamp address mode.
		/// </summary>
		public static SamplerState Point { get; private set; }

		/// <summary>
		///   Gets a sampler state with bilinear filtering and clamp address mode.
		/// </summary>
		public static SamplerState Bilinear { get; private set; }

		/// <summary>
		///   Initializes the sampler states.
		/// </summary>
		public static void Initialize()
		{
			Point = new SamplerState(GL_NEAREST, GL_CLAMP_TO_EDGE);
			Bilinear = new SamplerState(GL_LINEAR, GL_CLAMP_TO_EDGE);

			Bilinear.Bind();
		}

		/// <summary>
		///   Disposes the sampler states.
		/// </summary>
		public static void Dispose()
		{
			Point.SafeDispose();
			Bilinear.SafeDispose();
		}

		/// <summary>
		///   Binds the sampler state for rendering.
		/// </summary>
		public void Bind()
		{
			if (Change(ref State.SamplerState, this))
				glBindSampler(0, Handle);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.SamplerState, this);
			Deallocate(glDeleteSamplers, Handle);
		}
	}
}