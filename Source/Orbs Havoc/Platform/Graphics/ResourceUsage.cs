namespace OrbsHavoc.Platform.Graphics
{
	using static OpenGL3;

	/// <summary>
	///   Determines the usage pattern of a GPU resource.
	/// </summary>
	internal enum ResourceUsage
	{
		Dynamic = GL_DYNAMIC_DRAW,
		Static = GL_STATIC_DRAW
	}
}