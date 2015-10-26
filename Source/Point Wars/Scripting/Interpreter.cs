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

namespace PointWars.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Parsing;
	using Platform;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Interprets user-provided input to set and view cvars and invoke commands.
	/// </summary>
	internal class Interpreter : DisposableObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Interpreter()
		{
			Commands.OnExecute += OnExecute;
			Commands.OnProcess += OnProcess;
			Commands.OnListCommands += OnListCommands;
			Commands.OnListCvars += OnListCvars;
			Commands.OnReset += OnResetCvar;
			Commands.OnPrintAppInfo += OnPrintAppInfo;
			Commands.OnToggle += OnToggle;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnExecute -= OnExecute;
			Commands.OnProcess -= OnProcess;
			Commands.OnListCommands -= OnListCommands;
			Commands.OnListCvars -= OnListCvars;
			Commands.OnReset -= OnResetCvar;
			Commands.OnPrintAppInfo -= OnPrintAppInfo;
			Commands.OnToggle -= OnToggle;
		}

		/// <summary>
		///   Toggles the value of a Boolean console variable.
		/// </summary>
		/// <param name="name">The name of the console variable whose value should be toggled.</param>
		private static void OnToggle(string name)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));

			ICvar cvar;
			if (!Cvars.TryFind(name, out cvar))
				Log.Warn("Unknown cvar '{0}'.", name);
			else
			{
				if (cvar.ValueType != typeof(bool))
					Log.Warn("Cvar '{0}' is not of type {1}.", name, TypeRegistry.GetDescription<bool>());
				else
					cvar.SetValue(!(bool)cvar.Value, true);
			}
		}

		/// <summary>
		///   Prints information about the application.
		/// </summary>
		private static void OnPrintAppInfo()
		{
			var builder = new StringBuilder();
			builder.AppendFormat("\nApplication Name:     {0}\n", Application.Name);
			builder.AppendFormat("Operating System:     {0}\n", PlatformInfo.Platform);
			builder.AppendFormat("User File Directory:  {0}\n", FileSystem.UserDirectory);

			Log.Info("{0}", builder);
		}

		/// <summary>
		///   Resets the cvar with the given name to its default value.
		/// </summary>
		/// <param name="name">The name of the cvar that should be reset to its default value.</param>
		private static void OnResetCvar(string name)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));

			ICvar cvar;
			if (!Cvars.TryFind(name, out cvar))
				Log.Warn("Unknown cvar '{0}'.", name);
			else
				cvar.SetValue(cvar.DefaultValue, true);
		}

		/// <summary>
		///   Executes the given user-provided input.
		/// </summary>
		/// <param name="input">The input that should be executed.</param>
		private void OnExecute(string input)
		{
			Assert.ArgumentNotNull(input, nameof(input));

			if (String.IsNullOrWhiteSpace(input))
				return;

			try
			{
				Parser.ParseInstruction(new InputStream(input)).Execute(executedByUser: true);
			}
			catch (ParseException e)
			{
				Log.Error("{0}", e.Message);
			}
		}

		/// <summary>
		///   Invoked when the commands in the given file should be processed.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be processed.</param>
		private static void OnProcess(string fileName)
		{
			ConfigurationFile.Process(fileName, executedByUser: true);
		}

		/// <summary>
		///   Invoked when all commands with a matching name should be listed.
		/// </summary>
		/// <param name="pattern">The name pattern of the commands that should be listed.</param>
		private static void OnListCommands(string pattern)
		{
			ListElements(Commands.All, pattern, command => command.Name, command => command.Description);
		}

		/// <summary>
		///   Invoked when all cvars with a matching name should be listed.
		/// </summary>
		/// <param name="pattern">The name pattern of the cvars that should be listed.</param>
		private static void OnListCvars(string pattern)
		{
			ListElements(Cvars.All, pattern, cvar => cvar.Name, cvar => cvar.Description);
		}

		/// <summary>
		///   Lists all elements matching the given predicate.
		/// </summary>
		/// <typeparam name="T">The type of the elements that should be shown.</typeparam>
		/// <param name="source">The elements that should be shown.</param>
		/// <param name="pattern">The pattern that should be checked.</param>
		/// <param name="name">A selector that returns the name of an element.</param>
		/// <param name="description">A selector that returns the description of an element.</param>
		private static void ListElements<T>(IEnumerable<T> source, string pattern, Func<T, string> name, Func<T, string> description)
		{
			Assert.ArgumentNotNull(source, nameof(source));
			Assert.ArgumentNotNull(pattern, nameof(pattern));
			Assert.ArgumentNotNull(name, nameof(name));
			Assert.ArgumentNotNull(description, nameof(description));

			var elements = PatternMatches(source, name, pattern).ToArray();
			if (elements.Length == 0)
			{
				Log.Warn("No elements found matching search pattern '{0}'.", pattern);
				return;
			}

			var builder = new StringBuilder();
			builder.Append("\n");
			foreach (var element in elements)
				builder.AppendFormat("'\\lightgrey{0}\\default': {1}\n", name(element), description(element));

			Log.Info("{0}", builder);
		}

		/// <summary>
		///   Returns an ordered sequence of all elements of the source sequence, whose selected property matches the given
		///   pattern.
		/// </summary>
		/// <typeparam name="T">The type of the items that should be checked.</typeparam>
		/// <param name="source">The items that should be checked.</param>
		/// <param name="selector">The selector function that selects the item property that should be used for pattern matching.</param>
		/// <param name="pattern">The pattern that should be checked.</param>
		private static IEnumerable<T> PatternMatches<T>(IEnumerable<T> source, Func<T, string> selector, string pattern)
		{
			Assert.ArgumentNotNull(source, nameof(source));
			Assert.ArgumentNotNull(selector, nameof(selector));
			Assert.ArgumentNotNullOrWhitespace(pattern, nameof(pattern));

			if (pattern.Trim() == "*")
				return source.OrderBy(selector);

			return source.Where(item => selector(item).ToLower().Contains(pattern.ToLower())).OrderBy(selector);
		}
	}
}