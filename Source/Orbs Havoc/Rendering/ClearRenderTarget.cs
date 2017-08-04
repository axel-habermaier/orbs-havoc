namespace OrbsHavoc.Rendering
{
	using Platform.Graphics;
	using Utilities;

	/// <summary>
	///   Represents a render operation that clears a render target.
	/// </summary>
	public sealed class ClearRenderTarget : RenderOperation
	{
		/// <summary>
		///   Gets or sets the render target cleared by the operation.
		/// </summary>
		public RenderTarget RenderTarget { get; set; }

		/// <summary>
		///   Gets or sets the color that should be used to clear the render target.
		/// </summary>
		public Color ClearColor { get; set; }

		/// <summary>
		///   Executes the render operation.
		/// </summary>
		internal override void Execute()
		{
			Assert.NotNull(RenderTarget, "No render target has been set.");
			RenderTarget.Clear(ClearColor);
		}
	}
}