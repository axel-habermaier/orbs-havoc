namespace OrbsHavoc.Platform.Memory
{
	using System;
	using System.Runtime.InteropServices;
	using Utilities;

	/// <summary>
	///   Provides access to a pointer to a pinned object.
	/// </summary>
	internal unsafe struct PinnedPointer : IDisposable
	{
		/// <summary>
		///   The handle of the pinned object.
		/// </summary>
		private GCHandle _handle;

		/// <summary>
		///   The pointer to the pinned offset.
		/// </summary>
		private void* _pointer;

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (_handle.IsAllocated)
				_handle.Free();
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="obj">The object the pointer should be created for.</param>
		/// <param name="offsetInBytes">The offset in bytes that should be applied to the pinned pointer.</param>
		internal static PinnedPointer Create<T>(T obj, int offsetInBytes = 0)
			where T : class
		{
			Assert.ArgumentNotNull(obj, nameof(obj));

			var handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
			return new PinnedPointer { _handle = handle, _pointer = (byte*)handle.AddrOfPinnedObject().ToPointer() + offsetInBytes };
		}

		/// <summary>
		///   Converts the pinned pointer to an IntPtr.
		/// </summary>
		/// <param name="pointer">The pinned pointer that should be converted.</param>
		public static implicit operator IntPtr(PinnedPointer pointer)
		{
			return new IntPtr(pointer._pointer);
		}

		/// <summary>
		///   Converts the pinned pointer to a void pointer.
		/// </summary>
		/// <param name="pointer">The pinned pointer that should be converted.</param>
		public static implicit operator void*(PinnedPointer pointer)
		{
			return pointer._pointer;
		}

		/// <summary>
		///   Converts the pinned pointer to a byte pointer.
		/// </summary>
		/// <param name="pointer">The pinned pointer that should be converted.</param>
		public static implicit operator byte*(PinnedPointer pointer)
		{
			return (byte*)pointer._pointer;
		}
	}
}