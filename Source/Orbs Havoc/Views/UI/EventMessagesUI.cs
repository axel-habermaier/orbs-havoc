namespace OrbsHavoc.Views.UI
{
	using System;
	using Assets;
	using Scripting;
	using UserInterface;
	using UserInterface.Controls;
	using Utilities;

	internal class EventMessagesUI : StackPanel
	{
		private const int MaxMessageCount = 16;

		private readonly double[] _removalTimes = new double[MaxMessageCount];
		private int _messageCount;

		public EventMessagesUI()
		{
			Font = AssetBundle.Roboto14;
			IsHitTestVisible = false;

			for (var i = 0; i < MaxMessageCount; ++i)
				Add(new Label { Visibility = Visibility.Collapsed, Margin = new Thickness(2), TextWrapping = TextWrapping.Wrap });
		}

		public void Update()
		{
			for (var i = 0; i < _messageCount; ++i)
			{
				var timedOut = _removalTimes[i] <= Clock.GetTime();
				if (!timedOut)
					continue;

				// Remove the label, add it to the end and hide it
				var child = Children[i];
				child.Visibility = Visibility.Collapsed;

				Children.RemoveAt(i);
				Add(child);

				// Remove the removal time
				if (i != _messageCount)
					Array.Copy(_removalTimes, i + 1, _removalTimes, i, _messageCount - i - 1);

				--_messageCount;
				--i;
			}
		}

		public void ClearEvents()
		{
			_messageCount = 0;

			foreach (var child in Children)
				child.Visibility = Visibility.Collapsed;
		}

		public void AddEvent(string message, bool isChatMessage = false)
		{
			if (_messageCount == MaxMessageCount)
			{
				// If we're out of labels, remove the top-most one, hide it, and add it back at the end
				var label = Children[0];
				label.Visibility = Visibility.Collapsed;

				Children.RemoveAt(0);
				Add(label);

				// Also, shift down the removal times
				Array.Copy(_removalTimes, 1, _removalTimes, 0, MaxMessageCount - 1);

				--_messageCount;
			}

			Children[_messageCount].Visibility = Visibility.Visible;
			((Label)Children[_messageCount]).Text = message;
			_removalTimes[_messageCount] = Clock.GetTime() + (isChatMessage ? Cvars.ChatMessageDisplayTime : Cvars.EventMessageDisplayTime);
			++_messageCount;
		}
	}
}