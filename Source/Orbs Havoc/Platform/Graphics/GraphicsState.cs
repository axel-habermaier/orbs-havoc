namespace OrbsHavoc.Platform.Graphics
{
	using System.Diagnostics;
	using Utilities;

	/// <summary>
	///   Represents the state of the graphics device.
	/// </summary>
	internal sealed class GraphicsState
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
		public readonly Buffer[] UniformBuffers = new Buffer[UniformBufferSlotCount];

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
	}
}