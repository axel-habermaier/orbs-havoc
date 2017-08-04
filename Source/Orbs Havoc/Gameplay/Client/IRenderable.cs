namespace OrbsHavoc.Gameplay.Client
{
	using Rendering;

	/// <summary>
	///   Represents a renderable object.
	/// </summary>
	internal interface IRenderable
	{
		/// <summary>
		///   Draws the renderable using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing.</param>
		void Draw(SpriteBatch spriteBatch);
	}
}