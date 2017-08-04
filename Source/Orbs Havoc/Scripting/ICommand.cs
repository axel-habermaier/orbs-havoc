namespace OrbsHavoc.Scripting
{
	using System.Collections.Generic;
	using Parsing;

	/// <summary>
	///   A common interface for commands with zero or more parameters.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		///   Gets the external name of the command that is used to refer to the command in the console, for instance.
		/// </summary>
		string Name { get; }

		/// <summary>
		///   Gets a string describing the usage and the purpose of the command.
		/// </summary>
		string Description { get; }

		/// <summary>
		///   Gets the command's parameters.
		/// </summary>
		IEnumerable<CommandParameter> Parameters { get; }

		/// <summary>
		///   Gets a value indicating whether the command is hidden from the console.
		/// </summary>
		bool SystemOnly { get; }

		/// <summary>
		///   Invokes the command, extracting the command's parameters (if any) from the given parameters array.
		/// </summary>
		/// <param name="parameters">The parameters that should be used to invoke the command.</param>
		/// <param name="userInvoked">If true, indicates that the command was invoked by the user (e.g., via the console).</param>
		void Invoke(object[] parameters, bool userInvoked);

		/// <summary>
		///   Parses the next value in the input stream that corresponds to the type of the command's parameter with the given index.
		/// </summary>
		/// <param name="inputStream">The input stream the value should be parsed from.</param>
		/// <param name="parameterIndex">The zero-based command parameter index.</param>
		object Parse(InputStream inputStream, int parameterIndex);
	}
}