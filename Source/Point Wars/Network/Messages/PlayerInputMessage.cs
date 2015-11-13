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

namespace PointWars.Network.Messages
{
	using System.Numerics;
	using Gameplay.SceneNodes.Entities;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a server about the input state of a client.
	/// </summary>
	[UnreliableTransmission(MessageType.PlayerInput)]
	internal sealed class PlayerInputMessage : Message
	{
		/// <summary>
		///   Gets the boolean state value for the down movement input, including the seven previous states.
		/// </summary>
		public byte MoveDown { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the up movement input, including the seven previous states.
		/// </summary>
		public byte MoveUp { get; private set; }

		/// <summary>
		///   Gets the monotonically increasing frame number, starting at 1.
		/// </summary>
		public uint FrameNumber { get; private set; }

		/// <summary>
		///   Gets the player that generated the input.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the left movement input, including the seven previous states.
		/// </summary>
		public byte MoveLeft { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the right movement input, including the seven previous states.
		/// </summary>
		public byte MoveRight { get; private set; }

		/// <summary>
		///   Gets the position of the client's target relative to the client's ship.
		/// </summary>
		public Vector2 Target { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the primary weapon firing input, including the seven previous states.
		/// </summary>
		public byte FirePrimary { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the secondary weapon firing input, including the seven previous states.
		/// </summary>
		public byte FireSecondary { get; private set; }

		/// <summary>
		///   Gets the primary weapon selected by the player.
		/// </summary>
		public EntityType PrimaryWeapon { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteUInt32(FrameNumber);
			writer.WriteByte(MoveUp);
			writer.WriteByte(MoveDown);
			writer.WriteByte(MoveLeft);
			writer.WriteByte(MoveRight);
			writer.WriteByte(FirePrimary);
			writer.WriteByte(FireSecondary);
			writer.WriteByte((byte)PrimaryWeapon);
			WriteVector2(ref writer, Target);
		}

		/// <summary>
		///   Deserializes the message using the given reader.
		/// </summary>
		/// <param name="reader">The reader that should be used to deserialize the message.</param>
		public override void Deserialize(ref BufferReader reader)
		{
			Player = ReadIdentifier(ref reader);
			FrameNumber = reader.ReadUInt32();
			MoveUp = reader.ReadByte();
			MoveDown = reader.ReadByte();
			MoveLeft = reader.ReadByte();
			MoveRight = reader.ReadByte();
			FirePrimary = reader.ReadByte();
			FireSecondary = reader.ReadByte();
			PrimaryWeapon = (EntityType)reader.ReadByte();
			Target = ReadVector2(ref reader);
		}

		/// <summary>
		///   Dispatches the message to the given dispatcher.
		/// </summary>
		/// <param name="handler">The dispatcher that should be used to dispatch the message.</param>
		/// <param name="sequenceNumber">The sequence number of the message.</param>
		public override void Dispatch(IMessageHandler handler, uint sequenceNumber)
		{
			handler.OnPlayerInput(this);
		}

		/// <summary>
		///   Creates a chat message that the server broadcasts to all players.
		/// </summary>
		/// <param name="poolAllocator">The pool allocator that should be used to allocate the message.</param>
		/// <param name="player">The player that generated the input.</param>
		/// <param name="frameNumber">The number of the frame during which the input was generated, starting at 1.</param>
		/// <param name="target"></param>
		/// <param name="moveUp">The boolean state value for the up movement input, including the seven previous states.</param>
		/// <param name="moveDown">The boolean state value for the down movement input, including the seven previous states.</param>
		/// <param name="moveLeft">The boolean state value for the left movement input, including the seven previous states.</param>
		/// <param name="moveRight">The boolean state value for the right movement input, including the seven previous states.</param>
		/// <param name="firePrimary">The boolean state value for the primary weapon firing input, including the seven previous states.</param>
		/// <param name="fireSecondary">
		///   The boolean state value for the secondary weapon firing input, including the seven previous states.
		/// </param>
		/// <param name="primaryWeapon">The primary weapon selected by the player.</param>
		public static PlayerInputMessage Create(PoolAllocator poolAllocator, NetworkIdentity player, uint frameNumber, Vector2 target,
												byte moveUp, byte moveDown,
												byte moveLeft, byte moveRight,
												byte firePrimary, byte fireSecondary,
												EntityType primaryWeapon)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));
			Assert.ArgumentInRange(primaryWeapon, nameof(primaryWeapon));

			var message = poolAllocator.Allocate<PlayerInputMessage>();
			message.Player = player;
			message.FrameNumber = frameNumber;
			message.Target = target;
			message.MoveUp = moveUp;
			message.MoveDown = moveDown;
			message.MoveLeft = moveLeft;
			message.MoveRight = moveRight;
			message.FirePrimary = firePrimary;
			message.FireSecondary = fireSecondary;
			message.PrimaryWeapon = primaryWeapon;

			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return
				$"{MessageType}, Player={Player}, FrameNumber={FrameNumber}, Target={{{Target}}}, Up={MoveUp}, Down={MoveDown}, " +
				$"Left={MoveLeft}, Right={MoveRight}, FirePrimary={FirePrimary}, FireSecondary={FireSecondary}";
		}
	}
}