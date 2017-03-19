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

namespace OrbsHavoc.Network
{
	using Platform.Logging;
	using Platform.Memory;

	/// <summary>
	///   Represents the header of a packet.
	/// </summary>
	internal static class PacketHeader
	{
		/// <summary>
		///   Initializes a new instance from a buffer.
		/// </summary>
		/// <param name="buffer">The buffer the header data should be read from.</param>
		/// <param name="acknowledgement">Returns the acknowledged sequence number of the packet.</param>
		public static bool TryRead(ref BufferReader buffer, out uint acknowledgement)
		{
			acknowledgement = 0;

			if (!buffer.CanRead(NetworkProtocol.HeaderSize))
			{
				Log.Warn("Received a packet with an incomplete header.");
				return false;
			}

			if (buffer.ReadUInt32() != NetworkProtocol.AppIdentifier)
			{
				Log.Warn("Received a packet with an invalid application identifier.");
				return false;
			}

			acknowledgement = buffer.ReadUInt32();
			return true;
		}

		/// <summary>
		///   Writes the header into the given buffer.
		/// </summary>
		/// <param name="buffer">The buffer the header should be written into.</param>
		/// <param name="acknowledgement">The acknowledged sequence number of the packet.</param>
		public static void Write(ref BufferWriter buffer, uint acknowledgement)
		{
			buffer.WriteUInt32(NetworkProtocol.AppIdentifier);
			buffer.WriteUInt32(acknowledgement);
		}
	}
}