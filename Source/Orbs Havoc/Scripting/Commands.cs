﻿namespace OrbsHavoc.Scripting
{
	using Network;
	using UserInterface.Input;
	using Validators;

	/// <summary>
	///   Declares the commands required by the framework.
	/// </summary>
	internal interface ICommands
	{
		/// <summary>
		///   Immediately exits the application.
		/// </summary>
		void Exit();

		/// <summary>
		///   Describes the usage and the purpose of the the cvar or command with the given name. If no name is given, displays a
		///   help text about the usage of cvars and commands in general.
		/// </summary>
		/// <param name="name">The name of the cvar or the command for which the description should be displayed.</param>
		void Help(string name = "");

		/// <summary>
		///   Resets the given cvar to its default value.
		/// </summary>
		/// <param name="cvar">The name of the cvar that should be reset to its default value.</param>
		void Reset([NotEmpty] string cvar);

		/// <summary>
		///   Lists all cvars with names that match the search pattern.
		/// </summary>
		/// <param name="pattern">
		///   The search pattern of the cvars that should be listed. For instance, "draw" lists all cvars that have the string
		///   "draw" in their name. The pattern is case-insensitive; use "*" to list all cvars.
		/// </param>
		void ListCvars([NotEmpty] string pattern = "*");

		/// <summary>
		///   Lists all commands with names that match the search pattern.
		/// </summary>
		/// <param name="pattern">
		///   The search pattern of the commands that should be listed. For instance, "draw" lists all commands that have the
		///   string "draw" in their name. The pattern is case-insensitive; use "*" to list all commands.
		/// </param>
		void ListCommands([NotEmpty] string pattern = "*");

		/// <summary>
		///   Executes the given command.
		/// </summary>
		/// <param name="command">The command that should be executed, including its arguments.</param>
		void Execute([NotEmpty] string command);

		/// <summary>
		///   Processes the commands in the given file.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be processed.</param>
		void Process([NotEmpty, FileName] string fileName);

		/// <summary>
		///   Binds a command invocation to a logical input. Whenever the input is triggered, the command is invoked with the
		///   specified arguments.
		/// </summary>
		/// <param name="trigger">The input that triggers the command.</param>
		/// <param name="command">The command (including the arguments) that should be executed when the trigger is fired.</param>
		void Bind(InputTrigger trigger, [NotEmpty] string command);

		/// <summary>
		///   Unbinds all commands currently bound to a logical input.
		/// </summary>
		/// <param name="trigger">The input that should be unbound.</param>
		void Unbind(InputTrigger trigger);

		/// <summary>
		///   Removes all command bindings.
		/// </summary>
		void UnbindAll();

		/// <summary>
		///   Lists all active bindings.
		/// </summary>
		void ListBindings();

		/// <summary>
		///   Shows or hides the console.
		/// </summary>
		/// <param name="show">A value of 'true' indicates that the console should be shown.</param>
		void ShowConsole(bool show);

		/// <summary>
		///   Reloads all currently loaded assets.
		/// </summary>
		void ReloadAssets();

		/// <summary>
		///   Toggles the value of a Boolean console variable.
		/// </summary>
		/// <param name="cvar">The name of console variable whose value should be toggled.</param>
		void Toggle([NotEmpty] string cvar);

		/// <summary>
		///   Starts up a new local server instance. If a local server is currently running, it is shut down before the new server
		///   is started.
		/// </summary>
		/// <param name="serverName">The name of the server that is displayed in the Join screen.</param>
		/// <param name="port">The port the server should use to communicate with the clients.</param>
		void StartServer([MaximumLength(NetworkProtocol.ServerNameLength, checkUtf8Length: true)] string serverName = "",
						 ushort port = NetworkProtocol.DefaultServerPort);

		/// <summary>
		///   Shuts down the currently running server.
		/// </summary>
		void StopServer();

		/// <summary>
		///   Connects to a game session on a remote or local server.
		/// </summary>
		/// <param name="serverAddress">The IP address or name of the server.</param>
		/// <param name="port">The port of the server.</param>
		void Connect([NotEmpty] string serverAddress, ushort port = NetworkProtocol.DefaultServerPort);

		/// <summary>
		///   Disconnects from the current game session.
		/// </summary>
		void Disconnect();

		/// <summary>
		///   Sends a chat message to all peers.
		/// </summary>
		/// <param name="message">The message that should be sent.</param>
		void Say([NotEmpty, MaximumLength(NetworkProtocol.ChatMessageLength, checkUtf8Length: true)] string message);

		/// <summary>
		///   Adds a bot to the currently active, locally-hosted game session.
		/// </summary>
		void AddBot();

		/// <summary>
		///   Removes a bot from the currently active, locally-hosted game session.
		/// </summary>
		void RemoveBot();
	}
}