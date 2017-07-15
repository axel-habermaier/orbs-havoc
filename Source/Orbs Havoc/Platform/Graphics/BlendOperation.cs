// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Describes a blend operation of the output merger pipeline stage.
	/// </summary>
	public enum BlendOperation
	{
		/// <summary>
		///   Indicates that no blending should be used and all drawn objects are opaque.
		/// </summary>
		Opaque = 1,

		/// <summary>
		///   Indicates that pre-multiplied alpha blending should be used.
		/// </summary>
		Premultiplied,

		/// <summary>
		///   Indicates that additive blending should be used.
		/// </summary>
		Additive,

		/// <summary>
		///   Indicates that alpha blending should be used.
		/// </summary>
		Alpha
	}

	/// <summary>
	///   Provides extension methods for blend operations.
	/// </summary>
	public static class BlendOperationExtensions
	{
		/// <summary>
		///   Binds the blend state for rendering.
		/// </summary>
		public static void Bind(this BlendOperation blendOperation)
		{
			if (!Change(ref State.BlendOperation, blendOperation))
				return;

			switch (blendOperation)
			{
				case BlendOperation.Opaque:
					glDisable(GL_BLEND);
					break;
				case BlendOperation.Premultiplied:
					glEnable(GL_BLEND);
					glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
					break;
				case BlendOperation.Additive:
					glEnable(GL_BLEND);
					glBlendFunc(GL_ONE, GL_ONE);
					break;
				case BlendOperation.Alpha:
					glEnable(GL_BLEND);
					glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
					break;
				default:
					Assert.NotReached("Unknown blend state.");
					break;
			}
		}
	}
}