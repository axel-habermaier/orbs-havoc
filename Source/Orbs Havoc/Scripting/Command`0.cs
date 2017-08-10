namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Parsing;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///   Represents a parameterless command.
	/// </summary>
	internal sealed class Command : ICommand
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="name">The external name of the command that is used to refer to the command in the console, for instance.</param>
		/// <param name="description">A string describing the usage and the purpose of the command.</param>
		/// <param name="systemOnly">Indicates whether the command can only be invoked by the system and not via the console.</param>
		public Command(string name, string description, bool systemOnly)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			Assert.ArgumentNotNullOrWhitespace(description, nameof(description));

			Name = name;
			Description = description;
			SystemOnly = systemOnly;
		}

		/// <summary>
		///   Gets a value indicating whether the command can only be invoked by the system and not via the console.
		/// </summary>
		public bool SystemOnly { get; }

		/// <summary>
		///   Gets the command's parameters.
		/// </summary>
		public IEnumerable<CommandParameter> Parameters => Enumerable.Empty<CommandParameter>();

		/// <summary>
		///   Gets the external name of the command that is used to refer to the command in the console, for instance.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///   Gets a string describing the usage and the purpose of the command.
		/// </summary>
		public string Description { get; }

		/// <summary>
		///   Invokes the command, extracting the command's parameters (if any) from the given parameters array.
		/// </summary>
		/// <param name="parameters">The parameters that should be used to invoke the command.</param>
		/// <param name="userInvoked">If true, indicates that the command was invoked by the user (e.g., via the console).</param>
		void ICommand.Invoke(object[] parameters, bool userInvoked)
		{
			Assert.ArgumentNotNull(parameters, nameof(parameters));
			Assert.ArgumentSatisfies(parameters.Length == 0, nameof(parameters), "Argument count mismatch.");

			if (userInvoked && SystemOnly)
				Log.Warn($"'{Name}' can only be invoked by the application.");
			else
				Invoke();
		}

		/// <summary>
		///   Parses the next value in the input stream that corresponds to the type of the command's parameter with the given index.
		/// </summary>
		/// <param name="inputStream">The input stream the value should be parsed from.</param>
		/// <param name="parameterIndex">The zero-based command parameter index.</param>
		public object Parse(InputStream inputStream, int parameterIndex)
		{
			Assert.NotReached("The command does not have any parameters.");
			return null;
		}

		/// <summary>
		///   Invokes the command.
		/// </summary>
		public void Invoke()
		{
			Invoked?.Invoke();
		}

		/// <summary>
		///   Raised when the command has been invoked.
		/// </summary>
		public event Action Invoked;
	}
}