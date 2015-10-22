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
	using System.Diagnostics;
	using GLFW3;
	using Logging;
	using Memory;
	using Utilities;
	using static GLFW3.GLFW;

	/// <summary>
	///   Represents an instance of the native platform library.
	/// </summary>
	internal unsafe class PlatformLibrary : DisposableObject
	{
		private static PlatformLibrary _instance;
		private static readonly GLFWerrorfun ErrorCallback = OnError;

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
				glfwSetErrorCallback(ErrorCallback);
				if (glfwInit() == 0)
					Log.Die("GLFW initialization failed.");

				int major, minor, revision;
				glfwGetVersion(&major, &minor, &revision);

				if (major != GLFW_VERSION_MAJOR || minor < GLFW_VERSION_MINOR || (minor == GLFW_VERSION_MINOR && revision < GLFW_VERSION_REVISION))
				{
					Log.Die("GLFW is outdated; version {0}.{1}.{2} was found but at least version {3}.{4}.{5} is required.",
						major, minor, revision, GLFW_VERSION_MAJOR, GLFW_VERSION_MINOR, GLFW_VERSION_REVISION);
				}

				Log.Info("GLFW {0}.{1}.{2} initialized.", GLFW_VERSION_MAJOR, GLFW_VERSION_MINOR, GLFW_VERSION_REVISION);
			}
			catch (DllNotFoundException)
			{
				Log.Die("Unable to load GLFW 3. The library could not be not found. " +
						"Make sure GLFW 3 is installed on your system or 'glfw3.dll' is placed in the application directory.");
			}
		}

		/// <summary>
		///   Terminates the application after a GLFW error occurred.
		/// </summary>
		[DebuggerHidden]
		private static void OnError(int error, string description)
		{
			var message = description.Trim().Replace("\r", "");

			if (message.EndsWith(".."))
				message = message.Substring(0, message.Length - 1);
			else if (!message.EndsWith(".") && !message.EndsWith("!") && !message.EndsWith("?"))
				message += ".";

			Log.Die("{0}", message);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			glfwTerminate();
			_instance = null;
		}
	}
}