namespace OrbsHavoc.Rendering
{
	using Platform.Memory;

	/// <summary>
	///   Represents an operation performed by a renderer.
	/// </summary>
	public abstract class RenderOperation : PooledObject
	{
		/// <summary>
		///   Gets the renderer that executes the operation.
		/// </summary>
		public Renderer Renderer { get; internal set; }

		/// <summary>
		///   Executes the render operation.
		/// </summary>
		internal abstract void Execute();
	}
}