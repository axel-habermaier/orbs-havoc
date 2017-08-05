namespace OrbsHavoc.Scripting
{
	using Platform.Input;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///     Binds an instruction to a logical input. Whenever the input is triggered, the instruction is executed.
	/// </summary>
	internal struct Binding
	{
		/// <summary>
		///     The error message that was generated while parsing the command.
		/// </summary>
		private readonly string _errorMessage;

		/// <summary>
		///     The instruction that is executed when the input is triggered.
		/// </summary>
		private Instruction _instruction;

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The input that triggers the binding.</param>
		/// <param name="command">The command string.</param>
		/// <param name="instruction">The instruction that should be executed when the input is triggered.</param>
		public Binding(InputTrigger trigger, string command, Instruction instruction)
			: this()
		{
			Assert.ArgumentNotNullOrWhitespace(command, nameof(command));

			Trigger = trigger;
			Command = command;
			_instruction = instruction;
		}

		/// <summary>
		///     Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The input that triggers the binding.</param>
		/// <param name="command">The command string.</param>
		/// <param name="errorMessage">The error message that was generated while parsing the command.</param>
		public Binding(InputTrigger trigger, string command, string errorMessage)
			: this()
		{
			Assert.ArgumentNotNullOrWhitespace(command, nameof(command));
			Assert.ArgumentNotNullOrWhitespace(errorMessage, nameof(errorMessage));

			Trigger = trigger;
			Command = command;
			_errorMessage = errorMessage;
		}

		/// <summary>
		///     Gets the input that triggers the binding.
		/// </summary>
		public InputTrigger Trigger { get; }

		/// <summary>
		///     The original command string.
		/// </summary>
		public string Command { get; }

		/// <summary>
		///     Executes the user request if the input has been triggered.
		/// </summary>
		public void ExecuteIfTriggered(Keyboard keyboard, Mouse mouse)
		{
			Assert.ArgumentNotNull(keyboard, nameof(keyboard));
			Assert.ArgumentNotNull(mouse, nameof(mouse));

			var isTriggered = Trigger.IsTriggered(keyboard, mouse, TriggerType.WentDown);
			if (!isTriggered)
				return;

			if (_errorMessage == null)
				_instruction.Execute(true);
			else
				Log.Error($"Error while parsing the command '{Command}\\default':\n{_errorMessage}");
		}
	}
}