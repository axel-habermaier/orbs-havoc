namespace OrbsHavoc.Views
{
	using System;
	using Gameplay;
	using Platform.Logging;
	using UI;
	using Utilities;

	/// <summary>
	///     Shows event messages such as 'X killed Y' or received chat messages.
	/// </summary>
	internal class EventMessages : View<EventMessagesUI>
	{
		public override void Initialize()
		{
			Show();
		}

		/// <summary>
		///     Adds a chat message to the event list.
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
		///     Adds a player joined message to the event list.
		/// </summary>
		/// <param name="player">The player that has joined the session.</param>
		public void AddJoinMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));

			if (!player.IsServerPlayer)
				Add($"{player.Name}\\default has joined the game.");
		}

		/// <summary>
		///     Adds a player left message to the event list.
		/// </summary>
		/// <param name="player">The player that has left the session.</param>
		public void AddLeaveMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Add($"{player.Name}\\default has left the game.");
		}

		/// <summary>
		///     Adds a player kicked message to the event list.
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
		///     Adds a player timed out message to the event list.
		/// </summary>
		/// <param name="player">The player that has timed out.</param>
		public void AddTimeoutMessage(Player player)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Add($"The connection to {player.Name}\\default has been lost.");
		}

		/// <summary>
		///     Adds a kill message to the event list.
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
		///     Adds a player name change message to the event list.
		/// </summary>
		/// <param name="player">The player that has changed the name.</param>
		/// <param name="previousName">The previous player name.</param>
		public void AddNameChangeMessage(Player player, string previousName)
		{
			Assert.ArgumentNotNull(player, nameof(player));
			Assert.ArgumentNotNullOrWhitespace(previousName, nameof(previousName));

			Add($"{previousName}\\default was renamed to {player.Name}\\default.");
		}

		public override void Update()
		{
			UI.Update();
		}

		public void Clear()
		{
			UI.ClearEvents();
		}

		/// <summary>
		///     Adds the given event message to the list.
		/// </summary>
		/// <param name="message">The message that should be added.</param>
		/// <param name="isChatMessage">Indicates whether the message is a chat message that should be displayed longer.</param>
		private void Add(string message, bool isChatMessage = false)
		{
			UI.AddEvent(message, isChatMessage);
			Log.Info(message);
		}
	}
}