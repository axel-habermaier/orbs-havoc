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

namespace OrbsHavoc.Gameplay.Client
{
	using Network;
	using Network.Messages;
	using Utilities;

	/// <summary>
	///   Implements the client logic for handling incoming and outgoing client messages.
	/// </summary>
	internal partial class ClientLogic
	{
		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnConnect(ClientConnectMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles the given message.
		/// </summary>
		/// <param name="message">The message that should be dispatched.</param>
		void IMessageHandler.OnPlayerInput(PlayerInputMessage message)
		{
			HandleUnsupportedMessage(message);
		}

		/// <summary>
		///   Handles an unsupported message.
		/// </summary>
		/// <param name="message">The unsupported message that should be handled.</param>
		private static void HandleUnsupportedMessage(Message message)
		{
			Assert.That(false, $"The client cannot handle a message of type '{message.MessageType}'.");
		}
	}
}