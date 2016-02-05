// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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

namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using System.Text;
	using Logging;
	using Memory;
	using Utilities;
	using static GraphicsHelpers;
	using static OpenGL3;

	/// <summary>
	///   Represents a combination of vertex and fragment shaders that control the various pipeline stages of the GPU.
	/// </summary>
	public sealed unsafe class Shader : DisposableObject
	{
		private const int LogBufferLength = 4096;
		private int _fragmentShader;
		private int _program;
		private int _vertexShader;

		/// <summary>
		///   Binds the shader for rendering.
		/// </summary>
		public void Bind()
		{
			Assert.That(_program != 0, "The shader has not been initialized.");

			if (Change(ref State.Shader, this))
				glUseProgram(_program);

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
			glDeleteProgram(_program);
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

			_program = glCreateProgram();
			if (_program == 0)
				Log.Die("Failed to create OpenGL program object.");

			glAttachShader(_program, _vertexShader);
			glAttachShader(_program, _fragmentShader);
			glLinkProgram(_program);
			CheckErrors();

			int success, logLength;
			byte* log = stackalloc byte[LogBufferLength];

			glGetProgramiv(_program, GL_LINK_STATUS, &success);
			glGetProgramInfoLog(_program, LogBufferLength, &logLength, log);
			CheckErrors();

			if (success == GL_FALSE)
				Log.Die("Program linking failed: {0}", new string((sbyte*)log, 0, logLength).Trim());

			if (logLength != 0)
				Log.Warn("{0}", new string((sbyte*)log, 0, logLength).Trim());

			Bind(ref buffer, name => glGetUniformLocation(_program, name.Pointer), glUniform1i);
			Bind(ref buffer, name => glGetUniformBlockIndex(_program, name.Pointer),
				(index, binding) => glUniformBlockBinding(_program, index, binding));
		}

		/// <summary>
		///   Establishes sampler and uniform block bindings.
		/// </summary>
		private void Bind(ref BufferReader buffer, Func<BufferPointer, int> getIndex, Action<int, int> setBinding)
		{
			var count = buffer.ReadInt32();
			for (var i = 0; i < count; ++i)
			{
				var nameLength = buffer.ReadInt32();
				using (var name = buffer.Pointer)
				{
					var index = getIndex(name);
					CheckErrors();

					buffer.Skip(nameLength);
					var binding = buffer.ReadInt32();

					if (index == -1)
						continue;

					Bind();
					setBinding(index, binding);
					CheckErrors();
				}
			}
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