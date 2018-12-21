namespace OrbsHavoc.Rendering
{
	using System.Collections.Generic;
	using System.Numerics;
	using Platform;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Renders the application.
	/// </summary>
	internal sealed unsafe class Renderer : DisposableObject
	{
		private readonly PoolAllocator _allocator = new PoolAllocator();
		private readonly List<RenderOperation> _operations = new List<RenderOperation>();
		private readonly UniformBuffer<Matrix4x4> _projectionMatrixBuffer;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="graphicsDevice">The graphics device that should be used to draw the frame.</param>
		/// <param name="window">The window the renderer should belong to.</param>
		public Renderer(GraphicsDevice graphicsDevice, Window window)
		{
			Assert.ArgumentNotNull(graphicsDevice, nameof(graphicsDevice));
			Assert.ArgumentNotNull(window, nameof(window));

			GraphicsDevice = graphicsDevice;
			_projectionMatrixBuffer = new UniformBuffer<Matrix4x4>();

			Window = window;
			Window.Resized += UpdateProjectionMatrix;

			UpdateProjectionMatrix(Window.Size);

			var data = 0xFFFFFFFF;
			WhiteTexture = new Texture(new Size(1, 1), DataFormat.Rgba, &data);
		}

		/// <summary>
		///   Gets the graphics device used to draw the frame.
		/// </summary>
		public GraphicsDevice GraphicsDevice { get; }

		/// <summary>
		///   Gets the default camera that is used when no camera is set explicitly.
		/// </summary>
		public Camera DefaultCamera { get; } = new Camera();

		/// <summary>
		///   Gets a 1x1 pixels fully white texture.
		/// </summary>
		public Texture WhiteTexture { get; }

		/// <summary>
		///   Gets the window the renderer belongs to.
		/// </summary>
		public Window Window { get; }

		/// <summary>
		///   Gets the quads drawn by the renderer.
		/// </summary>
		public QuadCollection Quads { get; } = new QuadCollection();

		/// <summary>
		///   Gets the render buffer that represents the GPU memory holding the quads.
		/// </summary>
		internal RenderBuffer RenderBuffer { get; } = new RenderBuffer();

		/// <summary>
		///   Gets the number of vertices drawn in the previous frame.
		/// </summary>
		public int VertexCount { get; private set; }

		/// <summary>
		///   Gets the number of draw calls in the previous frame.
		/// </summary>
		public int DrawCalls { get; private set; }

		/// <summary>
		///   Renders the current frame by executing all registered render operations.
		/// </summary>
		internal void Render()
		{
			DrawCalls = 0;

			// Bind the projection matrix buffer and upload the quads to the GPU
			_projectionMatrixBuffer.Bind(0);
			Quads.UploadToGpu(RenderBuffer);

			// Now execute all render operations
			foreach (var operation in _operations)
				operation.Execute();

			// We're done with the quads, just forget about them
			VertexCount = Quads.Count * 2;
			Quads.Clear();

			// We're done with the operations; they'll be added back for the next frame
			_operations.SafeDisposeAll();
			_operations.Clear();
		}

		/// <summary>
		///   Draws primitiveCount-many primitives, starting at the given offset into the currently bound vertex buffers.
		/// </summary>
		/// <param name="renderTarget">The render target that should be drawn into.</param>
		/// <param name="vertexCount">The number of vertices that should be drawn.</param>
		/// <param name="vertexOffset">The offset into the vertex buffers.</param>
		/// <param name="primitiveType">The type of the primitives that should be drawn.</param>
		public void Draw(RenderTarget renderTarget, int vertexCount, int vertexOffset, PrimitiveType primitiveType)
		{
			++DrawCalls;
			renderTarget.Bind();

			GraphicsDevice.Draw(vertexCount, vertexOffset, primitiveType);
		}

		/// <summary>
		///   Draws indexCount-many indices, starting at the given index offset into the currently bound index buffer, where the
		///   vertex offset is added to each index before accessing the currently bound vertex buffers.
		/// </summary>
		/// <param name="renderTarget">The render target that should be drawn into.</param>
		/// <param name="indexCount">The number of indices to draw.</param>
		/// <param name="indexOffset">The location of the first index read by the GPU from the index buffer.</param>
		/// <param name="vertexOffset">The value that should be added to each index before reading a vertex from the vertex buffer.</param>
		/// <param name="primitiveType">The type of the primitives that should be drawn.</param>
		public void DrawIndexed(RenderTarget renderTarget, int indexCount, int indexOffset, int vertexOffset, PrimitiveType primitiveType)
		{
			++DrawCalls;
			renderTarget.Bind();

			GraphicsDevice.DrawIndexed(indexCount, indexOffset, vertexOffset, primitiveType);
		}

		/// <summary>
		///   Adds an operation to the renderer for the current frame.
		/// </summary>
		/// <typeparam name="T">The type of the render operation that should be added.</typeparam>
		public T AddOperation<T>()
			where T : RenderOperation, new()
		{
			var operation = _allocator.Allocate<T>();
			operation.Renderer = this;
			_operations.Add(operation);
			return operation;
		}

		/// <summary>
		///   Creates a sprite batch that efficiently draws a large number of sprites.
		/// </summary>
		/// <param name="renderTarget">The render target the sprites should be rendered to.</param>
		public SpriteBatch CreateSpriteBatch(RenderTarget renderTarget)
		{
			var spriteBatch = AddOperation<SpriteBatch>();
			spriteBatch.RenderState.RenderTarget = renderTarget;
			return spriteBatch;
		}

		/// <summary>
		///   Clears the given render target using the given color.
		/// </summary>
		/// <param name="renderTarget">The render target that should be cleared.</param>
		/// <param name="color">The color the render target should be cleared with.</param>
		public void ClearRenderTarget(RenderTarget renderTarget, Color color)
		{
			Assert.ArgumentNotNull(renderTarget, nameof(renderTarget));

			var operation = AddOperation<ClearRenderTarget>();
			operation.RenderTarget = renderTarget;
			operation.ClearColor = color;
		}

		/// <summary>
		///   Copies the given input render target, writing the result to the given output render target.
		/// </summary>
		/// <param name="input">The render target that should be copied.</param>
		/// <param name="output">The render target that should be rendered to.</param>
		public CopyEffect Copy(RenderTarget input, RenderTarget output)
		{
			var operation = AddOperation<CopyEffect>();
			operation.Input = input;
			operation.Output = output;
			return operation;
		}

		/// <summary>
		///   Blooms the given input render target, writing the result to the given output render target.
		/// </summary>
		/// <param name="input">The render target that should be bloomed.</param>
		/// <param name="output">The render target that should be rendered to.</param>
		public BloomEffect Bloom(RenderTarget input, RenderTarget output)
		{
			var operation = AddOperation<BloomEffect>();
			operation.Input = input;
			operation.Output = output;
			return operation;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_projectionMatrixBuffer.SafeDispose();
			_operations.SafeDisposeAll();
			_allocator.SafeDispose();

			Window.Resized -= UpdateProjectionMatrix;
			WhiteTexture.SafeDispose();
			RenderBuffer.SafeDispose();
			Quads.SafeDispose();
			DefaultCamera.SafeDispose();
		}

		/// <summary>
		///   Updates the sprite batch's projection matrix after a window size change.
		/// </summary>
		private void UpdateProjectionMatrix(Size size)
		{
			var matrix = Matrix4x4.CreateOrthographicOffCenter(0, size.Width, size.Height, 0, 0, 1);
			_projectionMatrixBuffer.Copy(&matrix);
		}
	}
}