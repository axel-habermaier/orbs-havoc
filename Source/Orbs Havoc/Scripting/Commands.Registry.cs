// The MIT License (MIT)
// 
// Copyright (c) 2012-2016, Axel Habermaier
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
	using System.Collections.Generic;
	using System.Linq;
	using Utilities;

	/// <summary>
	///   Provides access to all commands.
	/// </summary>
	public static partial class Commands
	{
		/// <summary>
		///   The registered commands.
		/// </summary>
		private static readonly Dictionary<string, ICommand> RegisteredCommands = new Dictionary<string, ICommand>();

		/// <summary>
		///   Gets all registered commands.
		/// </summary>
		internal static IEnumerable<ICommand> All => RegisteredCommands.Values;

		/// <summary>
		///   Registers the given command.
		/// </summary>
		/// <param name="command">The command that should be registered.</param>
		private static void Register(ICommand command)
		{
			Assert.ArgumentNotNull(command, nameof(command));
			Assert.NotNullOrWhitespace(command.Name, "The command cannot have an empty name.");
			Assert.That(!RegisteredCommands.ContainsKey(command.Name), $"A command with the name '{command.Name}' has already been registered.");
			Assert.That(Cvars.All.All(cvar => cvar.Name != command.Name),
				$"A cvar with the name '{command.Name}' has already been registered.");

			RegisteredCommands.Add(command.Name, command);
		}

		/// <summary>
		///   Finds the command with the given name. Returns false if no such command is found.
		/// </summary>
		/// <param name="name">The name of the command that should be returned.</param>
		/// <param name="command">The command with the given name, if it is found.</param>
		internal static bool TryFind(string name, out ICommand command)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			return RegisteredCommands.TryGetValue(name, out command);
		}
	}
}