﻿namespace OrbsHavoc.Platform.Memory
{
	using System.Collections.Generic;
	using Utilities;

	/// <summary>
	///   Pools objects of different types in order to reduce the pressure on the garbage collector. Instead of creating a new
	///   object of type T whenever one is needed, the pool's Allocate{T}() method should be used to retrieve a previously
	///   allocated instance, if any, or allocate a new one. Once the object is no longer being used, it must be returned to the
	///   pool so that it can be reused later on.
	/// </summary>
	internal class PoolAllocator : DisposableObject
	{
		/// <summary>
		///   The object pools that are used to allocate gameplay objects.
		/// </summary>
		private readonly List<ObjectPool> _objectPools = new List<ObjectPool>();

		/// <summary>
		///   Allocates an instance of the given type, either by creating a new instance or by reusing a previously freed one.
		/// </summary>
		/// <typeparam name="T">The type of the object that should be allocated.</typeparam>
		public T Allocate<T>()
			where T : class, new()
		{
			Assert.NotDisposed(this);

			foreach (var pool in _objectPools)
			{
				if (pool is ObjectPool<T> typedPool)
					return typedPool.Allocate();
			}

			var newPool = new ObjectPool<T>();
			_objectPools.Add(newPool);

			return newPool.Allocate();
		}

		/// <summary>
		///   Frees all allocated instances.
		/// </summary>
		public void Free()
		{
			foreach (var objectPool in _objectPools)
				objectPool.Free();
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_objectPools.SafeDisposeAll();
		}
	}
}