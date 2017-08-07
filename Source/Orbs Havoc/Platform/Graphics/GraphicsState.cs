namespace OrbsHavoc.Platform.Graphics
{
	using System.Collections.Generic;
	using System.Diagnostics;
	using Utilities;

	/// <summary>
	///   Represents the state of the graphics device.
	/// </summary>
	public sealed class GraphicsState
	{
		/// <summary>
		///   The maximum number of uniform buffers that can be bound simultaneously.
		/// </summary>
		public const int UniformBufferSlotCount = 14;

		/// <summary>
		///   The maximum number of constant buffers that can be bound simultaneously.
		/// </summary>
		public const int TextureSlotCount = 8;

		/// <summary>
		///   The maximum number of frames the GPU is allowed to be behind the CPU.
		/// </summary>
		public const int MaxFrameLag = 3;

		/// <summary>
		///   The uniform buffers that are currently bound.
		/// </summary>
		public readonly ConstantBuffer[] ConstantBuffers = new ConstantBuffer[UniformBufferSlotCount];

		/// <summary>
		///   The currently bound sampler states.
		/// </summary>
		public readonly SamplerState[] SamplerStates = new SamplerState[TextureSlotCount];

		/// <summary>
		///   The currently bound textures.
		/// </summary>
		public readonly Texture[] Textures = new Texture[TextureSlotCount];

		/// <summary>
		///   The currently active texture slot.
		/// </summary>
		public int ActiveTextureSlot = -1;

		/// <summary>
		///   The currently bound blend operation.
		/// </summary>
		public BlendOperation BlendOperation;

		/// <summary>
		///   Indicates whether drawing operations are currently allowed.
		/// </summary>
		public bool CanDraw;

		/// <summary>
		///   The monotonically increasing GPU frame number.
		/// </summary>
		public uint FrameNumber = 1;

		/// <summary>
		///   The currently bound render target.
		/// </summary>
		public RenderTarget RenderTarget;

		/// <summary>
		///   The currently bound shader.
		/// </summary>
		public Shader Shader;

		/// <summary>
		///   The currently bound vertex layout.
		/// </summary>
		public VertexLayout VertexLayout;

		/// <summary>
		///   The currently bound viewport.
		/// </summary>
		public Rectangle Viewport;

		/// <summary>
		///   The currently bound window.
		/// </summary>
		public Window Window;

		/// <summary>
		///   In debug builds, validates the state of the graphics device before drawing.
		/// </summary>
		[Conditional("DEBUG")]
		internal void Validate()
		{
			Assert.That(CanDraw, "Drawing commands can only be issued between a call to BeginFrame() and EndFrame().");
			Assert.NotNull(RenderTarget);
			Assert.NotNull(Shader);
			Assert.NotNull(RenderTarget);
			Assert.InRange(BlendOperation);
			Assert.That(VertexLayout >= 0, "Invalid vertex layout.");
			Assert.That(Viewport.Size.Width * Viewport.Size.Height > 0, "Viewport has an area of 0.");
		}

		/// <summary>
		///   Changes the state, if the current state value and the new one differ. Returns false to indicate that a state change was
		///   not required.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be changed.</typeparam>
		/// <param name="stateValue">The current state value that will be updated, if necessary.</param>
		/// <param name="value">The new state value.</param>
		public static bool Change<T>(ref T stateValue, T value)
		{
			if (EqualityComparer<T>.Default.Equals(stateValue, value))
				return false;

			stateValue = value;
			return true;
		}

		/// <summary>
		///   Changes the state, if the current state value and the new one differ. Returns false to indicate that a state change was
		///   not required.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be changed.</typeparam>
		/// <param name="stateValues">The current state values that will be updated, if necessary.</param>
		/// <param name="index">The index of the state value that should be updated.</param>
		/// <param name="value">The new state value.</param>
		public static bool Change<T>(T[] stateValues, int index, T value)
			where T : class
		{
			Assert.ArgumentNotNull(stateValues, nameof(stateValues));
			Assert.ArgumentInRange(index, stateValues, nameof(stateValues));

			if (stateValues[index] == value)
				return false;

			stateValues[index] = value;
			return true;
		}

		/// <summary>
		///   Unsets the given state value if it matches the given value.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be unset.</typeparam>
		/// <param name="stateValue">The current state value that will be unset, if necessary.</param>
		/// <param name="value">The state value that should be unset.</param>
		public static void Unset<T>(ref T stateValue, T value)
			where T : class
		{
			if (stateValue == value)
				stateValue = null;
		}

		/// <summary>
		///   Unsets the given state value if it matches the given value.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be unset.</typeparam>
		/// <param name="stateValues">The current state values that will be unset, if necessary.</param>
		/// <param name="value">The state value that should be unset.</param>
		public static void Unset<T>(T[] stateValues, T value)
			where T : class
		{
			for (var i = 0; i < stateValues.Length; ++i)
			{
				if (stateValues[i] == value)
					stateValues[i] = null;
			}
		}
	}
}