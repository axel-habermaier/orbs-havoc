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

namespace PointWars.Views
{
	using System;
	using Assets;
	using Gameplay;
	using Platform.Logging;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	/// <summary>
	///   Shows event messages such as 'X killed Y' or received chat messages.
	/// </summary>
	internal class EventMessages : View
	{
		private const int MaxMessageCount = 16;
		private readonly StackPanel _layoutRoot = new StackPanel { Font = AssetBundle.Roboto14, IsHitTestVisible = false };
		private readonly double[] _removalTimes = new double[MaxMessageCount];
		private Clock _clock = new Clock();
		private int _messageCount;

		/// <summary>
		///   Initializes the view.
		/// </summary>
		public override void Initialize()
		{
			Show();
			RootElement = _layoutRoot;

			for (var i = 0; i < MaxMessageCount; ++i)
				_layoutRoot.Add(new Label { Visibility = Visibility.Collapsed, Margin = new Thickness(2), TextWrapping = TextWrapping.Wrap });
		}

		/// <summary>
		///   Adds a chat message to the event list.
		/// </summary>
		/// <param name="player">The player that sent the chat message.</param>
		/// <param name="chatMessage">The chat message that has been sent.</param>
		public void AddChatMessage(Player player, string chatMessage)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNullOrWhitespace(chatMessage, nameof(chatMessage));

			Add($"{player.Name}\\default: \\yellow{chatMessage}", isChatMessage: true);
		}

		/// <summary>
		///   Adds a player joined message to the event list.
		/// </summary>
		/// <param name="player">The player that has joined the session.</param>
		public void AddJoinMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));

			if (!player.IsServerPlayer)
				Add($"{player.Name}\\default has joined the game.");
		}

		/// <summary>
		///   Adds a player left message to the event list.
		/// </summary>
		/// <param name="player">The player that has left the session.</param>
		public void AddLeaveMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Add($"{player.Name}\\default has left the game.");
		}

		/// <summary>
		///   Adds a player kicked message to the event list.
		/// </summary>
		/// <param name="player">The player that has been kicked from the session.</param>
		/// <param name="reason">The reason for the kick.</param>
		public void AddKickedMessage(Player player, string reason)
		{
			Assert.ArgumentNotNull(player, nameof(player));

			if (String.IsNullOrWhiteSpace(reason))
				Add($"{player.Name}\\default has been kicked.");
			else
				Add($"{player.Name}\\default has been kicked: {reason}");
		}

		/// <summary>
		///   Adds a player timed out message to the event list.
		/// </summary>
		/// <param name="player">The player that has timed out.</param>
		public void AddTimeoutMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Add($"The connection to {player.Name}\\default has been lost.");
		}

		/// <summary>
		///   Adds a kill message to the event list.
		/// </summary>
		/// <param name="killer">The player that has scored the kill.</param>
		/// <param name="victim">The player that has been killed.</param>
		public void AddKillMessage(Player killer, Player victim)
		{
			Assert.ArgumentNotNull(killer, nameof(killer));
			Assert.ArgumentNotNull(victim, nameof(victim));

			if (killer == victim)
				Add($"{victim.Name}\\default got himself killed.");
			else if (killer.IsServerPlayer)
				Add($"{victim.Name}\\default got killed.");
			else
				Add($"{killer.Name}\\default killed {victim.Name}\\default.");
		}

		/// <summary>
		///   Adds a player name change message to the event list.
		/// </summary>
		/// <param name="player">The player that has changed the name.</param>
		/// <param name="previousName">The previous player name.</param>
		public void AddNameChangeMessage(Player player, string previousName)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNullOrWhitespace(previousName, nameof(previousName));

			Add($"{previousName}\\default was renamed to {player.Name}\\default.");
		}

		/// <summary>
		///   Updates the list, removing all messages that have timed out.
		/// </summary>
		public override void Update()
		{
			for (var i = 0; i < _messageCount; ++i)
			{
				var timedOut = _removalTimes[i] <= _clock.Seconds;
				if (!timedOut)
					continue;

				// Remove the label, add it to the end and hide it
				var child = _layoutRoot.Children[i];
				child.Visibility = Visibility.Collapsed;

				_layoutRoot.Children.RemoveAt(i);
				_layoutRoot.Add(child);

				// Remove the removal time
				if (i != _messageCount)
					Array.Copy(_removalTimes, i + 1, _removalTimes, i, MaxMessageCount - i - 1);

				--_messageCount;
				--i;
			}
		}

		/// <summary>
		///   Clears the list, removing all messages.
		/// </summary>
		public void Clear()
		{
			_messageCount = 0;
			foreach (var child in _layoutRoot.Children)
				child.Visibility = Visibility.Collapsed;
		}

		/// <summary>
		///   Adds the given event message to the list.
		/// </summary>
		/// <param name="message">The message that should be added.</param>
		/// <param name="isChatMessage">Indicates whether the message is a chat message that should be displayed longer.</param>
		private void Add(string message, bool isChatMessage = false)
		{
			if (_messageCount == MaxMessageCount)
			{
				// If we're out of labels, remove the top-most one, hide it, and add it back at the end
				var label = _layoutRoot.Children[0];
				label.Visibility = Visibility.Collapsed;

				_layoutRoot.Children.RemoveAt(0);
				_layoutRoot.Add(label);

				// Also, shift down the removal times
				Array.Copy(_removalTimes, 1, _removalTimes, 0, MaxMessageCount - 1);

				--_messageCount;
			}

			_layoutRoot.Children[_messageCount].Visibility = Visibility.Visible;
			((Label)_layoutRoot.Children[_messageCount]).Text = message;
			_removalTimes[_messageCount] = _clock.Seconds + (isChatMessage ? Cvars.ChatMessageDisplayTime : Cvars.EventMessageDisplayTime);
			++_messageCount;

			Log.Info("{0}", message);
		}
	}
}