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
	using Utilities;

	/// <summary>
	///   Represents an attribute of a vertex layout.
	/// </summary>
	public struct VertexAttribute
	{
		/// <summary>
		///   The buffer the data is read from.
		/// </summary>
		public Buffer Buffer;

		/// <summary>
		///   The number of components accessed by the attribute.
		/// </summary>
		public int ComponentCount;

		/// <summary>
		///   The data format of the attribute.
		/// </summary>
		public DataFormat DataFormat;

		/// <summary>
		///   The number of bytes accessed by the attribute.
		/// </summary>
		public int SizeInBytes;

		/// <summary>
		///   Indicates whether the attribute values should be normalized.
		/// </summary>
		public bool Normalize;

		/// <summary>
		///   The stride in bytes between two consecutive values of the attribute.
		/// </summary>
		public int StrideInBytes;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="buffer">The buffer the data is read from.</param>
		/// <param name="dataFormat">The data format of the attribute.</param>
		/// <param name="componentCount">The number of components accessed by the attribute.</param>
		/// <param name="sizeInBytes">The number of bytes accessed by the attribute.</param>
		/// <param name="strideInBytes">The stride in bytes between two consecutive values of the attribute.</param>
		/// <param name="normalize">Indicates whether the attribute values should be normalized.</param>
		public VertexAttribute(Buffer buffer, DataFormat dataFormat, int componentCount, int sizeInBytes, int strideInBytes, bool normalize)
		{
			Assert.ArgumentNotNull(buffer, nameof(buffer));
			Assert.ArgumentInRange(dataFormat, nameof(dataFormat));

			Buffer = buffer;
			ComponentCount = componentCount;
			DataFormat = dataFormat;
			SizeInBytes = sizeInBytes;
			Normalize = normalize;
			StrideInBytes = strideInBytes;
		}
	}
}