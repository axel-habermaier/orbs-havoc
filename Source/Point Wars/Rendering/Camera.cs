// The MIT License (MIT)
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

namespace PointWars.Rendering
{
	using System.Numerics;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;
	using static Platform.Graphics.OpenGL3;
	using static Platform.Graphics.GraphicsHelpers;

	/// <summary>
	///   Represents a camera that can be used to draw scenes.
	/// </summary>
	public sealed unsafe class Camera : DisposableObject
	{
		private readonly DynamicBuffer _buffer = new DynamicBuffer(GL_UNIFORM_BUFFER, 1, sizeof(Vector2));
		private uint _lastChanged;
		private Vector2 _position;

		/// <summary>
		///   Gets or sets the camera's position within the scene.
		/// </summary>
		public Vector2 Position
		{
			get { return _position; }
			set
			{
				Assert.That(_lastChanged < State.FrameNumber, "The buffer cannot be changed multiple times per frame.");

				if (_position == value)
					return;

				_lastChanged = State.FrameNumber;
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