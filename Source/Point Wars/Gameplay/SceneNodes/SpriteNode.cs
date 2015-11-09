// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Gameplay.SceneNodes
{
	using Platform.Graphics;
	using Platform.Memory;
	using Rendering;
	using Utilities;

	/// <summary>
	///   Represents a node that draws a sprite.
	/// </summary>
	public class SpriteNode : SceneNode
	{
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
		///   Draws the sprite using the given renderer.
		/// </summary>
		/// <param name="renderer">The renderer that should be used for drawing.</param>
		public void Draw(Renderer renderer)
		{
			renderer.Layer = Layer;
			renderer.Draw(Texture, Color, WorldMatrix);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the sprite.</param>
		/// <param name="parent">The parent scene node the sprite should be attached to.</param>
		/// <param name="texture">The texture that should be used to draw the sprite.</param>
		/// <param name="color">The color that should be used to draw the sprite.</param>
		/// <param name="layer">The layer the sprite should be drawn on.</param>
		public static SpriteNode Create(PoolAllocator allocator, SceneNode parent, Texture texture, Color color, int layer)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(parent, nameof(parent));
			Assert.ArgumentNotNull(texture, nameof(texture));

			var sprite = allocator.Allocate<SpriteNode>();
			sprite.Texture = texture;
			sprite.Color = color;
			sprite.Layer = layer;
			sprite.AttachTo(parent);

			return sprite;
		}
	}
}