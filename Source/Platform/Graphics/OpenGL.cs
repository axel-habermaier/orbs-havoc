// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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
	using System;
	using System.Diagnostics;
	using Logging;
	using static OpenGL3;
	using static GLFW3.GLFW;

	/// <summary>
	///   Represents the OpenGL context.
	/// </summary>
	internal static unsafe class OpenGL
	{
		private static bool _isInitialized;

		/// <summary>
		///   Initializes OpenGL.
		/// </summary>
		public static void Initialize()
		{
			if (_isInitialized)
				return;

			Load(LoadEntryPoint);

			int major, minor;
			glGetIntegerv(GL_MAJOR_VERSION, &major);
			glGetIntegerv(GL_MINOR_VERSION, &minor);

			if (major < 3 || (major == 3 && minor < 3))
				Log.Die("Only OpenGL {0}.{1} seems to be supported. OpenGL 3.3 is required.", major, minor);

			if (!IsExtensionSupported("GL_ARB_shading_language_420pack"))
				Log.Die("Incompatible graphics card. Not all required OpenGL extensions are supported.");

			Log.Info("OpenGL renderer: {0} ({1})", new string((sbyte*)glGetString(GL_RENDERER)), new string((sbyte*)glGetString(GL_VENDOR)));
			Log.Info("OpenGL version: {0}", new string((sbyte*)glGetString(GL_VERSION)));
			Log.Info("OpenGL GLSL version: {0}", new string((sbyte*)glGetString(GL_SHADING_LANGUAGE_VERSION)));

			_isInitialized = true;
		}

		/// <summary>
		///   Loads an OpenGL entry point of the given name.
		/// </summary>
		/// <param name="entryPoint">The name of the entry point that should be loaded.</param>
		private static IntPtr LoadEntryPoint(string entryPoint)
		{
			var function = glfwGetProcAddress(entryPoint);

			// Stupid, but might be necessary; see also https://www.opengl.org/wiki/Load_OpenGL_Functions
			if ((long)function >= -1 && (long)function <= 3)
				Log.Die("Failed to load OpenGL entry point '{0}'.", entryPoint);

			return new IntPtr(function);
		}

		/// <summary>
		///   Checks whether the OpenGL extension with the given name is supported by the current OpenGL context.
		/// </summary>
		/// <param name="extensionName">The name of the extension that should be checked.</param>
		private static bool IsExtensionSupported(string extensionName)
		{
			if (glfwExtensionSupported(extensionName) != 0)
				return true;

			Log.Error("Extension '{0}' is not supported.", extensionName);
			return false;
		}

		/// <summary>
		///   In debug builds, checks for OpenGL errors.
		/// </summary>
		[DebuggerHidden, Conditional("DEBUG")]
		public static void CheckErrors()
		{
			var glErrorOccurred = false;
			for (var glError = glGetError(); glError != GL_NO_ERROR; glError = glGetError())
			{
				string msg;
				switch (glError)
				{
					case GL_INVALID_ENUM:
						msg = "GL_INVALID_ENUM";
						break;
					case GL_INVALID_VALUE:
						msg = "GL_INVALID_VALUE";
						break;
					case GL_INVALID_OPERATION:
						msg = "GL_INVALID_OPERATION";
						break;
					case GL_OUT_OF_MEMORY:
						msg = "GL_OUT_OF_MEMORY";
						break;
					default:
						msg = "Unknown OpenGL error.";
						break;
				}

				Log.Error("OpenGL error: {0}", msg);
				glErrorOccurred = true;
			}

			if (glErrorOccurred)
				Log.Die("Stopped after OpenGL error.");
		}
	}
}