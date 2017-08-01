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
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Parsing;
	using Platform;
	using Platform.Logging;

	/// <summary>
	///   Represents a configuration file that can be processed by the application, changed, and/or written to disk.
	/// </summary>
	internal static class ConfigurationFile
	{
		/// <summary>
		///   The name of the automatically executed configuration file.
		/// </summary>
		public const string AutoExec = "autoexec.cfg";

		/// <summary>
		///   Checks whether the given file name is valid.
		/// </summary>
		/// <param name="fileName">The name of the configuration file.</param>
		private static bool IsValidFileName(string fileName)
		{
			if (String.IsNullOrWhiteSpace(fileName))
			{
				Log.Error("The file name cannot consist of whitespace only.");
				return false;
			}

			if (FileSystem.IsValidFileName(fileName))
				return true;

			Log.Error($"'{fileName}' is not a valid file name.");
			return false;
		}

		/// <summary>
		///   Parses the individual instructions contained in the configuration file.
		/// </summary>
		private static IEnumerable<Instruction> ParseInstructions(string fileName)
		{
			string input;
			try
			{
				input = FileSystem.ReadAllText(fileName);
			}
			catch (FileSystemException e)
			{
				Log.Error($"Unable to read '{fileName}': {e.Message}");
				yield break;
			}

			var lines = input.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				Instruction? instruction = null;
				try
				{
					instruction = Parser.ParseInstruction(new InputStream(line));
				}
				catch (ParseException e)
				{
					Log.Error(e.Message);
				}

				if (instruction is Instruction value)
					yield return value;
			}
		}

		/// <summary>
		///   Processes all set cvar and command invocation user requests stored in the configuration file.
		/// </summary>
		/// <param name="fileName">The name of the file that should be processed.</param>
		/// <param name="executedByUser">Indicates whether the file is processed because of a user action.</param>
		public static void Process(string fileName, bool executedByUser)
		{
			if (!IsValidFileName(fileName))
				return;

			Log.Info($"Processing '{fileName}'...");
			foreach (var instruction in ParseInstructions(fileName))
				instruction.Execute(executedByUser);
		}

		/// <summary>
		///   Persists the values of all persistent cvars to the autoexec.cfg file.
		/// </summary>
		public static void WriteAutoExec()
		{
			var builder = new StringBuilder();

			foreach (var cvar in Cvars.All.Where(cvar => cvar.Persistent))
			{
				var value = TypeRegistry.ToString(cvar.Value);

				// If the cvar is of type string, escape all quotes and enclose the given string in quotes to ensure that the string 
				// can later be parsed again
				if (cvar.ValueType == typeof(string))
					value = "\"" + value.Replace("\"", "\\\"") + "\"";

				builder.AppendLine($"{cvar.Name} {value}");
			}

			try
			{
				FileSystem.WriteAllText(AutoExec, builder.ToString());
				Log.Info($"'{AutoExec}' has been written.");
			}
			catch (FileSystemException e)
			{
				Log.Error($"Failed to write '{AutoExec}': {e.Message}");
			}
		}
	}
}