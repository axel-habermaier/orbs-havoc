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
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Informs a server about the input state of a client.
	/// </summary>
	[UnreliableTransmission(MessageType.PlayerInput)]
	internal sealed class PlayerInputMessage : Message
	{
		/// <summary>
		///   Gets the boolean state value for the backwards input, including the seven previous states.
		/// </summary>
		public byte Backward { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the forward input, including the seven previous states.
		/// </summary>
		public byte Forward { get; private set; }

		/// <summary>
		///   Gets the monotonically increasing frame number, starting at 1.
		/// </summary>
		public uint FrameNumber { get; private set; }

		/// <summary>
		///   Gets the player that generated the input.
		/// </summary>
		public NetworkIdentity Player { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the strafe left input, including the seven previous states.
		/// </summary>
		public byte StrafeLeft { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the strafe right input, including the seven previous states.
		/// </summary>
		public byte StrafeRight { get; private set; }

		/// <summary>
		///   Gets the position of the client's target relative to the client's ship.
		/// </summary>
		public Vector2 Target { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the turn left input, including the seven previous states.
		/// </summary>
		public byte TurnLeft { get; private set; }

		/// <summary>
		///   Gets the boolean state value for the turn right input, including the seven previous states.
		/// </summary>
		public byte TurnRight { get; private set; }

		/// <summary>
		///   Serializes the message using the given writer.
		/// </summary>
		/// <param name="writer">The writer that should be used to serialize the message.</param>
		public override void Serialize(ref BufferWriter writer)
		{
			WriteIdentifier(ref writer, Player);
			writer.WriteUInt32(FrameNumber);
			writer.WriteByte(Forward);
			writer.WriteByte(Backward);
			writer.WriteByte(TurnLeft);
			writer.WriteByte(TurnRight);
			writer.WriteByte(StrafeLeft);
			writer.WriteByte(StrafeRight);
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
			Forward = reader.ReadByte();
			Backward = reader.ReadByte();
			TurnLeft = reader.ReadByte();
			TurnRight = reader.ReadByte();
			StrafeLeft = reader.ReadByte();
			StrafeRight = reader.ReadByte();
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
		/// <param name="forward">The boolean state value for the forward input, including the seven previous states.</param>
		/// <param name="backward">The boolean state value for the backwards input, including the seven previous states.</param>
		/// <param name="strafeLeft">The boolean state value for the strafe left input, including the seven previous states.</param>
		/// <param name="strafeRight">The boolean state value for the strafe right input, including the seven previous states.</param>
		/// <param name="turnLeft">The boolean state value for the turn left input, including the seven previous states.</param>
		/// <param name="turnRight">The boolean state value for the turn right input, including the seven previous states.</param>
		public static PlayerInputMessage Create(PoolAllocator poolAllocator, NetworkIdentity player, uint frameNumber, Vector2 target,
												byte forward, byte backward,
												byte strafeLeft, byte strafeRight,
												byte turnLeft, byte turnRight)
		{
			Assert.ArgumentNotNull(poolAllocator, nameof(poolAllocator));

			var message = poolAllocator.Allocate<PlayerInputMessage>();
			message.Player = player;
			message.FrameNumber = frameNumber;
			message.Target = target;
			message.Forward = forward;
			message.Backward = backward;
			message.StrafeLeft = strafeLeft;
			message.StrafeRight = strafeRight;
			message.TurnLeft = turnLeft;
			message.TurnRight = turnRight;

			return message;
		}

		/// <summary>
		///   Returns a string that represents the message.
		/// </summary>
		public override string ToString()
		{
			return $"{MessageType}, Player={Player}, FrameNumber={FrameNumber}, Target={{{Target}}}, Forward={Forward}, Backward={Backward}, " +
				   $"StrafeLeft={StrafeLeft}, StrafeRight={StrafeRight}, TurnLeft={TurnLeft}, TurnRight={TurnRight}";
		}
	}
}