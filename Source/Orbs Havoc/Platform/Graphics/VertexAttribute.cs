﻿namespace OrbsHavoc.Platform.Graphics
{
	using Utilities;

	/// <summary>
	///     Represents an attribute of a vertex layout.
	/// </summary>
	internal readonly struct VertexAttribute
	{
		/// <summary>
		///     The buffer the data is read from.
		/// </summary>
		public readonly Buffer Buffer;

		/// <summary>
		///     The number of components accessed by the attribute.
		/// </summary>
		public readonly int ComponentCount;

		/// <summary>
		///     The data format of the attribute.
		/// </summary>
		public readonly DataFormat DataFormat;

		/// <summary>
		///     The number of bytes accessed by the attribute.
		/// </summary>
		public readonly int SizeInBytes;

		/// <summary>
		///     Indicates whether the attribute values should be normalized.
		/// </summary>
		public readonly bool Normalize;

		/// <summary>
		///     The stride in bytes between two consecutive values of the attribute.
		/// </summary>
		public readonly int StrideInBytes;

		private VertexAttribute(Buffer buffer, DataFormat dataFormat, int componentCount, int sizeInBytes, int strideInBytes, bool normalize)
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

		/// <summary>
		///     Creates a new instance.
		/// </summary>
		/// <param name="buffer">The buffer the data is read from.</param>
		/// <param name="dataFormat">The data format of the attribute.</param>
		/// <param name="componentCount">The number of components accessed by the attribute.</param>
		/// <param name="sizeInBytes">The number of bytes accessed by the attribute.</param>
		/// <param name="strideInBytes">The stride in bytes between two consecutive values of the attribute.</param>
		/// <param name="normalize">Indicates whether the attribute values should be normalized.</param>
		public static VertexAttribute Create<T>(VertexBuffer<T> buffer, DataFormat dataFormat, int componentCount, int sizeInBytes,
			int strideInBytes, bool normalize)
			where T : unmanaged
		{
			return new VertexAttribute(buffer, dataFormat, componentCount, sizeInBytes, strideInBytes, normalize);
		}
	}
}