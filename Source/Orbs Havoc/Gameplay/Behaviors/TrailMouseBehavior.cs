namespace OrbsHavoc.Gameplay.Behaviors
{
	using UserInterface.Input;
	using Platform.Memory;
	using Rendering;
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Trails the position of a mouse cursor.
	/// </summary>
	internal class TrailMouseBehavior : Behavior<SceneNode>
	{
		private Camera _camera;
		private Mouse _mouse;

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			SceneNode.Position = _mouse.Position - _camera.Position;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="mouse">The mouse that should be trailed.</param>
		/// <param name="camera">The camera that determines the view of the world.</param>
		public static TrailMouseBehavior Create(PoolAllocator allocator, Mouse mouse, Camera camera)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNull(mouse, nameof(mouse));

			var behavior = allocator.Allocate<TrailMouseBehavior>();
			behavior._mouse = mouse;
			behavior._camera = camera;
			return behavior;
		}
	}
}