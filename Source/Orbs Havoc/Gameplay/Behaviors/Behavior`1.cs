namespace OrbsHavoc.Gameplay.Behaviors
{
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Represents a behavior that can be attached to a scene node of the given type or of a derived type.
	/// </summary>
	/// <typeparam name="T">The base type of the scene nodes the behavior can be attached to.</typeparam>
	public abstract class Behavior<T> : Behavior
		where T : SceneNode
	{
		/// <summary>
		///   Gets the scene node the behavior is attached to.
		/// </summary>
		public T SceneNode { get; private set; }

		/// <summary>
		///   Gets the scene node the behavior is attached to.
		/// </summary>
		public sealed override SceneNode GetSceneNode() => SceneNode;

		/// <summary>
		///   Attaches the behavior to the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node the behavior should be attached to.</param>
		public sealed override void Attach(SceneNode sceneNode)
		{
			Assert.ArgumentNotNull(sceneNode, nameof(sceneNode));
			Assert.ArgumentOfType<T>(sceneNode, nameof(sceneNode));
			Assert.NotPooled(this);
			Assert.IsNull(SceneNode, "The behavior has already been attached to a scene node.");

			if (sceneNode.Behavior != null)
				sceneNode.Behavior.Previous = this;

			SceneNode = (T)sceneNode;
			Next = sceneNode.Behavior;
			sceneNode.Behavior = this;

			OnAttached();
		}

		/// <summary>
		///   Detaches the behavior from the scene node it is attached to.
		/// </summary>
		public sealed override void Detach()
		{
			Assert.NotPooled(this);
			Assert.NotNull(SceneNode, "The behavior is not attached to any scene node.");

			OnDetached();

			if (Previous != null)
				Previous.Next = Next;

			if (Next != null)
				Next.Previous = Previous;

			if (SceneNode.Behavior == this)
				SceneNode.Behavior = Next;

			Previous = null;
			Next = null;
			SceneNode = null;
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
		}

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected virtual void OnAttached()
		{
		}

		/// <summary>
		///   Invoked when the behavior is detached from the scene node it is attached to.
		/// </summary>
		/// <remarks>This method is not called when the scene graph is disposed.</remarks>
		protected virtual void OnDetached()
		{
		}

		/// <summary>
		///   Invoked when the pooled instance is returned to the pool.
		/// </summary>
		protected sealed override void OnReturning()
		{
			SceneNode = null;
			Previous = null;
			Next = null;
		}
	}
}