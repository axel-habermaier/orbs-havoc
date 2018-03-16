namespace OrbsHavoc.Platform.Graphics
{
	using static OpenGL3;

	/// <summary>
	///   Represents a data format used by the GPU.
	/// </summary>
	internal enum DataFormat
	{
		Monochrome = GL_RED,
		Rgba = GL_RGBA,
		Float = GL_FLOAT,
		UnsignedByte = GL_UNSIGNED_BYTE,
		UnsignedShort = GL_UNSIGNED_SHORT
	}
}