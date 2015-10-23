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
	using System.Linq;
	using Parsing;
	using Platform.Logging;

	/// <summary>
	///   Parses and executes the command line arguments, consisting of potentially multiple instructions. For instance, the
	///   command line "/time_scale 0.01" sets the value of the time scale cvar to 0.01.
	/// </summary>
	internal static class CommandLine
	{
		/// <summary>
		///   Separates different arguments on the command line.
		/// </summary>
		private const string ArgumentSeparator = "/";

		/// <summary>
		///   Process the instructions passed via the command line.
		/// </summary>
		/// <param name="arguments">The command line arguments that have been passed to the application.</param>
		public static void Process(string[] arguments)
		{
			if (arguments == null || arguments.Length == 0)
			{
				Log.Info("No command line arguments have been provided.");
				return;
			}

			Log.Info("Parsing command line arguments...");

			for (var i = 0; i < arguments.Length;)
			{
				// We require all cvar and command names to be prefixed with the argument separator.
				// A cvar or command is expected now, so if the current argument doesn't start with the separator,
				// report an error and try again for the next argument.
				if (!arguments[i].StartsWith(ArgumentSeparator))
				{
					Log.Error("Encountered unexpected token '{0}\\default' in command line at position {1}. " +
							  "Expected the name of a cvar or command, prefixed with '{2}'.", arguments[i], i, ArgumentSeparator);

					++i;
					continue;
				}

				// Look for the next command line argument starting with the separator; everything in-between belongs to
				// the current cvar or command
				var position = i + 1;
				while (position < arguments.Length && !arguments[position].StartsWith(ArgumentSeparator))
					++position;

				// Generate the string for the parser by concatenating all parts and removing the separator, then parse it
				var input = String.Join(" ", arguments.Skip(i).Take(position - i)).Substring(1);

				try
				{
					Parser.ParseInstruction(new InputStream(input)).Execute(executedByUser: false);
				}
				catch (ParseException e)
				{
					Log.Error("{0}", e.Message);
				}

				i = position;
			}
		}
	}
}