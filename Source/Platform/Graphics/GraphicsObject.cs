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
	using System.Collections.Generic;
	using System.Diagnostics;
	using Logging;
	using Memory;
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Base class for all objects belonging to a graphics device.
	/// </summary>
	public abstract unsafe class GraphicsObject : DisposableObject
	{
		/// <summary>
		///   Gets the state of the graphics device.
		/// </summary>
		protected static GraphicsState State { get; } = new GraphicsState();

		/// <summary>
		///   Gets the graphics object's underlying OpenGL handle.
		/// </summary>
		protected uint Handle { get; set; }

		/// <summary>
		///   In debug builds, checks for OpenGL errors.
		/// </summary>
		[DebuggerHidden, Conditional("DEBUG")]
		protected static void CheckErrors()
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

		/// <summary>
		///   Allocates an OpenGL object using the given allocator.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the object.</param>
		/// <param name="type">A user-friendly name of the type of the allocated object.</param>
		protected static uint Allocate(Allocator allocator, string type)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentNotNullOrWhitespace(type, nameof(type));

			uint handle = 0;
			allocator(1, &handle);
			CheckErrors();

			if (handle == 0)
				Log.Die("Failed to allocate an OpenGL object of type '{0}'.", type);

			return handle;
		}

		/// <summary>
		///   Deallocates an OpenGL object using the given deallocator.
		/// </summary>
		/// <param name="deallocator">The deallocator that should be used to allocate the object.</param>
		/// <param name="obj">The object that should be deallocated.</param>
		protected static void Deallocate(Deallocator deallocator, uint obj)
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
		protected static bool Change<T>(ref T stateValue, T value)
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
		protected static bool Change<T>(T[] stateValues, int index, T value)
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
		protected static void Unset<T>(ref T stateValue, T value)
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
		protected static void Unset<T>(T[] stateValues, T value)
			where T : class
		{
			for (var i = 0; i < stateValues.Length; ++i)
			{
				if (stateValues[i] == value)
					stateValues[i] = null;
			}
		}

		/// <summary>
		///   Casts the graphics object to its underlying OpenGL handle.
		/// </summary>
		public static implicit operator uint(GraphicsObject obj)
		{
			Assert.ArgumentNotNull(obj, nameof(obj));
			return obj.Handle;
		}

		/// <summary>
		///   Represents a pointer to an OpenGL allocation function.
		/// </summary>
		/// <param name="count">The number of objects that should be allocated.</param>
		/// <param name="objects">A pointer to the allocated objects.</param>
		protected delegate void Allocator(int count, uint* objects);

		/// <summary>
		///   Represents a pointer to an OpenGL deallocation function.
		/// </summary>
		/// <param name="count">The number of objects that should be deallocated.</param>
		/// <param name="objects">A pointer to the objects that should be deallocated.</param>
		protected delegate void Deallocator(int count, uint* objects);
	}
}