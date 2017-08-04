namespace OrbsHavoc.Gameplay.Client
{
	using System.Collections.Generic;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Renders the entities of a game session.
	/// </summary>
	internal class EntityRenderer
	{
		private readonly List<IRenderable> _renderables = new List<IRenderable>();

		/// <summary>
		///   Adds the given renderable object.
		/// </summary>
		/// <param name="renderable">The renderable object that should be added.</param>
		public void Add(IRenderable renderable)
		{
			Assert.ArgumentNotNull(renderable, nameof(renderable));
			Assert.ArgumentSatisfies(!_renderables.Contains(renderable), nameof(renderable), "The sprite has already been added.");

			_renderables.Add(renderable);
		}

		/// <summary>
		///   Removes the given renderable object.
		/// </summary>
		/// <param name="renderable">The renderable object that should be removed.</param>
		public void Remove(IRenderable renderable)
		{
			Assert.ArgumentNotNull(renderable, nameof(renderable));
			Assert.ArgumentSatisfies(_renderables.Contains(renderable), nameof(renderable), "The sprite has not been added.");

			_renderables.Remove(renderable);
		}

		/// <summary>
		///   Draws the sprites using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing.</param>
		public void Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			foreach (var renderable in _renderables)
				renderable.Draw(spriteBatch);
		}
	}
}