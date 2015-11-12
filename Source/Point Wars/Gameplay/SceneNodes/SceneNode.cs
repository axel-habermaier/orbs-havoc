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
	using Behaviors;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Represents the base class for all scene nodes of a scene graph.
	/// </summary>
	public abstract class SceneNode : PooledObject
	{
		/// <summary>
		///   The local transformation matrix of the scene node, relative to the parent scene node.
		/// </summary>
		private Matrix3x2 _localMatrix = Matrix3x2.Identity;

		/// <summary>
		///   The scene node's rotation in radians, relative to the parent scene node.
		/// </summary>
		private float _orientation;

		/// <summary>
		///   The scene node's position, relative to the parent scene node.
		/// </summary>
		private Vector2 _position;

		/// <summary>
		///   The world transformation matrix of the scene node.
		/// </summary>
		private Matrix3x2 _worldMatrix = Matrix3x2.Identity;

		/// <summary>
		///   Gets the first child scene node.
		/// </summary>
		internal SceneNode Child { get; private set; }

		/// <summary>
		///   Gets the next sibling scene node.
		/// </summary>
		internal SceneNode NextSibling { get; private set; }

		/// <summary>
		///   Get the previous sibling scene node.
		/// </summary>
		internal SceneNode PreviousSibling { get; private set; }

		/// <summary>
		///   Gets the local transformation matrix of the scene node, relative to the parent scene node.
		/// </summary>
		public Matrix3x2 LocalMatrix
		{
			get
			{
				Assert.NotPooled(this);
				return _localMatrix;
			}
		}

		/// <summary>
		///   Gets the world transformation matrix of the scene node.
		/// </summary>
		public Matrix3x2 WorldMatrix
		{
			get
			{
				Assert.NotPooled(this);
				return _worldMatrix;
			}
		}

		/// <summary>
		///   Gets or sets the scene node's position, relative to the parent scene node.
		/// </summary>
		public Vector2 Position
		{
			get
			{
				Assert.NotPooled(this);
				return _position;
			}
			set
			{
				Assert.NotPooled(this);
				_position = value;
				UpdateMatrices();
			}
		}

		/// <summary>
		///   Gets or sets the scene node's orientation in radians, relative to the parent scene node.
		/// </summary>
		public float Orientation
		{
			get
			{
				Assert.NotPooled(this);
				return _orientation;
			}
			set
			{
				Assert.NotPooled(this);
				_orientation = value;
				UpdateMatrices();
			}
		}

		/// <summary>
		///   Gets the scene node's world position.
		/// </summary>
		public Vector2 WorldPosition => _worldMatrix.Translation;

		/// <summary>
		///   Gets the parent scene node or null if the scene node is the root of the scene graph.
		/// </summary>
		public SceneNode Parent { get; private set; }

		/// <summary>
		///   Gets or sets the head of the behavior list.
		/// </summary>
		internal Behavior Behavior { get; set; }

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all behaviors of the scene node.
		/// </summary>
		internal BehaviorEnumerator Behaviors
		{
			get
			{
				Assert.NotPooled(this);
				return new BehaviorEnumerator(Behavior);
			}
		}

		/// <summary>
		///   Gets an enumerator that can be used to enumerate all children of the scene node.
		/// </summary>
		internal SceneNodeEnumerator Children
		{
			get
			{
				Assert.NotPooled(this);
				return new SceneNodeEnumerator(Child);
			}
		}

		/// <summary>
		///   Gets the scene graph the scene node belongs to or null if the scene node does not belong to any scene graph.
		/// </summary>
		public SceneGraph SceneGraph { get; internal set; }

		/// <summary>
		///   Gets a value indicating whether the scene node has been removed from the scene graph it belonged to.
		/// </summary>
		public bool IsRemoved { get; internal set; }

		/// <summary>
		///   Gets the first behavior of the given type, or null if there is none.
		/// </summary>
		internal T GetBehavior<T>()
			where T : Behavior
		{
			for (var behavior = Behavior; behavior != null; behavior = behavior.Next)
			{
				var typedBehavior = behavior as T;
				if (typedBehavior != null)
					return typedBehavior;
			}

			return null;
		}

		/// <summary>
		///   Changes the local position and rotation of the scene node, relative to the parent scene node.
		/// </summary>
		/// <param name="position">The local position relative to the parent scene node's position.</param>
		/// <param name="orientation">The local orientation relative to the parent scene node's orientation.</param>
		public void ChangeLocalTransformation(Vector2 position, float orientation)
		{
			Assert.NotPooled(this);

			_position = position;
			_orientation = orientation;

			UpdateMatrices();
		}

		/// <summary>
		///   Updates the transformation matrices.
		/// </summary>
		private void UpdateMatrices()
		{
			_localMatrix = Matrix3x2.CreateRotation(-Orientation) * Matrix3x2.CreateTranslation(Position);

			var parentMatrix = Parent?.WorldMatrix ?? Matrix3x2.Identity;
			_worldMatrix = parentMatrix * LocalMatrix;

			foreach (var child in Children)
				child.UpdateMatrices();
		}

		/// <summary>
		///   Attaches the scene node to the given scene graph and parent node.
		/// </summary>
		/// <param name="sceneGraph">The scene graph the scene node should be attached to.</param>
		/// <param name="parent">The parent node the scene node should be attached to.</param>
		internal void Attach(SceneGraph sceneGraph, SceneNode parent)
		{
			Assert.ArgumentNotNull(sceneGraph, nameof(sceneGraph));
			Assert.NotPooled(this);

			SceneGraph = sceneGraph;

			if (parent != null)
			{
				NextSibling = parent.Child;

				if (parent.Child != null)
					parent.Child.PreviousSibling = this;

				NextSibling = parent.Child;
				Parent = parent;
				parent.Child = this;
			}

			UpdateMatrices();
			OnAttached();
		}

		/// <summary>
		///   Detaches the scene node from the scene graph and parent node it is currently attached to.
		/// </summary>
		internal void Detach()
		{
			Assert.NotPooled(this);

			OnDetached();

			if (Parent.Child == this)
				Parent.Child = NextSibling;

			if (NextSibling != null)
				NextSibling.PreviousSibling = PreviousSibling;

			if (PreviousSibling != null)
				PreviousSibling.NextSibling = NextSibling;

			NextSibling = null;
			PreviousSibling = null;
			Parent = null;
			SceneGraph = null;
		}

		/// <summary>
		///   Invoked when the scene node is attached to a parent scene node.
		/// </summary>
		protected virtual void OnAttached()
		{
		}

		/// <summary>
		///   Invoked when the scene node is detached from its scene graph.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected virtual void OnDetached()
		{
		}

		/// <summary>
		///   Adds the given behavior to the scene node.
		/// </summary>
		/// <param name="behavior">The behavior that should be attached.</param>
		public void AddBehavior(Behavior behavior)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(behavior, nameof(behavior));
			Assert.ArgumentSatisfies(behavior.GetSceneNode() == null, nameof(behavior), "The behavior is already attached to a scene node.");

			// If we're not attached to a scene graph yet, we can just add the behavior; otherwise, the scene
			// graph might have to defer the operation
			if (SceneGraph == null)
				behavior.Attach(this);
			else
				SceneGraph.AddBehavior(this, behavior);
		}

		/// <summary>
		///   Detaches the given behavior from the scene node.
		/// </summary>
		/// <param name="behavior">The behavior that should be detached.</param>
		public void RemoveBehavior(Behavior behavior)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(behavior, nameof(behavior));
			Assert.ArgumentSatisfies(behavior.GetSceneNode() == this, nameof(behavior), "The behavior is not attached to this scene node.");

			// If we're not attached to a scene graph yet, we can just add the behavior; otherwise, the scene
			// graph might have to defer the operation
			if (SceneGraph == null)
				behavior.Detach();
			else
				SceneGraph.RemoveBehavior(behavior);
		}

		/// <summary>
		///   Attaches the scene node to the given parent node.
		/// </summary>
		/// <param name="parent">The parent the scene node should be added to.</param>
		public void AttachTo(SceneNode parent)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(parent, nameof(parent));
			Assert.ArgumentSatisfies(parent.SceneGraph != null, nameof(parent), "The parent does not belong to a scene graph.");

			parent.SceneGraph.Add(this, parent);
		}

		/// <summary>
		///   Attaches the scene node to the given scene graph's root.
		/// </summary>
		/// <param name="sceneGraph">The scene graph the scene node should be added to.</param>
		public void AttachTo(SceneGraph sceneGraph)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(sceneGraph, nameof(sceneGraph));

			AttachTo(sceneGraph.Root);
		}

		/// <summary>
		///   Attaches the given scene node as a child of the current node.
		/// </summary>
		/// <param name="childNode">The child node that should be attached.</param>
		public void AttachChild(SceneNode childNode)
		{
			Assert.NotPooled(this);
			Assert.ArgumentNotNull(childNode, nameof(childNode));
			Assert.That(SceneGraph != null, "The scene node does not belong to a scene graph.");

			SceneGraph.Add(childNode, this);
		}

		/// <summary>
		///   Removes the scene node from the scene graph.
		/// </summary>
		public void Remove()
		{
			Assert.NotPooled(this);
			SceneGraph?.Remove(this);
		}

		/// <summary>
		///   Changes the parent node of the scene node.
		/// </summary>
		/// <param name="parent">The new parent of the scene node.</param>
		public void Reparent(SceneNode parent)
		{
			Assert.NotPooled(this);
			Assert.NotNull(SceneGraph, "The scene node does not belong to a scene graph.");

			SceneGraph.Reparent(this, parent);
		}

		/// <summary>
		///   Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected override void OnReturning()
		{
			foreach (var behavior in Behaviors)
			{
				behavior.Detach();
				behavior.SafeDispose();
			}

			foreach (var child in Children)
				child.Remove();

			Behavior = null;
			Parent = null;
			SceneGraph = null;
			IsRemoved = false;

			_localMatrix = Matrix3x2.Identity;
			_worldMatrix = Matrix3x2.Identity;
			_position = Vector2.Zero;
			_orientation = 0;

			Child = null;
			NextSibling = null;
			PreviousSibling = null;
		}

		/// <summary>
		///   Enumerates a list of behaviors.
		/// </summary>
		public struct BehaviorEnumerator
		{
			/// <summary>
			///   The remaining behaviors that have yet to be enumerated.
			/// </summary>
			private Behavior _behavior;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="behavior">The behaviors that should be enumerated.</param>
			public BehaviorEnumerator(Behavior behavior)
				: this()
			{
				_behavior = behavior;
			}

			/// <summary>
			///   Gets the behavior at the current position of the enumerator.
			/// </summary>
			public Behavior Current { get; private set; }

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			public bool MoveNext()
			{
				Current = _behavior;
				if (Current == null)
					return false;

				_behavior = _behavior.Next;
				return true;
			}

			/// <summary>
			///   Enables C#'s foreach support for the enumerator.
			/// </summary>
			public BehaviorEnumerator GetEnumerator()
			{
				return this;
			}
		}

		/// <summary>
		///   Enumerates a list of scene nodes.
		/// </summary>
		public struct SceneNodeEnumerator
		{
			/// <summary>
			///   The remaining scene nodes that have yet to be enumerated.
			/// </summary>
			private SceneNode _node;

			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="node">The scene nodes that should be enumerated.</param>
			public SceneNodeEnumerator(SceneNode node)
				: this()
			{
				_node = node;
			}

			/// <summary>
			///   Gets the scene node at the current position of the enumerator.
			/// </summary>
			public SceneNode Current { get; private set; }

			/// <summary>
			///   Advances the enumerator to the next item.
			/// </summary>
			public bool MoveNext()
			{
				Current = _node;
				if (Current == null)
					return false;

				_node = _node.NextSibling;
				return true;
			}

			/// <summary>
			///   Enables C#'s foreach support for the enumerator.
			/// </summary>
			public SceneNodeEnumerator GetEnumerator()
			{
				return this;
			}
		}
	}
}