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

namespace OrbsHavoc.Gameplay
{
	using System;
	using System.Collections.Generic;
	using Behaviors;
	using Platform.Memory;
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Arranges the logical and spacial representation of a graphical 3D scene.
	/// </summary>
	public class SceneGraph : DisposableObject
	{
		/// <summary>
		///   The updates that should be applied to the scene graph.
		/// </summary>
		private readonly Queue<UpdateInfo> _deferredUpdates = new Queue<UpdateInfo>();

		/// <summary>
		///   The number of enumerators that are currently enumerating the scene graph.
		/// </summary>
		private int _enumeratorCount;

		/// <summary>
		///   The scene graph version, used to detect modifications during enumerations.
		/// </summary>
		private int _version;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocated pooled objects.</param>
		public SceneGraph(PoolAllocator allocator)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			Root = RootNode.Create(allocator);
			Root.Attach(this, null);
		}

		/// <summary>
		///   Gets a value indicating whether the scene graph is currently being enumerated.
		/// </summary>
		private bool IsEnumerated => _enumeratorCount > 0;

		/// <summary>
		///   Gets the root node of the scene graph.
		/// </summary>
		public SceneNode Root { get; }

		/// <summary>
		///   Raised when a scene node has been added to the scene graph.
		/// </summary>
		public event Action<SceneNode> NodeAdded;

		/// <summary>
		///   Raised when a scene node has been removed from the scene graph.
		/// </summary>
		public event Action<SceneNode> NodeRemoved;

		/// <summary>
		///   Adds the given scene node to the scene graph's root.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be added.</param>
		public void Add(SceneNode sceneNode)
		{
			Add(sceneNode, Root);
		}

		/// <summary>
		///   Adds the given scene node to the given parent node.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be added.</param>
		/// <param name="parentNode">The parent the scene node should be added to.</param>
		public void Add(SceneNode sceneNode, SceneNode parentNode)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(sceneNode, nameof(sceneNode));
			Assert.ArgumentNotNull(parentNode, nameof(parentNode));
			Assert.NotPooled(sceneNode);
			Assert.NotPooled(parentNode);
			Assert.ArgumentSatisfies(sceneNode.SceneGraph == null, nameof(sceneNode), "The scene node already belongs to a scene graph.");
			Assert.ArgumentSatisfies(!sceneNode.IsRemoved, nameof(sceneNode), "The scene node has already been removed and cannot be re-added.");
			Assert.ArgumentSatisfies(sceneNode != parentNode, nameof(sceneNode), "The scene node cannot be its own parent.");
			Assert.ArgumentSatisfies(parentNode.SceneGraph == this, nameof(parentNode), "The parent does not belong to this scene graph.");

			sceneNode.SceneGraph = this;

			if (IsEnumerated)
				_deferredUpdates.Enqueue(new UpdateInfo { UpdateType = UpdateType.AddNode, SceneNode = sceneNode, ParentNode = parentNode });
			else
				AddImmediately(sceneNode, parentNode);
		}

		/// <summary>
		///   Removes the given scene node from the scene graph.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be removed.</param>
		public void Remove(SceneNode sceneNode)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(sceneNode, nameof(sceneNode));
			Assert.NotPooled(sceneNode);
			Assert.ArgumentSatisfies(sceneNode.Parent != null, nameof(sceneNode), "Cannot remove the root node.");
			Assert.ArgumentSatisfies(sceneNode.SceneGraph == this, nameof(sceneNode), "The scene node does not belong to this scene graph.");

			if (sceneNode.IsRemoved)
				return;

			foreach (var node in EnumeratePostOrder(sceneNode))
			{
				if (node.IsRemoved)
					continue;

				node.IsRemoved = true;

				if (IsEnumerated)
					_deferredUpdates.Enqueue(new UpdateInfo { UpdateType = UpdateType.RemoveNode, SceneNode = node });
				else
					RemoveImmediately(node);
			}
		}

		/// <summary>
		///   Changes the parent node of the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be attached to the new parent.</param>
		/// <param name="parentNode">The new parent of the scene node.</param>
		public void Reparent(SceneNode sceneNode, SceneNode parentNode)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(sceneNode, nameof(sceneNode));
			Assert.ArgumentNotNull(parentNode, nameof(parentNode));
			Assert.ArgumentSatisfies(!sceneNode.IsRemoved, nameof(sceneNode), "The scene node has already been removed.");
			Assert.ArgumentSatisfies(!parentNode.IsRemoved, nameof(parentNode), "The parent has already been removed.");
			Assert.ArgumentSatisfies(sceneNode.SceneGraph == this, nameof(sceneNode), "The scene node belongs to a different scene graph.");
			Assert.ArgumentSatisfies(parentNode.SceneGraph == this, nameof(parentNode), "The parent belongs to a different scene graph.");
			Assert.ArgumentSatisfies(sceneNode.Parent != null, nameof(sceneNode), "Cannot change the parent of the root node.");
			Assert.ArgumentSatisfies(sceneNode != parentNode, nameof(sceneNode), "The scene node cannot be its own parent.");

			if (IsEnumerated)
				_deferredUpdates.Enqueue(new UpdateInfo { UpdateType = UpdateType.ReparentNode, SceneNode = sceneNode, ParentNode = parentNode });
			else
				ReparentImmediately(sceneNode, parentNode);
		}

		/// <summary>
		///   Executes the behaviors of all scene nodes in the scene graph.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behaviors.</param>
		public void ExecuteBehaviors(float elapsedSeconds)
		{
			Assert.NotDisposed(this);

			foreach (var sceneNode in EnumeratePreOrder())
			{
				foreach (var behavior in sceneNode.Behaviors)
					behavior.Execute(elapsedSeconds);
			}
		}

		/// <summary>
		///   Enumerates all scene nodes of the scene graph in pre-order, that is, the parent node first,
		///   followed by the first child subtree, followed by the second child subtree, etc.
		/// </summary>
		/// <param name="startNode">The scene node where the enumeration should start. Defaults to the root node.</param>
		public PreOrderEnumerator<SceneNode> EnumeratePreOrder(SceneNode startNode = null)
		{
			return new PreOrderEnumerator<SceneNode>(this, startNode);
		}

		/// <summary>
		///   Enumerates all scene nodes of the scene graph in pre-order, that is, the first child subtree first, followed by the
		///   second child subtree, ..., and the parent node last.
		/// </summary>
		/// <param name="startNode">The scene node where the enumeration should start. Defaults to the root node.</param>
		public PostOrderEnumerator<SceneNode> EnumeratePostOrder(SceneNode startNode = null)
		{
			return new PostOrderEnumerator<SceneNode>(this, startNode);
		}

		/// <summary>
		///   Enumerates all scene nodes of the scene graph in pre-order, that is, the parent node first,
		///   followed by the first child subtree, followed by the second child subtree, etc. Only scene nodes of the given type are
		///   enumerated.
		/// </summary>
		/// <typeparam name="T">The type of the scene nodes that should be enumerated.</typeparam>
		/// <param name="startNode">The scene node where the enumeration should start. Defaults to the root node.</param>
		public PreOrderEnumerator<T> EnumeratePreOrder<T>(SceneNode startNode = null)
			where T : SceneNode
		{
			return new PreOrderEnumerator<T>(this, startNode);
		}

		/// <summary>
		///   Enumerates all scene nodes of the scene graph in pre-order, that is, the first child subtree first, followed by the
		///   second child subtree, ..., and the parent node last. Only scene nodes of the given type are enumerated.
		/// </summary>
		/// <typeparam name="T">The type of the scene nodes that should be enumerated.</typeparam>
		/// <param name="startNode">The scene node where the enumeration should start. Defaults to the root node.</param>
		public PostOrderEnumerator<T> EnumeratePostOrder<T>(SceneNode startNode = null)
			where T : SceneNode
		{
			return new PostOrderEnumerator<T>(this, startNode);
		}

		/// <summary>
		///   Adds the given behavior to the scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node the behavior should be added to.</param>
		/// <param name="behavior">The behavior that should be attached.</param>
		internal void AddBehavior(SceneNode sceneNode, Behavior behavior)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(sceneNode, nameof(sceneNode));
			Assert.ArgumentNotNull(behavior, nameof(behavior));
			Assert.ArgumentSatisfies(behavior.GetSceneNode() == null, nameof(behavior), "The behavior is already attached to a scene node.");

			if (IsEnumerated)
				_deferredUpdates.Enqueue(new UpdateInfo { UpdateType = UpdateType.AddBehavior, SceneNode = sceneNode, Behavior = behavior });
			else
				AddBehaviorImmediately(sceneNode, behavior);
		}

		/// <summary>
		///   Detaches the given behavior from the scene node.
		/// </summary>
		/// <param name="behavior">The behavior that should be detached.</param>
		internal void RemoveBehavior(Behavior behavior)
		{
			Assert.NotDisposed(this);
			Assert.ArgumentNotNull(behavior, nameof(behavior));

			if (IsEnumerated)
				_deferredUpdates.Enqueue(new UpdateInfo { UpdateType = UpdateType.RemoveBehavior, Behavior = behavior });
			else
				RemoveBehaviorImmediately(behavior);
		}

		/// <summary>
		///   Immediately adds the given scene node to the given parent node.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be added.</param>
		/// <param name="parentNode">The parent the scene node should be added to.</param>
		private void AddImmediately(SceneNode sceneNode, SceneNode parentNode)
		{
			if (IsDisposing)
			{
				sceneNode.SafeDispose();
				return;
			}

			sceneNode.Attach(this, parentNode);
			NodeAdded?.Invoke(sceneNode);
			++_version;
		}

		/// <summary>
		///   Immediately removes the given scene node from the scene graph.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be removed.</param>
		private void RemoveImmediately(SceneNode sceneNode)
		{
			foreach (var behavior in sceneNode.Behaviors)
			{
				behavior.Detach();
				behavior.SafeDispose();
			}

			if (!IsDisposing)
				NodeRemoved?.Invoke(sceneNode);

			sceneNode.Detach();

			++_version;
			sceneNode.SafeDispose();
		}

		/// <summary>
		///   Immediately changes the parent node of the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node that should be attached to the new parent.</param>
		/// <param name="parentNode">The new parent of the scene node.</param>
		private void ReparentImmediately(SceneNode sceneNode, SceneNode parentNode)
		{
			if (IsDisposing)
				return;

			sceneNode.Detach();
			sceneNode.Attach(this, parentNode);
			++_version;
		}

		/// <summary>
		///   Immediately adds the given behavior to the scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node the behavior should be added to.</param>
		/// <param name="behavior">The behavior that should be attached.</param>
		private void AddBehaviorImmediately(SceneNode sceneNode, Behavior behavior)
		{
			if (IsDisposing)
			{
				behavior.SafeDispose();
				return;
			}

			behavior.Attach(sceneNode);
			++_version;
		}

		/// <summary>
		///   Immediately detaches the given behavior from the scene node.
		/// </summary>
		/// <param name="behavior">The behavior that should be detached.</param>
		private void RemoveBehaviorImmediately(Behavior behavior)
		{
			behavior.Detach();
			++_version;
		}

		/// <summary>
		///   Applies all deferred scene graph updates.
		/// </summary>
		public void Update()
		{
			Assert.NotDisposed(this);
			Assert.That(!IsEnumerated, "Cannot update the scene graph while it is being enumerated.");

			while (_deferredUpdates.Count > 0)
			{
				var update = _deferredUpdates.Dequeue();
				switch (update.UpdateType)
				{
					case UpdateType.AddNode:
						AddImmediately(update.SceneNode, update.ParentNode);
						break;
					case UpdateType.RemoveNode:
						RemoveImmediately(update.SceneNode);
						break;
					case UpdateType.ReparentNode:
						ReparentImmediately(update.SceneNode, update.ParentNode);
						break;
					case UpdateType.AddBehavior:
						AddBehaviorImmediately(update.SceneNode, update.Behavior);
						break;
					case UpdateType.RemoveBehavior:
						RemoveBehaviorImmediately(update.Behavior);
						break;
					default:
						Assert.NotReached("Unknown update type.");
						break;
				}
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Root.SafeDispose();
			Update();
		}

		/// <summary>
		///   Represents an update of a scene graph.
		/// </summary>
		private struct UpdateInfo
		{
			/// <summary>
			///   The behavior that should be updated.
			/// </summary>
			public Behavior Behavior;

			/// <summary>
			///   The parent scene node.
			/// </summary>
			public SceneNode ParentNode;

			/// <summary>
			///   The scene node that should be updated.
			/// </summary>
			public SceneNode SceneNode;

			/// <summary>
			///   Indicates the type of the update that should be applied to a scene graph.
			/// </summary>
			public UpdateType UpdateType;
		}

		/// <summary>
		///   Indicates the type of update that should be applied to a scene graph.
		/// </summary>
		private enum UpdateType
		{
			/// <summary>
			///   Indicates that a scene node should be added.
			/// </summary>
			AddNode,

			/// <summary>
			///   Indicates that a scene node should be removed.
			/// </summary>
			RemoveNode,

			/// <summary>
			///   Indicates that the parent of a scene node should be changed.
			/// </summary>
			ReparentNode,

			/// <summary>
			///   Indicates that a behavior should be added to a scene node.
			/// </summary>
			AddBehavior,

			/// <summary>
			///   Indicates that a behavior should be removed from a scene node.
			/// </summary>
			RemoveBehavior
		}

		/// <summary>
		///   Represents an enumerator that can be used to traverse the scene graph in post-order, that is,
		///   the first child subtree first, followed by the second child subtree, ..., and the parent node last.
		///   Only nodes of the given type are enumerated.
		/// </summary>
		/// <typeparam name="T">The type of the scene nodes that should be enumerated.</typeparam>
		public struct PostOrderEnumerator<T> : IDisposable
			where T : SceneNode
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
			///   The untyped scene node at the current position of the enumerator.
			/// </summary>
			private SceneNode _current;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="sceneGraph">The scene graph that should be enumerated.</param>
			/// <param name="startNode">The node where the enumeration should start. Defaults to the root node.</param>
			public PostOrderEnumerator(SceneGraph sceneGraph, SceneNode startNode = null)
				: this()
			{
				Assert.ArgumentNotNull(sceneGraph, nameof(sceneGraph));

				_startNode = startNode ?? sceneGraph.Root;
				_version = sceneGraph._version;
				_sceneGraph = sceneGraph;
				++_sceneGraph._enumeratorCount;
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
				--_sceneGraph._enumeratorCount;
			}

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			private bool Next()
			{
				Assert.That(_version == _sceneGraph._version, "The scene graph has been modified while it was being enumerated.");
				Assert.NotDisposed(_sceneGraph);

				// Special case for the first node
				if (_next == null)
				{
					_next = GetLeftMostChild(_startNode);
					_current = _next;
					return true;
				}

				while (_current != _startNode)
				{
					if (_current.NextSibling != null)
					{
						_current = GetLeftMostChild(_current.NextSibling);
						return true;
					}

					if (_current.Parent == null)
						break;

					_current = _current.Parent;
					return true;
				}

				return false;
			}

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			public bool MoveNext()
			{
				while (Next())
				{
					var typedNode = _current as T;
					if (typedNode == null)
						continue;

					Current = typedNode;
					return true;
				}

				return false;
			}

			/// <summary>
			///   Gets the left-most child of the given scene node.
			/// </summary>
			/// <param name="sceneNode">The scene node the left-most child should be returned for.</param>
			private static SceneNode GetLeftMostChild(SceneNode sceneNode)
			{
				while (sceneNode.Child != null)
					sceneNode = sceneNode.Child;

				return sceneNode;
			}

			/// <summary>
			///   Enables C#'s foreach support for the enumerator.
			/// </summary>
			public PostOrderEnumerator<T> GetEnumerator()
			{
				return this;
			}
		}

		/// <summary>
		///   Represents an enumerator that can be used to traverse the scene graph in pre-order, that is, the parent node first,
		///   followed by the first child subtree, followed by the second child subtree, etc. Only nodes of the given type are
		///   enumerated.
		/// </summary>
		/// <typeparam name="T">The type of the scene nodes that should be enumerated.</typeparam>
		public struct PreOrderEnumerator<T> : IDisposable
			where T : SceneNode
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
			///   The untyped scene node at the current position of the enumerator.
			/// </summary>
			private SceneNode _current;

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
				_version = sceneGraph._version;
				_sceneGraph = sceneGraph;
				++_sceneGraph._enumeratorCount;
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
				--_sceneGraph._enumeratorCount;
			}

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			private bool Next()
			{
				Assert.That(_version == _sceneGraph._version, "The scene graph has been modified while it was being enumerated.");
				Assert.NotDisposed(_sceneGraph);

				// Special case for the root node that gets the enumeration started
				if (_next == null)
				{
					_next = _startNode;
					_current = _next;
					return true;
				}

				if (_current == null)
					return false;

				if (_current.Child != null)
				{
					_current = _current.Child;
					return true;
				}

				if (_current == _startNode)
					return false;

				if (_current.NextSibling != null)
				{
					_current = _current.NextSibling;
					return true;
				}

				while (_current != null && _current != _startNode && _current.NextSibling == null)
					_current = _current.Parent;

				if (_current == null || _current == _startNode)
					return false;

				_current = _current.NextSibling;
				return true;
			}

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			public bool MoveNext()
			{
				while (Next())
				{
					var typedNode = _current as T;
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
			public PreOrderEnumerator<T> GetEnumerator()
			{
				return this;
			}
		}
	}
}