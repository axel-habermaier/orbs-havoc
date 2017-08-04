namespace OrbsHavoc.Platform.Memory
{
	using System.Collections.Generic;
	using Utilities;

	/// <summary>
	///   A base class for object pools.
	/// </summary>
	public abstract class ObjectPool : DisposableObject
	{
		/// <summary>
		///   The object pools with global lifetime that should be disposed automatically during application shutdown.
		/// </summary>
		private static readonly List<ObjectPool> _globalPools = new List<ObjectPool>();

		/// <summary>
		///   Used for thread synchronization.
		/// </summary>
		private static readonly object _lockObject = new object();

		/// <summary>
		///   Adds the given pool to the list of global pools that are disposed automatically during application shutdown.
		/// </summary>
		/// <param name="objectPool">The object pool that should be added.</param>
		protected static void AddGlobalPool(ObjectPool objectPool)
		{
			Assert.ArgumentNotNull(objectPool, nameof(objectPool));

			lock (_lockObject)
			{
				Assert.That(!_globalPools.Contains(objectPool), "The object pool has already been added.");
				_globalPools.Add(objectPool);
			}
		}

		/// <summary>
		///   Disposes all pools with global lifetime.
		/// </summary>
		internal static void DisposeGlobalPools()
		{
			lock (_lockObject)
				_globalPools.SafeDisposeAll();
		}

		/// <summary>
		///   Returns an object to the pool so that it can be reused later.
		/// </summary>
		/// <param name="obj">The object that should be returned to the pool.</param>
		public abstract void Free(object obj);

		/// <summary>
		///   Frees all allocated instances.
		/// </summary>
		public abstract void Free();
	}
}