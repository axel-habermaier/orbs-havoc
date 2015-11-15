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

namespace OrbsHavoc.Platform.Memory
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;

	/// <summary>
	///   Provides extension methods for classes implementing the IDisposable interface.
	/// </summary>
	public static class DisposableExtensions
	{
		/// <summary>
		///   Disposes all objects contained in the list if the list is not null and clears the list.
		/// </summary>
		/// <param name="list">The list that should be disposed.</param>
		[DebuggerHidden]
		public static void SafeDisposeAll<T>(this List<T> list)
			where T : class, IDisposable
		{
			if (list == null)
				return;

			foreach (var obj in list)
				obj.SafeDispose();

			list.Clear();
		}

		/// <summary>
		///   Disposes all objects contained in the queue if the queue is not null and clears the queue.
		/// </summary>
		/// <param name="queue">The queue that should be disposed.</param>
		[DebuggerHidden]
		public static void SafeDisposeAll<T>(this Queue<T> queue)
			where T : class, IDisposable
		{
			if (queue == null)
				return;

			foreach (var obj in queue)
				obj.SafeDispose();

			queue.Clear();
		}

		/// <summary>
		///   Disposes all objects contained in the array if the array is not null.
		/// </summary>
		/// <param name="array">The array that should be disposed.</param>
		[DebuggerHidden]
		public static void SafeDisposeAll<T>(this T[] array)
			where T : class, IDisposable
		{
			if (array == null)
				return;

			foreach (var obj in array)
				obj.SafeDispose();
		}

		/// <summary>
		///   Disposes all objects contained in the list if the list is not null and clears the list.
		/// </summary>
		/// <param name="list">The list that should be disposed.</param>
		[DebuggerHidden]
		public static void DisposeAll<T>(this List<T> list)
			where T : struct, IDisposable
		{
			if (list == null)
				return;

			foreach (var obj in list)
				obj.Dispose();

			list.Clear();
		}

		/// <summary>
		///   Disposes all objects contained in the queue if the queue is not null and clears the queue.
		/// </summary>
		/// <param name="queue">The queue that should be disposed.</param>
		[DebuggerHidden]
		public static void DisposeAll<T>(this Queue<T> queue)
			where T : struct, IDisposable
		{
			if (queue == null)
				return;

			foreach (var obj in queue)
				obj.Dispose();

			queue.Clear();
		}

		/// <summary>
		///   Disposes all objects contained in the array if the array is not null.
		/// </summary>
		/// <param name="array">The array that should be disposed.</param>
		[DebuggerHidden]
		public static void DisposeAll<T>(this T[] array)
			where T : struct, IDisposable
		{
			if (array == null)
				return;

			foreach (var obj in array)
				obj.Dispose();
		}

		/// <summary>
		///   Disposes the object if it is not null.
		/// </summary>
		/// <param name="obj">The object that should be disposed.</param>
		[DebuggerHidden]
		public static void SafeDispose<T>(this T obj)
			where T : class, IDisposable
		{
			var disposableObject = obj as DisposableObject;
			if (disposableObject != null && disposableObject.IsDisposed)
				return;

			obj?.Dispose();
		}
	}
}