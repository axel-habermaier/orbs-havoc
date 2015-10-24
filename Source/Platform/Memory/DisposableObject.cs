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

namespace PointWars.Platform.Memory
{
	using System;
	using System.Diagnostics;
	using JetBrains.Annotations;
	using Utilities;

	/// <summary>
	///   Base implementation for the IDisposable interface. In debug builds, throws an exception if the finalizer runs
	///   because of the object not having been disposed by calling the Dispose() method. In release builds, the finalizer
	///   is not executed and the object might silently leak unmanaged resources.
	/// </summary>
	public abstract class DisposableObject : IDisposable
	{
		/// <summary>
		///   Gets a value indicating whether the object has already been disposed.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <summary>
		///   Gets a value indicating  whether the object is currently being disposed.
		/// </summary>
		protected bool IsDisposing { get; private set; }

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			Assert.That(!IsDisposed, "The instance has already been disposed.");

			IsDisposing = true;
			OnDisposing();
			IsDisposing = false;
			IsDisposed = true;

#if DEBUG
			GC.SuppressFinalize(this);
#endif
		}

		/// <summary>
		///   In debug builds, sets a description for the instance in order to make debugging easier.
		/// </summary>
		/// <param name="description">The description of the instance.</param>
		/// <param name="arguments">The arguments that should be copied into the description.</param>
		[Conditional("DEBUG"), StringFormatMethod("description")]
		public void SetDescription(string description, params object[] arguments)
		{
			Assert.ArgumentNotNull(description, nameof(description));

#if DEBUG
			_description = String.Format(description, arguments);
#endif
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected abstract void OnDisposing();

#if DEBUG
	/// <summary>
	///   A description for the instance in order to make debugging easier.
	/// </summary>
		private string _description;

		/// <summary>
		///   Ensures that the instance has been disposed.
		/// </summary>
		~DisposableObject()
		{
			Log.Error("Finalizer runs for a disposable object of type '{0}'.\nInstance description: '{1}'",
				GetType().FullName, _description ?? "None");
		}
#endif
	}
}