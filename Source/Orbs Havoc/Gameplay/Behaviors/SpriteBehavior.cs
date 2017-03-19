// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Gameplay.Behaviors
{
	using Client;
	using Platform.Graphics;
	using Rendering;
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Represents a behavior that draws a sprite.
	/// </summary>
	internal class SpriteBehavior : Behavior<SceneNode>, IRenderable
	{
		private GameSession _gameSession;

		/// <summary>
		///   Gets or sets the texture of the sprite.
		/// </summary>
		public Texture Texture { get; set; }

		/// <summary>
		///   Gets or sets the color of the sprite.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		///   Gets or sets the layer the sprite is drawn on.
		/// </summary>
		public int Layer { get; set; }

		/// <summary>
		///   Draws the sprite using the given sprite batch.
		/// </summary>
		/// <param name="spriteBatch">The sprite batch that should be used for drawing.</param>
		void IRenderable.Draw(SpriteBatch spriteBatch)
		{
			Assert.ArgumentNotNull(spriteBatch, nameof(spriteBatch));

			spriteBatch.RenderState.Layer = Layer;
			spriteBatch.Draw(Texture, SceneNode.WorldPosition, -SceneNode.Orientation, Color);
		}

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			_gameSession.EntityRenderer.Add(this);
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			_gameSession.EntityRenderer.Remove(this);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the sprite belongs to.</param>
		/// <param name="texture">The texture that should be used to draw the sprite.</param>
		/// <param name="color">The color that should be used to draw the sprite.</param>
		/// <param name="layer">The layer the sprite should be drawn on.</param>
		public static SpriteBehavior Create(GameSession gameSession, Texture texture, Color color, int layer)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));
			Assert.ArgumentNotNull(texture, nameof(texture));

			var sprite = gameSession.Allocator.Allocate<SpriteBehavior>();
			sprite._gameSession = gameSession;
			sprite.Texture = texture;
			sprite.Color = color;
			sprite.Layer = layer;

			return sprite;
		}
	}
}