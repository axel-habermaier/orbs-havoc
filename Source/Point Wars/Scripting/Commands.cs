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
	using System.Net;
	using Network;
	using Platform.Input;
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
		///   Prints information about the application.
		/// </summary>
		void PrintAppInfo();

		/// <summary>
		///   Binds a command invocation to a logical input. Whenever the input is triggered, the command is invoked with the
		///   specified arguments.
		/// </summary>
		/// <param name="trigger">The input that triggers the command.</param>
		/// <param name="command">The command (including the arguments) that should be executed when the trigger is fired.</param>
		void Bind(ConfigurableInput trigger, [NotEmpty] string command);

		/// <summary>
		///   Unbinds all commands currently bound to a logical input.
		/// </summary>
		/// <param name="trigger">The input that should be unbound.</param>
		void Unbind(ConfigurableInput trigger);

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
		void StartServer([NotEmpty, MaximumLength(NetworkProtocol.ServerNameLength, checkUtf8Length: true)] string serverName = "Server",
						 ushort port = NetworkProtocol.DefaultServerPort);

		/// <summary>
		///   Shuts down the currently running server.
		/// </summary>
		void StopServer();

		/// <summary>
		///   Connects to a game session on a remote or local server.
		/// </summary>
		/// <param name="ipAddress">The IP address of the server.</param>
		/// <param name="port">The port of the server.</param>
		void Connect(IPAddress ipAddress, ushort port = NetworkProtocol.DefaultServerPort);

		/// <summary>
		///   Disconnects from the current game session.
		/// </summary>
		void Disconnect();

		/// <summary>
		///   Sends a chat message to all peers.
		/// </summary>
		/// <param name="message">The message that should be sent.</param>
		void Say([NotEmpty, MaximumLength(NetworkProtocol.ChatMessageLength, checkUtf8Length: true)] string message);
	}
}