namespace OrbsHavoc.Scripting
{
	using System;
	using System.Linq;
	using Utilities;

	internal sealed class ConsoleAutoCompletion
	{
		private int _autoCompletionIndex;
		private string[] _autoCompletionList;

		public event Action<string> InputAutoCompleted;

		public void Reset()
		{
			_autoCompletionList = null;
		}

		public void Next(string input)
		{
			AutoComplete(input, next: true);
		}

		public void Previous(string input)
		{
			AutoComplete(input, next: false);
		}

		private void AutoComplete(string input, bool next)
		{
			if (_autoCompletionList == null)
			{
				_autoCompletionList = GetAutoCompletionList(input.ToLower());

				// If auto-completion returned no results, we're done here
				if (_autoCompletionList == null)
					return;

				_autoCompletionIndex = next ? 0 : _autoCompletionList.Length - 1;
			}
			else
			{
				_autoCompletionIndex = (_autoCompletionIndex + (next ? 1 : -1)) % _autoCompletionList.Length;
				if (_autoCompletionIndex < 0)
					_autoCompletionIndex += _autoCompletionList.Length;
			}

			InputAutoCompleted?.Invoke(_autoCompletionList[_autoCompletionIndex] + " ");
		}

		private static string[] GetAutoCompletionList(string input)
		{
			if (TextString.IsNullOrWhiteSpace(input))
				return null;

			var commands = Commands.All.Where(c => c.Name.ToLower().StartsWith(input)).Select(c => c.Name);
			var cvars = Cvars.All.Where(c => c.Name.ToLower().StartsWith(input)).Select(c => c.Name);

			var list = cvars.Union(commands).OrderBy(item => item).ToArray();
			if (list.Length == 0)
				return null;

			return list;
		}
	}
}