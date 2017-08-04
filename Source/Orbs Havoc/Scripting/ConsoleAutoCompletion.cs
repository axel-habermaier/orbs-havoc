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