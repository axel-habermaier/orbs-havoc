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
	using System.Numerics;
	using Platform.Memory;
	using Rendering;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Represents a node that emits and draws particles.
	/// </summary>
	public class ParticleEffectNode : SceneNode
	{
		private ParticleEffect _effect;

		/// <summary>
		///   Invoked when the scene node is detached from its scene graph.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected override void OnDetached()
		{
			_effect.SafeDispose();
		}

		/// <summary>
		///   Updates the particle effect.
		/// </summary>
		/// <param name="elapsedSeconds">The amount of seconds that has passed since the last update.</param>
		public void Update(float elapsedSeconds)
		{
			_effect.SetSpawnPosition(WorldPosition);
			_effect.Update(elapsedSeconds);

			if (_effect.IsCompleted)
				Remove();
		}

		/// <summary>
		///   Draws the sprite using the given renderer.
		/// </summary>
		/// <param name="renderer">The renderer that should be used for drawing.</param>
		public void Draw(Renderer renderer)
		{
			_effect.Draw(renderer);
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the sprite.</param>
		/// <param name="effectTemplate">The template of the particle effect that should be added to the scene graph.</param>
		/// <param name="position">The node's initial position.</param>
		public static ParticleEffectNode Create(PoolAllocator allocator, ParticleEffectTemplate effectTemplate, Vector2 position)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var sprite = allocator.Allocate<ParticleEffectNode>();
			sprite.Position = position;
			sprite._effect = effectTemplate.Allocate();

			return sprite;
		}
	}
}