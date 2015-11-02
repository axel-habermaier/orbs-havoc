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

namespace PointWars.Gameplay.Scene
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents a scene graph enumerator that enumerates all of the scene graph's scene nodes of the
	///   given type in post-order.
	/// </summary>
	/// <typeparam name="T">The type of the scene nodes that should be enumerated.</typeparam>
	public struct TypedPostOrderEnumerator<T> : IDisposable
		where T : SceneNode
	{
		/// <summary>
		///   The post-order enumerator that is used to enumerate the scene graph.
		/// </summary>
		private PostOrderEnumerator _enumerator;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="sceneGraph">The scene graph that should be enumerated.</param>
		/// <param name="startNode">The node where the enumeration should start. Defaults to the root node.</param>
		public TypedPostOrderEnumerator(SceneGraph sceneGraph, SceneNode startNode = null)
			: this()
		{
			Assert.ArgumentNotNull(sceneGraph, nameof(sceneGraph));
			_enumerator = new PostOrderEnumerator(sceneGraph, startNode);
		}

		/// <summary>
		///   Gets the scene node at the current position of the enumerator.
		/// </summary>
		public T Current { get; private set; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_enumerator.Dispose();
		}

		/// <summary>
		///   Advances the enumerator to the next item.
		/// </summary>
		public bool MoveNext()
		{
			while (_enumerator.MoveNext())
			{
				var typedNode = _enumerator.Current as T;
				if (typedNode == null)
					continue;

				Current = typedNode;
				return true;
			}

			return false;
		}

		/// <summary>
		///   Enables C#'s foreach support for the enumerator.
		/// </summary>
		public TypedPostOrderEnumerator<T> GetEnumerator()
		{
			return this;
		}
	}
}