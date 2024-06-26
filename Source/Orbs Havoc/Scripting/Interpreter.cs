﻿namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Parsing;
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
			Commands.OnToggle -= OnToggle;
		}

		/// <summary>
		///   Toggles the value of a Boolean console variable.
		/// </summary>
		/// <param name="name">The name of the console variable whose value should be toggled.</param>
		private static void OnToggle(string name)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));

			if (!Cvars.TryFind(name, out var cvar))
				Log.Warn($"Unknown cvar '{name}'.");
			else
			{
				if (cvar.ValueType != typeof(bool))
					Log.Warn($"Cvar '{name}' is not of type {TypeRegistry.GetDescription<bool>()}.");
				else
					cvar.SetValue(!(bool)cvar.Value, true);
			}
		}

		/// <summary>
		///   Resets the cvar with the given name to its default value.
		/// </summary>
		/// <param name="name">The name of the cvar that should be reset to its default value.</param>
		private static void OnResetCvar(string name)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));

			if (!Cvars.TryFind(name, out var cvar))
				Log.Warn($"Unknown cvar '{name}'.");
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
				Log.Error(e.Message);
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

			if (pattern.Trim() != "*")
				source = source.Where(item => name(item).ToLower().Contains(pattern.ToLower()));

			var elements = source.OrderBy(name).ToArray();
			if (elements.Length == 0)
			{
				Log.Warn($"No elements found matching search pattern '{pattern}'.");
				return;
			}

			var builder = new StringBuilder();
			builder.Append("\n");
			foreach (var element in elements)
				builder.AppendFormat("'\\lightgrey{0}\\default': {1}\n", name(element), description(element));

			Log.Info(builder.ToString());
		}
	}
}