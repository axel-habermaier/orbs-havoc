﻿namespace OrbsHavoc.Platform.Graphics
{
	using Logging;
	using Memory;
	using Utilities;
	using static OpenGL3;
	using static SDL2;

	/// <summary>
	///   Represents the GPU.
	/// </summary>
	internal sealed unsafe class GraphicsDevice : DisposableObject
	{
		private readonly int[] _beginQueries = new int[GraphicsState.MaxFrameLag];
		private readonly void* _context;
		private readonly void* _contextWindow;
		private readonly int[] _endQueries = new int[GraphicsState.MaxFrameLag];
		private readonly void*[] _syncQueries = new void*[GraphicsState.MaxFrameLag];
		private int _syncedIndex;

		/// <summary>
		///   Initializes the graphics device.
		/// </summary>
		public GraphicsDevice()
		{
			var title = stackalloc byte[] { 0 };
			_contextWindow = SDL_CreateWindow(title, 0, 0, 1, 1, SDL_WINDOW_HIDDEN | SDL_WINDOW_OPENGL);
			if (_contextWindow == null)
				Log.Die($"Failed to create the OpenGL context window: {SDL_GetError()}");

			_context = SDL_GL_CreateContext(_contextWindow);
			if (_context == null)
				Log.Die("Failed to initialize the OpenGL context. OpenGL 3.3 is not supported by your graphics card.");

			MakeCurrent();

			int major, minor;
			glGetIntegerv(GL_MAJOR_VERSION, &major);
			glGetIntegerv(GL_MINOR_VERSION, &minor);

			if (major < 3 || (major == 3 && minor < 3))
				Log.Die($"Only OpenGL {major}.{minor} seems to be supported. OpenGL 3.3 is required.");

			string getString(int option) => Interop.ToString(glGetString(option));
			Log.Info($"OpenGL renderer: {getString(GL_RENDERER)} ({getString(GL_VENDOR)})");
			Log.Info($"OpenGL version: {getString(GL_VERSION)}");
			Log.Info($"OpenGL GLSL version: {getString(GL_SHADING_LANGUAGE_VERSION)}");

			SamplerState.Initialize();
			glDisable(GL_DEPTH_TEST);
			glDisable(GL_CULL_FACE);
			glBlendEquation(GL_FUNC_ADD);

			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				_beginQueries[i] = Allocate(glGenQueries, "TimestampQuery");
				_endQueries[i] = Allocate(glGenQueries, "TimestampQuery");
				_syncQueries[i] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);

				glQueryCounter(_beginQueries[i], GL_TIMESTAMP);
				glQueryCounter(_endQueries[i], GL_TIMESTAMP);
			}
		}

		/// <summary>
		///   Gets the GPU frame time in milliseconds.
		/// </summary>
		public double FrameTime { get; private set; }

		/// <summary>
		///   Makes the OpenGL context for the given window the current one on the calling thread.
		/// </summary>
		public void MakeCurrent(Window window = null)
		{
			if (SDL_GL_MakeCurrent(window ?? _contextWindow, _context) != 0)
				Log.Die($"Failed to make OpenGL context current: {SDL_GetError()}");
		}

		/// <summary>
		///   Ensures that the CPU and GPU are synchronized, so that the actual frame lag is less
		///   than or equal to the maximum allowed one.
		/// </summary>
		internal void SyncWithCpu()
		{
			var isSynced = GL_UNSIGNALED;
			while (isSynced != GL_CONDITION_SATISFIED && isSynced != GL_ALREADY_SIGNALED)
				isSynced = glClientWaitSync(_syncQueries[_syncedIndex], GL_SYNC_FLUSH_COMMANDS_BIT, 0);
		}

		/// <summary>
		///   Marks the beginning of a frame, properly synchronizing the GPU and the CPU.
		/// </summary>
		public void BeginFrame()
		{
			// Get the GPU frame time for the frame that we just synced
			long begin, end;
			glGetQueryObjectui64v(_beginQueries[_syncedIndex], GL_QUERY_RESULT, &begin);
			glGetQueryObjectui64v(_endQueries[_syncedIndex], GL_QUERY_RESULT, &end);
			FrameTime = (end - begin) / 1000000.0;

			// Issue timing query for the current frame and allow drawing
			glQueryCounter(_beginQueries[_syncedIndex], GL_TIMESTAMP);
			State.CanDraw = true;
		}

		/// <summary>
		///   Marks the end of a frame, properly synchronizing the GPU and the CPU and updating the GPU frame time.
		/// </summary>
		public void EndFrame()
		{
			// Issue timing query to get frame end time
			glQueryCounter(_endQueries[_syncedIndex], GL_TIMESTAMP);

			// We've completed the frame, so issue the synced query for the current frame and update the synced index
			glDeleteSync(_syncQueries[_syncedIndex]);
			_syncQueries[_syncedIndex] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
			_syncedIndex = (_syncedIndex + 1) % GraphicsState.MaxFrameLag;

			// Drawing is no longer allowed, but all frame-dependent resources can now be updated again
			State.CanDraw = false;
			State.FrameNumber += 1;
		}

		/// <summary>
		///   Sets up and validates the required GPU state for a draw call.
		/// </summary>
		private static void BeforeDraw()
		{
			State.Validate();
			glBindVertexArray(State.VertexLayout);
		}

		/// <summary>
		///   Sets up and validates the GPU state after a draw call.
		/// </summary>
		private static void AfterDraw()
		{
			glBindVertexArray(0);
		}

		/// <summary>
		///   Draws primitiveCount-many primitives, starting at the given offset into the currently bound vertex buffers.
		/// </summary>
		/// <param name="vertexCount">The number of vertices that should be drawn.</param>
		/// <param name="vertexOffset">The offset into the vertex buffers.</param>
		/// <param name="primitiveType">The type of the primitives that should be drawn.</param>
		public void Draw(int vertexCount, int vertexOffset, PrimitiveType primitiveType)
		{
			Assert.ArgumentInRange(primitiveType, nameof(primitiveType));

			BeforeDraw();
			glDrawArrays((int)primitiveType, vertexOffset, vertexCount);
			AfterDraw();
		}

		/// <summary>
		///   Draws indexCount-many indices, starting at the given index offset into the currently bound index buffer, where the
		///   vertex offset is added to each index before accessing the currently bound vertex buffers.
		/// </summary>
		/// <param name="indexCount">The number of indices to draw.</param>
		/// <param name="indexOffset">The location of the first index read by the GPU from the index buffer.</param>
		/// <param name="vertexOffset">The value that should be added to each index before reading a vertex from the vertex buffer.</param>
		/// <param name="primitiveType">The type of the primitives that should be drawn.</param>
		public void DrawIndexed(int indexCount, int indexOffset, int vertexOffset, PrimitiveType primitiveType)
		{
			Assert.ArgumentInRange(primitiveType, nameof(primitiveType));

			BeforeDraw();
			glDrawElementsBaseVertex((int)primitiveType, indexCount, GL_UNSIGNED_INT, (void*)(indexOffset * sizeof(int)), vertexOffset);
			AfterDraw();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			SamplerState.Dispose();

			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				glDeleteSync(_syncQueries[i]);
				Deallocate(glDeleteQueries, _beginQueries[i]);
				Deallocate(glDeleteQueries, _endQueries[i]);
			}

			SDL_GL_DeleteContext(_context);
			SDL_DestroyWindow(_contextWindow);
		}

		/// <summary>
		///   Enables the scissor test.
		/// </summary>
		/// <param name="renderTarget">The render target the scissor test should be enabled for.</param>
		/// <param name="area">The area that should be drawn.</param>
		public static void EnableScissorTest(RenderTarget renderTarget, Rectangle area)
		{
			Assert.ArgumentNotNull(renderTarget, nameof(renderTarget));

			glEnable(GL_SCISSOR_TEST);
			glScissor(
				MathUtils.RoundIntegral(area.Left),
				MathUtils.RoundIntegral(renderTarget.Size.Height - area.Height - area.Top),
				MathUtils.RoundIntegral(area.Width),
				MathUtils.RoundIntegral(area.Height));
		}

		/// <summary>
		///   Disables the scissor test.
		/// </summary>
		public static void DisableScissorTest()
		{
			glDisable(GL_SCISSOR_TEST);
		}
	}
}