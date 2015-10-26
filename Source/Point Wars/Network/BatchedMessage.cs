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

namespace PointWars.Network
{
	using System.Collections.Generic;
	using Messages;
	using Utilities;

	/// <summary>
	///   Batches a list of messages for optimized network transmission.
	/// </summary>
	internal class BatchedMessage
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="messageType">The type of the batched messages.</param>
		public BatchedMessage(MessageType messageType)
		{
			Assert.ArgumentInRange(messageType, nameof(messageType));

			MessageType = messageType;
			Messages = new Queue<Message>();
		}

		/// <summary>
		///   Gets the type of the batched messages.
		/// </summary>
		public MessageType MessageType { get; private set; }

		/// <summary>
		///   Gets the messages that the batched message contains.
		/// </summary>
		public Queue<Message> Messages { get; private set; }
	}
}