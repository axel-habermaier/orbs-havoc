﻿namespace OrbsHavoc.Network.Messages
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Numerics;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///     Represents a message that is used for the communication between the server and the client.
	/// </summary>
	internal abstract class Message : PooledObject
	{
		/// <summary>
		///     Maps a message type to its transmission information.
		/// </summary>
		private static readonly Dictionary<Type, TransmissionInfo> _transmissionInfos = new Dictionary<Type, TransmissionInfo>();

		/// <summary>
		///     Maps a message type to a message instance allocator.
		/// </summary>
		private static readonly Dictionary<MessageType, Func<PoolAllocator, Message>> _messageConstructors =
			new Dictionary<MessageType, Func<PoolAllocator, Message>>(new MessageTypeComparer());

		/// <summary>
		///     Initializes the type.
		/// </summary>
		static Message()
		{
			var allocateMethod = typeof(PoolAllocator).GetTypeInfo().GetDeclaredMethod("Allocate");

			var messageTypes = typeof(Message)
				.GetTypeInfo()
				.Assembly
				.DefinedTypes
				.Where(type => type.GetTypeInfo().IsClass && !type.GetTypeInfo().IsAbstract && typeof(Message).GetTypeInfo().IsAssignableFrom(type));

			foreach (var messageType in messageTypes)
			{
				var messageAllocationMethod = allocateMethod.MakeGenericMethod(messageType.AsType());
				var allocator = (Func<PoolAllocator, Message>)Delegate.CreateDelegate(typeof(Func<PoolAllocator, Message>), messageAllocationMethod);
				var reliable = (ReliableTransmissionAttribute)messageType
					.GetCustomAttributes(typeof(ReliableTransmissionAttribute), false).FirstOrDefault();
				var unreliable = (UnreliableTransmissionAttribute)messageType
					.GetCustomAttributes(typeof(UnreliableTransmissionAttribute), false).FirstOrDefault();

				Assert.That(reliable == null || unreliable == null,
					$"Cannot use both reliable and unreliable transmission for messages of type '{messageType.FullName}'.");
				Assert.That(reliable != null || unreliable != null,
					$"No transmission type has been specified for messages of type '{messageType.FullName}'.");

				if (reliable != null)
				{
					Assert.That((int)reliable.MessageType < 100, "Invalid reliable transmission message type.");

					_messageConstructors.Add(reliable.MessageType, allocator);
					_transmissionInfos.Add(messageType.AsType(), new TransmissionInfo
					{
						BatchedTransmission = false,
						MessageType = reliable.MessageType,
						ReliableTransmission = true
					});
				}

				if (unreliable != null)
				{
					Assert.That((int)unreliable.MessageType > 100, "Invalid unreliable transmission message type.");

					_messageConstructors.Add(unreliable.MessageType, allocator);
					_transmissionInfos.Add(messageType.AsType(), new TransmissionInfo
					{
						BatchedTransmission = unreliable.EnableBatching,
						MessageType = unreliable.MessageType,
						ReliableTransmission = false,
					});
				}

				RuntimeHelpers.RunClassConstructor(messageType.AsType().TypeHandle);
			}
		}

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		protected Message()
		{
			var transmissionInfo = _transmissionInfos[GetType()];

			MessageType = transmissionInfo.MessageType;
			IsReliable = transmissionInfo.ReliableTransmission;
			UseBatchedTransmission = transmissionInfo.BatchedTransmission;
		}

		/// <summary>
		///     Gets the type of the message.
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		///     Gets a value indicating whether as many messages of this type as possible are batched together into a
		///     single network transmission.
		/// </summary>
		public bool UseBatchedTransmission { get; }

		/// <summary>
		///     Gets a value indicating whether the message is reliable.
		/// </summary>
		public bool IsReliable { get; }

		/// <summary>
		///     Gets a value indicating whether the message is unreliable.
		/// </summary>
		public bool IsUnreliable => !IsReliable;

		/// <summary>
		///     Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public abstract void Serialize(ref BufferWriter writer);

		/// <summary>
		///     Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public abstract void Deserialize(ref BufferReader reader);

		/// <summary>
		///     Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public abstract void Dispatch(IMessageHandler handler, uint sequenceNumber);

		/// <summary>
		///     Allocates a message instance for a message of the given message transmission type.
		/// </summary>
		/// <param name="allocator">The allocator that should be used to allocate the message.</param>
		/// <param name="messageType">The message transmission type a message instance should be created for.</param>
		public static Message Allocate(PoolAllocator allocator, MessageType messageType)
		{
			Assert.ArgumentNotNull(allocator, nameof(allocator));
			Assert.ArgumentInRange(messageType, nameof(messageType));

			if (!_messageConstructors.TryGetValue(messageType, out var constructor))
				throw new InvalidOperationException("Unsupported message type.");

			return constructor(allocator);
		}

		/// <summary>
		///     Reads an identity from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the identity should be read from.</param>
		protected static NetworkIdentity ReadIdentifier(ref BufferReader buffer)
		{
			var generation = buffer.ReadUInt16();
			var id = buffer.ReadUInt16();

			return new NetworkIdentity(id, generation);
		}

		/// <summary>
		///     Reads a vector from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the vector should be read from.</param>
		protected static Vector2 ReadVector2(ref BufferReader buffer)
		{
			return new Vector2(buffer.ReadInt16(), buffer.ReadInt16());
		}

		/// <summary>
		///     Reads an orientation, in radians, from the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the orientation should be read from.</param>
		protected static float ReadOrientation(ref BufferReader buffer)
		{
			return MathUtils.DegToRad(buffer.ReadUInt16() / NetworkProtocol.AngleFactor);
		}

		/// <summary>
		///     Writes the given identity into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the identity should be written into.</param>
		/// <param name="identity">The identity that should be written into the buffer.</param>
		protected static void WriteIdentifier(ref BufferWriter buffer, NetworkIdentity identity)
		{
			buffer.WriteUInt16(identity.Generation);
			buffer.WriteUInt16(identity.Identifier);
		}

		/// <summary>
		///     Writes the given vector into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the vector should be written into.</param>
		/// <param name="vector">The vector that should be written into the buffer.</param>
		protected static void WriteVector2(ref BufferWriter buffer, Vector2 vector)
		{
			vector = Vector2.Clamp(vector, new Vector2(Int16.MinValue), new Vector2(Int16.MaxValue));
			buffer.WriteInt16((short)vector.X);
			buffer.WriteInt16((short)vector.Y);
		}

		/// <summary>
		///     Writes the given orientation into the buffer.
		/// </summary>
		/// <param name="buffer">The buffer the orientation should be written into.</param>
		/// <param name="orientation">The orientation, in radians, that should be written into the buffer.</param>
		protected static void WriteOrientation(ref BufferWriter buffer, float orientation)
		{
			buffer.WriteUInt16((ushort)(MathUtils.RadToDeg360(orientation) * NetworkProtocol.AngleFactor));
		}

		/// <summary>
		///     The comparer that is used by the message constructor dictionary to compare the message type keys,
		///     otherwise boxing would occur.
		/// </summary>
		private class MessageTypeComparer : IEqualityComparer<MessageType>
		{
			/// <summary>
			///     Determines whether the specified message types are equal.
			/// </summary>
			/// <param name="messageType1">The first message type to compare.</param>
			/// <param name="messageType2">The second message type to compare.</param>
			public bool Equals(MessageType messageType1, MessageType messageType2)
			{
				return messageType1 == messageType2;
			}

			/// <summary>
			///     Returns a hash code for the given message type.
			/// </summary>
			/// <param name="messageType">The message type the hash code should be returned for.</param>
			public int GetHashCode(MessageType messageType)
			{
				return (int)messageType;
			}
		}

		/// <summary>
		///     Provides transmission information about a message.
		/// </summary>
		private struct TransmissionInfo
		{
			/// <summary>
			///     Indicates whether as many messages as possible should be batched together for optimized transmission.
			/// </summary>
			public bool BatchedTransmission;

			/// <summary>
			///     The transmission type of the message.
			/// </summary>
			public MessageType MessageType;

			/// <summary>
			///     Indicates whether reliable or unreliable transmission should be used.
			/// </summary>
			public bool ReliableTransmission;
		}
	}
}