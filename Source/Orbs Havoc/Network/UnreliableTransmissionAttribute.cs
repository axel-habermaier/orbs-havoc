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
	using System;
	using Messages;
	using Utilities;

	/// <summary>
	///   When applied to a network message class, indicates that the message should be transmitted unreliably. Optionally, as
	///   many messages of the type as possible can be batched together for optimized transmission.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	internal sealed class UnreliableTransmissionAttribute : Attribute
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="messageType">The network message type of the unreliable message.</param>
		public UnreliableTransmissionAttribute(MessageType messageType)
		{
			Assert.ArgumentInRange(messageType, nameof(messageType));
			MessageType = messageType;
		}

		/// <summary>
		///   Gets the network message type of the unreliable message.
		/// </summary>
		public MessageType MessageType { get; }

		/// <summary>
		///   Gets or sets a value indicating whether as many messages of the type as possible should be batched together for
		///   optimized transmission.
		/// </summary>
		public bool EnableBatching { get; set; }
	}
}