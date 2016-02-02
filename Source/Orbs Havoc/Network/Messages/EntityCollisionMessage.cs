// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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

namespace OrbsHavoc.Network.Messages
{
	using System.Numerics;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Notifies a client about a collision between two entities.
	/// </summary>
	[UnreliableTransmission(MessageType.EntityCollision)]
	internal sealed class EntityCollisionMessage : Message
	{
		/// <summary>
		///   Gets the first entity involved in the collision.
		/// </summary>
		public NetworkIdentity Entity1 { get; private set; }

		/// <summary>
		///   Gets the second entity involved in the collision.
		/// </summary>
		public NetworkIdentity Entity2 { get; private set; }

		/// <summary>
		///   Gets the position of the impact.
		/// </summary>
		public Vector2 ImpactPosition { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity1);
			WriteIdentifier(ref writer, Entity2);
			WriteVector2(ref writer, ImpactPosition);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity1 = ReadIdentifier(ref reader);
			Entity2 = ReadIdentifier(ref reader);
			ImpactPosition = ReadVector2(ref reader);
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnEntityCollision(this);
		}

		/// <summary>
		///   Creates an collision message that the server broadcasts to all clients.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="entity1">The first entity of the collision.</param>
		/// <param name="entity2">The second entity of the collision.</param>
		/// <param name="impactPosition">The position of the impact.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity entity1, NetworkIdentity entity2, Vector2 impactPosition)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));

			var message = poolAllocator.Allocate<EntityCollisionMessage>();
			message.Entity1 = entity1;
			message.Entity2 = entity2;
			message.ImpactPosition = impactPosition;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Entity1={Entity1}, Entity2={Entity2}, ImpactPosition={ImpactPosition}";
		}
	}
}