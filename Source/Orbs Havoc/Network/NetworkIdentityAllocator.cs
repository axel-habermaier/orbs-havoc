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
	using System.Collections.Generic;
	using Utilities;

	/// <summary>
	///   Allocates generational identities.
	/// </summary>
	internal struct NetworkIdentityAllocator
	{
		/// <summary>
		///   The available identifiers that can be reassigned.
		/// </summary>
		private readonly Queue<ushort> _available;

		/// <summary>
		///   Stores the current generation for identifiers that are currently allocated and the next generation for identifiers that
		///   are currently available.
		/// </summary>
		private readonly ushort[] _generations;

		/// <summary>
		///   The next unused identifier.
		/// </summary>
		private ushort _nextIdentifier;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="maxIdentities">The maximum number of identities that can be allocated at the same time.</param>
		public NetworkIdentityAllocator(ushort maxIdentities)
			: this()
		{
			_available = new Queue<ushort>();
			_generations = new ushort[maxIdentities];
		}

		/// <summary>
		///   Allocates an identity, possibly reusing a previously allocated one with an increased generation count.
		/// </summary>
		public NetworkIdentity Allocate()
		{
			ushort identifier;
			if (_available.Count > 0)
				identifier = _available.Dequeue();
			else
			{
				identifier = _nextIdentifier++;
				Assert.That(identifier < _generations.Length, "Too many identifiers have been allocated.");
			}

			return new NetworkIdentity(identifier, _generations[identifier]);
		}

		/// <summary>
		///   Checks whether the given identity is valid, i.e., has been allocated previously and has not yet been freed.
		/// </summary>
		/// <param name="identity">The identity that should be checked.</param>
		public bool IsValid(NetworkIdentity identity)
		{
			return _generations[identity.Identifier] == identity.Generation;
		}

		/// <summary>
		///   Invalidates the identity so that the next call to IsValid returns false. The identity will not be removed, however,
		///   before Free is called.
		/// </summary>
		/// <param name="identity">The identity that should be invalidated.</param>
		public void Invalidate(NetworkIdentity identity)
		{
			Assert.ArgumentSatisfies(IsValid(identity), nameof(identity), "The identity is already invalid.");
			Assert.ArgumentInRange(identity.Identifier, 0, _nextIdentifier, nameof(identity));
			Assert.ArgumentSatisfies(!_available.Contains(identity.Identifier), nameof(identity), "The identity has already been freed.");

			++_generations[identity.Identifier];
		}

		/// <summary>
		///   Marks the given identity as unused, allowing it to be reallocated later.
		/// </summary>
		/// <param name="identity">The identity that can be reused.</param>
		public void Free(NetworkIdentity identity)
		{
			Assert.ArgumentInRange(identity.Identifier, 0, _nextIdentifier, nameof(identity));
			Assert.ArgumentSatisfies(!_available.Contains(identity.Identifier), nameof(identity), "The identity has already been freed.");

			if (IsValid(identity))
				Invalidate(identity);

			_available.Enqueue(identity.Identifier);
		}
	}
}