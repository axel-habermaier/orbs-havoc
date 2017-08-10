namespace OrbsHavoc.Platform.Memory
{
	using System;
	using System.Diagnostics;
	using Logging;
	using Utilities;

	/// <summary>
	///   Base implementation for the IDisposable interface. In debug builds, throws an exception if the finalizer runs
	///   because of the object not having been disposed by calling the Dispose() method. In release builds, the finalizer
	///   is not executed and the object might silently leak unmanaged resources.
	/// </summary>
	internal abstract class DisposableObject : IDisposable
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
		[Conditional("DEBUG")]
		public void SetDescription(string description)
		{
#if DEBUG
			_description = description;
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
			Log.Error($"Finalizer runs for a disposable object of type '{GetType().FullName}'.\nInstance description: '{_description ?? "None"}'");
		}
#endif
	}
}