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
	using System.Text;
	using Platform.Logging;
	using Platform.Memory;
	using Utilities;

	/// <summary>
	///   Explains the usage of the cvar and command system.
	/// </summary>
	internal class Help : DisposableObject
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		public Help()
		{
			Commands.OnHelp += OnHelp;
		}

		/// <summary>
		///   Invoked when a description of the cvar or command with the given name should be displayed.
		/// </summary>
		/// <param name="name">The name of the cvar or the command for which the help should be displayed.</param>
		private static void OnHelp(string name)
		{
			Assert.ArgumentNotNull(name, nameof(name));

			name = name.Trim();

			if (String.IsNullOrWhiteSpace(name))
				PrintHelp();
			else if (Cvars.TryFind(name, out var cvar))
				PrintCvarHelp(cvar);
			else if (Commands.TryFind(name, out var command))
				PrintCommandHelp(command);
			else
				Log.Error($"'{name}' is neither a cvar nor a command.");
		}

		/// <summary>
		///   Prints the help for the console system.
		/// </summary>
		private static void PrintHelp()
		{
			var builder = new StringBuilder();
			builder.Append("\nUse the console to view or set cvars and to invoke commands.\n");
			builder.Append("Cvars:\n");
			builder.Append("   Type '\\lightgrey<cvar-name>\\default' to view the current value of the cvar.\n");
			builder.Append("   Type '\\lightgrey<cvar-name> <value>\\default' to set a cvar to a new value.\n");
			builder.Append("   Type '\\lightgreyhelp <cvar-name>\\default' to view a description of the usage and purpose of the cvar.\n");
			builder.Append("   Type '\\lightgreylist_cvars\\default' to list all available cvars.\n");
			builder.Append("Commands:\n");
			builder.Append(
				"   Type '\\lightgrey<command-name> <value1> <value2> ...\\default' to invoke the command with parameters value1, value2, ... " +
				"Optional parameters can be omitted at the end of the command invocation.\n");
			builder.Append("   Type '\\lightgreyhelp <command-name>\\default' to view a description of the usage and purpose of the command.\n");
			builder.Append("   Type '\\lightgreylist_commands\\default' to list all available commands.\n");

			Log.Info(builder.ToString());
		}

		/// <summary>
		///   Prints the help for the given cvar.
		/// </summary>
		/// <param name="cvar">The cvar the help should be printed for.</param>
		private static void PrintCvarHelp(ICvar cvar)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("\nCvar:          {0}\n", cvar.Name);
			builder.AppendFormat("Description:   {0}\n", cvar.Description);
			builder.AppendFormat("Type:          {0} (e.g., {1})\n", TypeRegistry.GetDescription(cvar.ValueType),
				String.Join(", ", TypeRegistry.GetExamples(cvar.ValueType)));
			builder.AppendFormat("Default Value: {0}\\default\n", TypeRegistry.ToString(cvar.DefaultValue));
			builder.AppendFormat("Current Value: {0}\\default\n", TypeRegistry.ToString(cvar.Value));

			if (cvar.Validators.Any())
				builder.AppendFormat("Remarks:       {0}\n", String.Join("; ", cvar.Validators.Select(v => v.Description)));

			builder.AppendFormat("Persistent:    {0}\n", cvar.Persistent ? "yes" : "no");
			builder.AppendFormat("User Access:   {0}\n", cvar.SystemOnly ? "read" : "read/write");

			Log.Info(builder.ToString());
		}

		/// <summary>
		///   Prints the help for the given command.
		/// </summary>
		/// <param name="command">The command the help should be printed for.</param>
		private static void PrintCommandHelp(ICommand command)
		{
			var builder = new StringBuilder();
			builder.AppendFormat("\nCommand:     {0}\n", command.Name);
			builder.AppendFormat("Description: {0}\n", command.Description);
			builder.AppendFormat("Invocation:  {0}", command.SystemOnly ? "application only" : "user or application");

			if (command.Parameters.Any())
				builder.Append("\n\nParameters:\n");

			var first = true;
			foreach (var parameter in command.Parameters)
			{
				if (first)
					first = false;
				else
					builder.Append("\n\n");

				builder.AppendFormat("    Parameter:     {0}\n", parameter.Name);
				builder.AppendFormat("    Description:   {0}\n", parameter.Description);
				builder.AppendFormat("    Type:          {0} (e.g., {1})", TypeRegistry.GetDescription(parameter.Type),
					String.Join(", ", TypeRegistry.GetExamples(parameter.Type)));

				if (parameter.Validators.Any())
					builder.AppendFormat("\n    Remarks:       {0}", String.Join("; ", parameter.Validators.Select(v => v.Description)));

				if (parameter.HasDefaultValue)
					builder.AppendFormat("\n    Default Value: {0}", TypeRegistry.ToString(parameter.DefaultValue));
			}

			builder.Append("\n");
			Log.Info(builder.ToString());
		}

		/// <summary>
		///   Gets a string that contains a hint about the usage of help.
		/// </summary>
		/// <param name="name">The cvar or command name that should be used in the hint.</param>
		public static string GetHint(string name)
		{
			return String.Format("Use 'help {0}' for details about the usage of cvar or command '{0}'.", name);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			Commands.OnHelp -= OnHelp;
		}
	}
}