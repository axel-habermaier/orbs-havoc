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
	///   Represents an enumerator that can be used to traverse the scene graph in pre-order, that is, the parent node first,
	///   followed by the first child subtree, followed by the second child subtree, etc.
	/// </summary>
	public struct PreOrderEnumerator : IDisposable
	{
		/// <summary>
		///   The scene graph that is being enumerated.
		/// </summary>
		private readonly SceneGraph _sceneGraph;

		/// <summary>
		///   The node where the enumeration should start.
		/// </summary>
		private readonly SceneNode _startNode;

		/// <summary>
		///   The version of the scene graph when the enumerator was created.
		/// </summary>
		private readonly int _version;

		/// <summary>
		///   The next scene node that is enumerated.
		/// </summary>
		private SceneNode _next;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="sceneGraph">The scene graph that should be enumerated.</param>
		/// <param name="startNode">The node where the enumeration should start. Defaults to the root node.</param>
		public PreOrderEnumerator(SceneGraph sceneGraph, SceneNode startNode = null)
			: this()
		{
			Assert.ArgumentNotNull(sceneGraph, nameof(sceneGraph));

			_startNode = startNode ?? sceneGraph.Root;
			_version = sceneGraph.Version;
			_sceneGraph = sceneGraph;
			++_sceneGraph.EnumeratorCount;
		}

		/// <summary>
		///   Gets the scene node at the current position of the enumerator.
		/// </summary>
		public SceneNode Current { get; private set; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			--_sceneGraph.EnumeratorCount;
			_sceneGraph.ApplyDeferredUpdates();
		}

		/// <summary>
		///   Advances the enumerator to the next item.
		/// </summary>
		public bool MoveNext()
		{
			Assert.That(_version == _sceneGraph.Version, "The scene graph has been modified while it was being enumerated.");
			Assert.NotDisposed(_sceneGraph);

			// Special case for the root node that gets the enumeration started
			if (_next == null)
			{
				_next = _startNode;
				Current = _next;
				return true;
			}

			if (Current == null)
				return false;

			if (Current.Child != null)
			{
				Current = Current.Child;
				return true;
			}

			if (Current == _startNode)
				return false;

			if (Current.NextSibling != null)
			{
				Current = Current.NextSibling;
				return true;
			}

			while (Current != null && Current != _startNode && Current.NextSibling == null)
				Current = Current.Parent;

			if (Current == null || Current == _startNode)
				return false;

			Current = Current.NextSibling;
			return true;
		}

		/// <summary>
		///   Enables C#'s foreach support for the enumerator.
		/// </summary>
		public PreOrderEnumerator GetEnumerator()
		{
			return this;
		}
	}
}