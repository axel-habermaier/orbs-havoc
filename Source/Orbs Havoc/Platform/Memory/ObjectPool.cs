﻿// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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