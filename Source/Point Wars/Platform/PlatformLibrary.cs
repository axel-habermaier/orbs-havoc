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

namespace PointWars.Platform
{
	using System;
	using Logging;
	using Memory;
	using Utilities;
	using static SDL2;

	/// <summary>
	///   Represents an instance of the native platform library.
	/// </summary>
	internal unsafe class PlatformLibrary : DisposableObject
	{
		private static PlatformLibrary _instance;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public PlatformLibrary()
		{
			Assert.That(_instance == null, "The native platform library has already been initialized.");
			_instance = this;
		}

		/// <summary>
		///   Initializes the platform library.
		/// </summary>
		public void Initialize()
		{
			try
			{
				if (SDL_Init(SDL_INIT_VIDEO) != 0)
					Log.Die("SDL2 initialization failed: {0}", SDL_GetError());

				SDL_version version;
				SDL_GetVersion(out version);

				const int major = 2, minor = 0, patch = 2;
				if (!SDL_VERSION_ATLEAST(major, minor, patch))
				{
					Log.Die("SDL2 is outdated: Version {0}.{1}.{2} is installed but at least version {3}.{4}.{5} is required.",
						version.major, version.minor, version.patch, major, minor, patch);
				}

				SDL_GL_SetAttribute(SDL_GL_RED_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_GREEN_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_BLUE_SIZE, 8);
				SDL_GL_SetAttribute(SDL_GL_DEPTH_SIZE, 0);
				SDL_GL_SetAttribute(SDL_GL_STENCIL_SIZE, 0);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 3);
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

#if DEBUG
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG | SDL_GL_CONTEXT_DEBUG_FLAG);
#else
				SDL_GL_SetAttribute(SDL_GL_CONTEXT_FLAGS, SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
#endif

				SDL_StopTextInput();
				if (SDL_GL_LoadLibrary(null) != 0)
					Log.Die("Failed to load the OpenGL library: {0}", SDL_GetError());

				Log.Info("SDL {0}.{1}.{2} initialized.", version.major, version.minor, version.patch);
			}
			catch (DllNotFoundException)
			{
				Log.Die("The SDL2 library could not be not found. Make sure SDL2 is installed on your " +
						"system or 'SDL2.dll' is placed in the application directory.");
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			try
			{
				SDL_GL_UnloadLibrary();
				SDL_Quit();
			}
			catch (DllNotFoundException)
			{
			}

			_instance = null;
		}
	}
}