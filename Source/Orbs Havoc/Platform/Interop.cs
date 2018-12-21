namespace OrbsHavoc.Platform
{
	using System;
	using System.Runtime.CompilerServices;
	using System.Text;
	using Memory;
	using Utilities;

	/// <summary>
	///     Provides methods that facilitate native code interop.
	/// </summary>
	internal static unsafe class Interop
	{
		/// <summary>
		///     Converts the given native string to a .NET string.
		/// </summary>
		/// <param name="str">The string that should be converted.</param>
		public static string ToString(byte* str)
		{
			var length = 0;
			while (str[length] != '\0')
				++length;

			return Encoding.UTF8.GetString(str, length);
		}

		/// <summary>
		///     Gets a pinned pointer to the string.
		/// </summary>
		/// <param name="str">The string the pointer should be created for.</param>
		public static PinnedPointer ToPointer(string str)
		{
			return PinnedPointer.Create(Encoding.UTF8.GetBytes(str));
		}

		/// <summary>
		///     Copies given number of bytes from the source to the destination.
		/// </summary>
		/// <param name="destination">The address of the first byte that should be written.</param>
		/// <param name="source">The address of the first byte that should be read.</param>
		/// <param name="elementCount">The number of elements that should be copied.</param>
		internal static void Copy<T>(T* destination, T* source, int elementCount)
			where T : unmanaged
		{
			Assert.ArgumentNotNull(new IntPtr(destination), nameof(destination));
			Assert.ArgumentNotNull(new IntPtr(source), nameof(source));
			Assert.ArgumentSatisfies(elementCount > 0, nameof(elementCount), "At least 1 element must be copied.");
			Assert.ArgumentSatisfies(
				(source < destination && (byte*)source + (elementCount * sizeof(T)) <= destination) ||
				(destination < source && (byte*)destination + (elementCount * sizeof(T)) <= source),
				nameof(source), "The memory regions overlap.");

			Unsafe.CopyBlock(destination, source, (uint)(elementCount * sizeof(T)));
		}
	}
}