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

namespace OrbsHavoc.Network
{
	using Utilities;

	/// <summary>
	///   Maps identities to actual objects.
	/// </summary>
	/// <typeparam name="T">The type of the mapped objects.</typeparam>
	internal struct NetworkIdentityMap<T>
		where T : class
	{
		/// <summary>
		///   Maps each identity to the corresponding object.
		/// </summary>
		private readonly ObjectIdentity[] _map;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="capacity">
		///   The number of objects that can be mapped. The valid identities that can be mapped fall into the
		///   range [0;capacity-1].
		/// </param>
		public NetworkIdentityMap(ushort capacity)
		{
			_map = new ObjectIdentity[capacity];
		}

		/// <summary>
		///   Gets the object that corresponds to the given identity. Returns null if no object with the given identity could
		///   be found, or if the generation did not match.
		/// </summary>
		/// <param name="identity">The identity of the object that should be returned.</param>
		public T this[NetworkIdentity identity]
		{
			get
			{
				Assert.ArgumentInRange(identity.Identifier, 0, _map.Length - 1, nameof(identity));

				var obj = _map[identity.Identifier];
				if (obj.Object == null)
					return null;

				return obj.Identity.Generation == identity.Generation ? obj.Object : null;
			}
		}

		/// <summary>
		///   Adds a mapping for the given object.
		/// </summary>
		/// <param name="identity">The identity of the object.</param>
		/// <param name="obj">The object that should be mapped.</param>
		public void Add(NetworkIdentity identity, T obj)
		{
			Assert.ArgumentInRange(identity.Identifier, 0, _map.Length - 1, nameof(identity));
			Assert.ArgumentNotNull(obj, nameof(obj));
			Assert.That(_map[identity.Identifier].Object == null, "There already is a mapping for the object's identity.");

			_map[identity.Identifier] = new ObjectIdentity { Object = obj, Identity = identity };
		}

		/// <summary>
		///   Removes the mapping for the given object.
		/// </summary>
		/// <param name="identity">The identity of the object whose mapping should be removed.</param>
		public void Remove(NetworkIdentity identity)
		{
			Assert.ArgumentInRange(identity.Identifier, 0, _map.Length - 1, nameof(identity));
			Assert.That(_map[identity.Identifier].Object != null, "The object is not mapped.");
			Assert.That(_map[identity.Identifier].Identity.Generation == identity.Generation,
				"Attempted to unmap an object of a different generation.");

			_map[identity.Identifier] = new ObjectIdentity();
		}

		/// <summary>
		///   Gets a value indicating whether the an object with the given identity is currently mapped.
		/// </summary>
		/// <param name="identity">The identity that should be checked.</param>
		public bool Contains(NetworkIdentity identity)
		{
			Assert.ArgumentInRange(identity.Identifier, 0, _map.Length - 1, nameof(identity));

			var obj = _map[identity.Identifier];
			if (obj.Object == null)
				return false;

			return obj.Identity == identity;
		}

		/// <summary>
		///   Associates an object with its identity.
		/// </summary>
		private struct ObjectIdentity
		{
			/// <summary>
			///   The generational identity of the object.
			/// </summary>
			public NetworkIdentity Identity;

			/// <summary>
			///   The object with the generational identity.
			/// </summary>
			public T Object;
		}
	}
}