namespace OrbsHavoc.Gameplay.Behaviors
{
	using System.Numerics;
	using Platform.Memory;
	using SceneNodes;
	using Utilities;

	/// <summary>
	///   Moves a scene node in a circle.
	/// </summary>
	public class CircleMovementBehavior : Behavior<SceneNode>
	{
		private Vector2 _position;
		private float _radius;
		private float _speed;
		private float _time;

		/// <summary>
		///   Invoked when the behavior is attached to a scene node.
		/// </summary>
		protected override void OnAttached()
		{
			_position = SceneNode.Position;
		}

		/// <summary>
		///   Invoked when the behavior should execute a step.
		/// </summary>
		/// <param name="elapsedSeconds">The elapsed time in seconds since the last execution of the behavior.</param>
		public override void Execute(float elapsedSeconds)
		{
			_time += elapsedSeconds;
			SceneNode.Position = _position + new Vector2(MathUtils.Sin(_time * _speed) * _radius, MathUtils.Cos(_time * _speed) * _radius);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate pooled objects.</param>
		/// <param name="speed">The movement speed.</param>
		/// <param name="radius">The circle of the radius.</param>
		public static CircleMovementBehavior Create(PoolAllocator allocator, float speed, float radius)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));

			var behavior = allocator.Allocate<CircleMovementBehavior>();
			behavior._speed = speed;
			behavior._radius = radius;
			behavior._time = RandomNumbers.NextSingle(0, MathUtils.TwoPi);
			return behavior;
		}
	}
}