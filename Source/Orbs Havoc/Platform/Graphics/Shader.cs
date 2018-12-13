namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using System.Text;
	using Logging;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Represents a combination of vertex and fragment shaders that control the various pipeline stages of the GPU.
	/// </summary>
	internal sealed unsafe class Shader : DisposableObject
	{
		private const int LogBufferLength = 4096;
		private int _fragmentShader;
		private int _program;
		private int _vertexShader;
		private int _geometryShader;

		/// <summary>
		///   Binds the shader for rendering.
		/// </summary>
		public void Bind()
		{
			Assert.That(_program != 0, "The shader has not been initialized.");

			if (Change(ref State.Shader, this))
				glUseProgram(_program);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Unset(ref State.Shader, this);

			glDeleteShader(_vertexShader);
			glDeleteShader(_fragmentShader);
			glDeleteShader(_geometryShader);
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
			_geometryShader = LoadShader(GL_GEOMETRY_SHADER, buffer.ReadString());
			_fragmentShader = LoadShader(GL_FRAGMENT_SHADER, buffer.ReadString());

			_program = glCreateProgram();
			if (_program == 0)
				Log.Die("Failed to create OpenGL program object.");

			if (_geometryShader != 0)
				glAttachShader(_program, _geometryShader);

			glAttachShader(_program, _vertexShader);
			glAttachShader(_program, _fragmentShader);
			glLinkProgram(_program);

			int success, logLength;
			var log = stackalloc byte[LogBufferLength];

			glGetProgramiv(_program, GL_LINK_STATUS, &success);
			glGetProgramInfoLog(_program, LogBufferLength, &logLength, log);

			if (success == GL_FALSE)
				Log.Die($"Program linking failed: {Interop.ToString(log).Trim()}");

			if (logLength != 0)
				Log.Debug(Interop.ToString(log).Trim());

			Bind(ref buffer, name => glGetUniformLocation(_program, name), glUniform1i);
			Bind(ref buffer, name => glGetUniformBlockIndex(_program, name),
				(index, binding) => glUniformBlockBinding(_program, index, binding));
		}

		/// <summary>
		///   Establishes sampler and uniform block bindings.
		/// </summary>
		private void Bind(ref BufferReader buffer, Func<PinnedPointer, int> getIndex, Action<int, int> setBinding)
		{
			var count = buffer.ReadInt32();
			for (var i = 0; i < count; ++i)
			{
				var nameLength = buffer.ReadInt32();
				using (var name = buffer.Pointer)
				{
					var index = getIndex(name);

					buffer.Skip(nameLength);
					var binding = buffer.ReadInt32();

					if (index == -1)
						continue;

					Bind();
					setBinding(index, binding);
				}
			}
		}

		/// <summary>
		///   Loads a shader of the given type.
		/// </summary>
		private static int LoadShader(int shaderType, string shaderCode)
		{
			if (String.IsNullOrWhiteSpace(shaderCode))
				return 0;

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
			var log = stackalloc byte[LogBufferLength];

			glCompileShader(shader);
			glGetShaderiv(shader, GL_COMPILE_STATUS, &success);
			glGetShaderInfoLog(shader, LogBufferLength, &logLength, log);

			if (success == GL_FALSE)
				Log.Die($"Shader compilation failed: {Interop.ToString(log).Trim()}");

			if (logLength != 0)
				Log.Debug(Interop.ToString(log).Trim());

			return shader;
		}
	}
}