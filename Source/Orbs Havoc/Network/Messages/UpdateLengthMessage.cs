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

namespace OrbsHavoc.Network.Messages
{
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a client about an update to a lighting bolt.
	/// </summary>
	[UnreliableTransmission(MessageType.UpdateRay, EnableBatching = true)]
	internal sealed class UpdateLightingBoltMessage : Message
	{
		/// <summary>
		///   Gets the entity that is updated.
		/// </summary>
		public NetworkIdentity Entity { get; private set; }

		/// <summary>
		///   Gets the new lighting bolt length.
		/// </summary>
		public float Length { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Entity);
			writer.WriteUInt16((ushort)Length);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Entity = ReadIdentifier(ref reader);
			Length = reader.ReadUInt16();
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnUpdateLightingBolt(this, sequenceNumber);
		}

		/// <summary>
		///   Creates an update message that the server broadcasts to all players.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="entity">The entity that is updated.</param>
		/// <param name="length">The length of the lighting bolt.</param>
		public static Message Create(PoolAllocator poolAllocator, NetworkIdentity entity, float length)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));

			var message = poolAllocator.Allocate<UpdateLightingBoltMessage>();
			message.Entity = entity;
			message.Length = length;
			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Entity={Entity}, Length={Length}";
		}
	}
}