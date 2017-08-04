namespace OrbsHavoc.Scripting
{
	using System;
	using System.Linq;
	using Parsing;
	using Platform.Logging;

	/// <summary>
	///   Parses and executes the command line arguments, consisting of potentially multiple instructions. For instance, the
	///   command line "/time_scale 0.01" sets the value of the 'time_scale' cvar to 0.01.
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
					Log.Error($"Encountered unexpected token '{arguments[i]}\\default' in command line at position {i}. " +
							  $"Expected the name of a cvar or command, prefixed with '{ArgumentSeparator}'.");

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
					Log.Error(e.Message);
				}

				i = position;
			}
		}
	}
}