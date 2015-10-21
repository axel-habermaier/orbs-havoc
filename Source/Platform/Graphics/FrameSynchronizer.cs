﻿// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace PointWars.Platform.Graphics
{
	using Memory;
	using static OpenGL3;

	/// <summary>
	///   Synchronizes GPU and CPU.
	/// </summary>
	public sealed unsafe class FrameSynchronizer : DisposableObject
	{
		private readonly uint[] _beginQueries = new uint[GraphicsState.MaxFrameLag];
		private readonly uint[] _endQueries = new uint[GraphicsState.MaxFrameLag];
		private readonly void*[] _syncQueries = new void*[GraphicsState.MaxFrameLag];
		private uint _syncedIndex = 0;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public FrameSynchronizer()
		{
			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				_beginQueries[i] = GraphicsObject.Allocate(glGenQueries, "TimestampQuery");
				_endQueries[i] = GraphicsObject.Allocate(glGenQueries, "TimestampQuery");
				_syncQueries[i] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);

				glQueryCounter(_beginQueries[i], GL_TIMESTAMP);
				glQueryCounter(_endQueries[i], GL_TIMESTAMP);
			}
		}

		/// <summary>
		///   Gets the GPU frame time in seconds.
		/// </summary>
		public double FrameTime { get; private set; }

		/// <summary>
		///   Marks the beginning of a frame, properly synchronizing the GPU and the CPU.
		/// </summary>
		public void BeginFrame()
		{
			// Make sure the GPU is not more than FrameLag frames behind, so let's wait for the completion of the synced
			// query issued FrameLag frames ago
			uint isSynced;
			do
			{
				isSynced = glClientWaitSync(_syncQueries[_syncedIndex], GL_SYNC_FLUSH_COMMANDS_BIT, 0);
			} while (isSynced != GL_CONDITION_SATISFIED && isSynced != GL_ALREADY_SIGNALED);

			// Get the GPU frame time for the frame that we just synced
			ulong begin, end;
			glGetQueryObjectui64v(_beginQueries[_syncedIndex], GL_QUERY_RESULT, &begin);
			glGetQueryObjectui64v(_endQueries[_syncedIndex], GL_QUERY_RESULT, &end);
			FrameTime = (end - begin) / 1000000000000.0;

			// Issue timing query for the current frame and allow drawing
			glQueryCounter(_beginQueries[_syncedIndex], GL_TIMESTAMP);
			GraphicsObject.State.CanDraw = true;
		}

		/// <summary>
		///   Marks the end of a frame, properly synchronizing the GPU and the CPU and updating the GPU frame time.
		/// </summary>
		internal void EndFrame()
		{
			// Issue timing query to get frame end time
			glQueryCounter(_endQueries[_syncedIndex], GL_TIMESTAMP);

			// We've completed the frame, so issue the synced query for the current frame and update the synced index
			glDeleteSync(_syncQueries[_syncedIndex]);
			_syncQueries[_syncedIndex] = glFenceSync(GL_SYNC_GPU_COMMANDS_COMPLETE, 0);
			_syncedIndex = (_syncedIndex + 1) % GraphicsState.MaxFrameLag;

			// Drawing is no longer allowed
			GraphicsObject.State.CanDraw = false;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			for (var i = 0; i < GraphicsState.MaxFrameLag; ++i)
			{
				glDeleteSync(_syncQueries[i]);
				GraphicsObject.Deallocate(glDeleteQueries, _beginQueries[i]);
				GraphicsObject.Deallocate(glDeleteQueries, _endQueries[i]);
			}
		}
	}
}