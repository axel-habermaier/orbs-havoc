namespace OrbsHavoc.Platform.Graphics
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Runtime.CompilerServices;
	using System.Runtime.InteropServices;
	using Logging;
	using Utilities;
	using static SDL2;

	/// <summary>
	///   Provides OpenGL helper methods.
	/// </summary>
	internal static unsafe partial class OpenGL3
	{
		/// <summary>
		///   Represents a pointer to an OpenGL allocation function.
		/// </summary>
		/// <param name="count">The number of objects that should be allocated.</param>
		/// <param name="objects">A pointer to the allocated objects.</param>
		internal delegate void Allocator(int count, int* objects);

		/// <summary>
		///   Represents a pointer to an OpenGL deallocation function.
		/// </summary>
		/// <param name="count">The number of objects that should be deallocated.</param>
		/// <param name="objects">A pointer to the objects that should be deallocated.</param>
		internal delegate void Deallocator(int count, int* objects);

		/// <summary>
		///   Gets the state of the graphics device.
		/// </summary>
		public static GraphicsState State { get; } = new GraphicsState();

		/// <summary>
		///   Gets a human-readable message for the given OpenGL error.
		/// </summary>
		private static string GetErrorMessage(int error)
		{
			switch (error)
			{
				case GL_INVALID_ENUM:
					return nameof(GL_INVALID_ENUM);
				case GL_INVALID_VALUE:
					return nameof(GL_INVALID_VALUE);
				case GL_INVALID_OPERATION:
					return nameof(GL_INVALID_OPERATION);
				case GL_OUT_OF_MEMORY:
					return nameof(GL_OUT_OF_MEMORY);
				default:
					return "Unknown OpenGL error.";
			}
		}

		/// <summary>
		///   In debug builds, checks for OpenGL errors.
		/// </summary>
		[Conditional("DEBUG"), DebuggerHidden]
		private static void CheckErrors([CallerMemberName] string entryPoint = "")
		{
			var glErrorOccurred = false;
			for (var glError = glGetError(); glError != GL_NO_ERROR; glError = glGetError())
			{
				Log.Error($"OpenGL error after invocation of '{entryPoint}': {GetErrorMessage(glError)}");
				glErrorOccurred = true;
			}

			if (glErrorOccurred)
				Log.Die("Stopped after OpenGL error.");
		}

		/// <summary>
		///   Allocates an OpenGL object using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the object.</param>
		/// <param name="type">A user-friendly name of the type of the allocated object.</param>
		public static int Allocate(Allocator allocator, string type)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNullOrWhitespace(type, nameof(type));

			var handle = 0;
			allocator(1, &handle);
			CheckErrors();

			if (handle == 0)
				Log.Die($"Failed to allocate an OpenGL object of type '{type}'.");

			return handle;
		}

		/// <summary>
		///   Deallocates an OpenGL object using the given deallocator.
		/// </summary>
		/// <param name="deallocator">The deallocator that should be used to allocate the object.</param>
		/// <param name="obj">The object that should be deallocated.</param>
		public static void Deallocate(Deallocator deallocator, int obj)
		{
			Assert.ArgumentNotNull(deallocator, nameof(deallocator));

			deallocator(1, &obj);
			CheckErrors();
		}

		/// <summary>
		///   Changes the state, if the current state value and the new one differ. Returns false to indicate that a state change was
		///   not required.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be changed.</typeparam>
		/// <param name="stateValue">The current state value that will be updated, if necessary.</param>
		/// <param name="value">The new state value.</param>
		public static bool Change<T>(ref T stateValue, T value)
		{
			if (EqualityComparer<T>.Default.Equals(stateValue, value))
				return false;

			stateValue = value;
			return true;
		}

		/// <summary>
		///   Changes the state, if the current state value and the new one differ. Returns false to indicate that a state change was
		///   not required.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be changed.</typeparam>
		/// <param name="stateValues">The current state values that will be updated, if necessary.</param>
		/// <param name="index">The index of the state value that should be updated.</param>
		/// <param name="value">The new state value.</param>
		public static bool Change<T>(T[] stateValues, int index, T value)
			where T : class
		{
			Assert.ArgumentNotNull(stateValues, nameof(stateValues));
			Assert.ArgumentInRange(index, stateValues, nameof(stateValues));

			if (stateValues[index] == value)
				return false;

			stateValues[index] = value;
			return true;
		}

		/// <summary>
		///   Unsets the given state value if it matches the given value.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be unset.</typeparam>
		/// <param name="stateValue">The current state value that will be unset, if necessary.</param>
		/// <param name="value">The state value that should be unset.</param>
		public static void Unset<T>(ref T stateValue, T value)
			where T : class
		{
			if (stateValue == value)
				stateValue = null;
		}

		/// <summary>
		///   Unsets the given state value if it matches the given value.
		/// </summary>
		/// <typeparam name="T">The type of the state that should be unset.</typeparam>
		/// <param name="stateValues">The current state values that will be unset, if necessary.</param>
		/// <param name="value">The state value that should be unset.</param>
		public static void Unset<T>(T[] stateValues, T value)
			where T : class
		{
			for (var i = 0; i < stateValues.Length; ++i)
			{
				if (stateValues[i] == value)
					stateValues[i] = null;
			}
		}

		private static T Load<T>(string entryPoint)
		{
			using (var entryPointPtr = Interop.ToPointer(entryPoint))
			{
				var function = SDL_GL_GetProcAddress(entryPointPtr);

				// Stupid, but might be necessary; see also https://www.opengl.org/wiki/Load_OpenGL_Functions
				if ((long)function >= -1 && (long)function <= 3)
					Log.Die($"Failed to load OpenGL entry point '{entryPoint}'.");

				return Marshal.GetDelegateForFunctionPointer<T>(new IntPtr(function));
			}
		}
	}
}