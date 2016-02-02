// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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