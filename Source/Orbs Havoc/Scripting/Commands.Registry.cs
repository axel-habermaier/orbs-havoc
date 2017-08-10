namespace OrbsHavoc.Scripting
{
	using System.Collections.Generic;
	using System.Linq;
	using Utilities;

	/// <summary>
	///   Provides access to all commands.
	/// </summary>
	internal static partial class Commands
	{
		/// <summary>
		///   The registered commands.
		/// </summary>
		private static readonly Dictionary<string, ICommand> _registeredCommands = new Dictionary<string, ICommand>();

		/// <summary>
		///   Gets all registered commands.
		/// </summary>
		internal static IEnumerable<ICommand> All => _registeredCommands.Values;

		/// <summary>
		///   Registers the given command.
		/// </summary>
		/// <param name="command">The command that should be registered.</param>
		private static void Register(ICommand command)
		{
			Assert.ArgumentNotNull(command, nameof(command));
			Assert.NotNullOrWhitespace(command.Name, "The command cannot have an empty name.");
			Assert.That(!_registeredCommands.ContainsKey(command.Name), $"A command with the name '{command.Name}' has already been registered.");
			Assert.That(Cvars.All.All(cvar => cvar.Name != command.Name),
				$"A cvar with the name '{command.Name}' has already been registered.");

			_registeredCommands.Add(command.Name, command);
		}

		/// <summary>
		///   Finds the command with the given name. Returns false if no such command is found.
		/// </summary>
		/// <param name="name">The name of the command that should be returned.</param>
		/// <param name="command">The command with the given name, if it is found.</param>
		internal static bool TryFind(string name, out ICommand command)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			return _registeredCommands.TryGetValue(name, out command);
		}
	}
}