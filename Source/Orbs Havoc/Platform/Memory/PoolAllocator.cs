// The MIT License (MIT)
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
	///   Pools objects of different types in order to reduce the pressure on the garbage collector. Instead of creating a new
	///   object of type T whenever one is needed, the pool's Allocate{T}() method should be used to retrieve a previously
	///   allocated instance, if any, or allocate a new one. Once the object is no longer being used, it must be returned to the
	///   pool so that it can be reused later on.
	/// </summary>
	public class PoolAllocator : DisposableObject
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

			// Not using Linq for performance reasons
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