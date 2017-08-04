namespace OrbsHavoc.Scripting
{
	using System.Linq;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///   Represents an interpreted instruction that invokes a command, sets a cvar, or displays the value of a cvar.
	/// </summary>
	internal struct Instruction
	{
		/// <summary>
		///   The parameter of the instruction.
		/// </summary>
		private readonly object _parameter;

		/// <summary>
		///   The target of the instruction.
		/// </summary>
		private readonly object _target;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="target">The target of the instruction.</param>
		/// <param name="parameter">The parameter of the instruction.</param>
		public Instruction(object target, object parameter)
		{
			Assert.ArgumentNotNull(target, nameof(target));
			Assert.That(!(target is ICommand) || (parameter is object[] && ((object[])parameter).Length == ((ICommand)target).Parameters.Count()),
				"Incorrect command parameters.");

			_target = target;
			_parameter = parameter;
		}

		/// <summary>
		///   Gets a value indicating whether the instruction invokes a command.
		/// </summary>
		public bool IsCommandInvocation => _target is ICommand;

		/// <summary>
		///   Executes the instruction.
		/// </summary>
		/// <param name="executedByUser">If true, indicates that the instruction originates from the user (e.g., via the console).</param>
		public void Execute(bool executedByUser)
		{
			var command = _target as ICommand;
			var cvar = _target as ICvar;

			command?.Invoke((object[])_parameter, executedByUser);

			if (cvar != null && _parameter == null)
			{
				Log.Info($"'{cvar.Name}' is '{TypeRegistry.ToString(cvar.Value)}\\default', default " +
						 $"'{TypeRegistry.ToString(cvar.DefaultValue)}\\default'.");
			}

			if (cvar != null && _parameter != null)
				cvar.SetValue(_parameter, executedByUser);
		}
	}
}