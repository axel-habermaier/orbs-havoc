namespace OrbsHavoc.Gameplay.Behaviors
{
	using Platform.Memory;
	using SceneNodes;

	/// <summary>
	///   Represents a behavior that can be attached to a scene node.
	/// </summary>
	public abstract class Behavior : PooledObject
	{
		/// <summary>
		///   Gets or sets the next behavior in an intrusive list.
		/// </summary>
		public Behavior Next { get; set; }

		/// <summary>
		///   Gets or sets the previous behavior in an intrusive list.
		/// </summary>
		public Behavior Previous { get; set; }

		/// <summary>
		///   Gets the scene node the behavior is attached to.
		/// </summary>
		public abstract SceneNode GetSceneNode();

		/// <summary>
		///   Attaches the behavior to the given scene node.
		/// </summary>
		/// <param name="sceneNode">The scene node the behavior should be attached to.</param>
		public abstract void Attach(SceneNode sceneNode);

		/// <summary>
		///   Detaches the behavior from the scene node it is attached to.
		/// </summary>
		public abstract void Detach();

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public abstract void Execute(float elapsedSeconds);
	}
}