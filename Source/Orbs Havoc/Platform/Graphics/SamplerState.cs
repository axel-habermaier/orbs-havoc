namespace OrbsHavoc.Platform.Graphics
{
	using Memory;
	using static OpenGL3;

	/// <summary>
	///   Describes a sampler state of a shader pipeline stage.
	/// </summary>
	public sealed unsafe class SamplerState : DisposableObject
	{
		private readonly int _state;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private SamplerState(int filter, int addressMode)
		{
			_state = Allocate(glGenSamplers, nameof(SamplerState));

			glSamplerParameteri(_state, GL_TEXTURE_MIN_FILTER, filter);
			glSamplerParameteri(_state, GL_TEXTURE_MAG_FILTER, filter);
			glSamplerParameteri(_state, GL_TEXTURE_WRAP_S, addressMode);
			glSamplerParameteri(_state, GL_TEXTURE_WRAP_T, addressMode);
			glSamplerParameteri(_state, GL_TEXTURE_WRAP_R, addressMode);
		}

		/// <summary>
		///   Gets a sampler state with point-filtering and clamp address mode.
		/// </summary>
		public static SamplerState Point { get; private set; }

		/// <summary>
		///   Gets a sampler state with bilinear filtering and clamp address mode.
		/// </summary>
		public static SamplerState Bilinear { get; private set; }

		/// <summary>
		///   Initializes the sampler states.
		/// </summary>
		public static void Initialize()
		{
			Point = new SamplerState(GL_NEAREST, GL_CLAMP_TO_EDGE);
			Bilinear = new SamplerState(GL_LINEAR, GL_CLAMP_TO_EDGE);

			Bilinear.Bind(0);
		}

		/// <summary>
		///   Disposes the sampler states.
		/// </summary>
		public static void Dispose()
		{
			Point.SafeDispose();
			Bilinear.SafeDispose();
		}

		/// <summary>
		///   Binds the sampler state for rendering.
		/// </summary>
		public void Bind(int slot)
		{
			if (Change(State.SamplerStates, slot, this))
				glBindSampler(slot, _state);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(State.SamplerStates, this);
			Deallocate(glDeleteSamplers, _state);
		}
	}
}