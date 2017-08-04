namespace OrbsHavoc.Platform.Graphics
{
	using static OpenGL3;

	/// <summary>
	///   Represents the type of primitive that is drawn by a draw call.
	/// </summary>
	public enum PrimitiveType
	{
		Points = GL_POINTS,
		Triangles = GL_TRIANGLES
	}
}