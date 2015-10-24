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

namespace PointWars.Platform.Graphics
{
	using System.Text;
	using Logging;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a combination of vertex and fragment shaders that control the various pipeline stages of the GPU.
	/// </summary>
	public sealed unsafe class Shader : GraphicsObject
	{
		private const int LogBufferLength = 4096;
		private int _fragmentShader;
		private int _vertexShader;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		private Shader()
		{
		}

		/// <summary>
		///   Loads a shader from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the shader should be read from.</param>
		public static Shader Create(ref BufferReader buffer)
		{
			var shader = new Shader();
			shader.Load(ref buffer);
			return shader;
		}

		/// <summary>
		///   Binds the shader for rendering.
		/// </summary>
		public void Bind()
		{
			Assert.That(Handle != 0, "The shader has not been initialized.");

			if (Change(ref State.Shader, this))
				glUseProgram(Handle);

			CheckErrors();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.Shader, this);

			glDeleteShader(_vertexShader);
			glDeleteShader(_fragmentShader);
			glDeleteProgram(Handle);
		}

		/// <summary>
		///   Loads the shader from the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the shader should be read from.</param>
		public void Load(ref BufferReader buffer)
		{
			OnDisposing();

			_vertexShader = LoadShader(GL_VERTEX_SHADER, buffer.ReadString());
			_fragmentShader = LoadShader(GL_FRAGMENT_SHADER, buffer.ReadString());

			Handle = glCreateProgram();
			if (Handle == 0)
				Log.Die("Failed to create OpenGL program object.");

			glAttachShader(Handle, _vertexShader);
			glAttachShader(Handle, _fragmentShader);
			glLinkProgram(Handle);
			CheckErrors();

			int success, logLength;
			byte* log = stackalloc byte[LogBufferLength];

			glGetProgramiv(Handle, GL_LINK_STATUS, &success);
			glGetProgramInfoLog(Handle, LogBufferLength, &logLength, log);
			CheckErrors();

			if (success == GL_FALSE)
				Log.Die("Program linking failed: {0}", new string((sbyte*)log, 0, logLength).Trim());

			if (logLength != 0)
				Log.Warn("{0}", new string((sbyte*)log, 0, logLength).Trim());
		}

		/// <summary>
		///   Loads a shader of the given type.
		/// </summary>
		private static int LoadShader(int shaderType, string shaderCode)
		{
			var shader = glCreateShader(shaderType);
			if (shader == 0)
				Log.Die("Failed to create OpenGL shader object.");

			var code = Encoding.UTF8.GetBytes(shaderCode);
			var length = code.Length;

			fixed (byte* pinnedCode = code)
			{
				var source = pinnedCode;
				glShaderSource(shader, 1, &source, &length);
			}

			int success, logLength;
			byte* log = stackalloc byte[LogBufferLength];

			glCompileShader(shader);
			glGetShaderiv(shader, GL_COMPILE_STATUS, &success);
			glGetShaderInfoLog(shader, LogBufferLength, &logLength, log);
			CheckErrors();

			if (success == GL_FALSE)
				Log.Die("Shader compilation failed: {0}", new string((sbyte*)log, 0, logLength).Trim());

			if (logLength != 0)
				Log.Warn("{0}", new string((sbyte*)log, 0, logLength).Trim());

			return shader;
		}
	}
}