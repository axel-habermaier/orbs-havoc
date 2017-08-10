namespace OrbsHavoc.Rendering
{
	using Assets;
	using Platform.Graphics;

	/// <summary>
	///   Copies a render target to another.
	/// </summary>
	internal sealed class CopyEffect : FullscreenEffect
	{
		/// <summary>
		///   Executes the render operation.
		/// </summary>
		internal override void Execute()
		{
			Input.Texture.Bind(0);
			SamplerState.Bilinear.Bind(0);
			BlendOperation.Opaque.Bind();
			AssetBundle.CopyShader.Bind();

			DrawFullscreen(Output);
		}
	}
}