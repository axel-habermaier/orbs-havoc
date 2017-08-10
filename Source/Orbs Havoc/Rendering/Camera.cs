namespace OrbsHavoc.Rendering
{
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Memory;

	/// <summary>
	///   Represents a camera that can be used to draw scenes.
	/// </summary>
	internal sealed unsafe class Camera : DisposableObject
	{
		private readonly UniformBuffer _buffer = new UniformBuffer(sizeof(Vector2));
		private Vector2 _position;

		/// <summary>
		///   Gets or sets the camera's position within the scene.
		/// </summary>
		public Vector2 Position
		{
			get => _position;
			set
			{
				if (_position == value)
					return;

				_position = value;
				_buffer.Copy(&value);
			}
		}

		/// <summary>
		///   Activates this camera, causing all subsequent drawing operations to use this camera's matrices.
		/// </summary>
		internal void Bind()
		{
			_buffer.Bind(1);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_buffer.SafeDispose();
		}
	}
}