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
	using Platform.Input;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///   Binds an instruction to a logical input. Whenever the input is triggered, the instruction is executed.
	/// </summary>
	internal struct Binding
	{
		/// <summary>
		///   The error message that was generated while parsing the command.
		/// </summary>
		private readonly string _errorMessage;

		/// <summary>
		///   The instruction that is executed when the input is triggered.
		/// </summary>
		private Instruction _instruction;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The input that triggers the binding.</param>
		/// <param name="input">The input that should trigger the execution of the instruction.</param>
		/// <param name="command">The command string.</param>
		/// <param name="instruction">The instruction that should be executed when the input is triggered.</param>
		public Binding(ConfigurableInput trigger, LogicalInput input, string command, Instruction instruction)
			: this()
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentNotNullOrWhitespace(command, nameof(command));

			Trigger = trigger;
			Input = input;
			Command = command;
			_instruction = instruction;
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="trigger">The input that triggers the binding.</param>
		/// <param name="input">The input that should trigger the execution of the instruction.</param>
		/// <param name="command">The command string.</param>
		/// <param name="errorMessage">The error message that was generated while parsing the command.</param>
		public Binding(ConfigurableInput trigger, LogicalInput input, string command, string errorMessage)
			: this()
		{
			Assert.ArgumentNotNull(input, nameof(input));
			Assert.ArgumentNotNullOrWhitespace(command, nameof(command));
			Assert.ArgumentNotNullOrWhitespace(errorMessage, nameof(errorMessage));

			Trigger = trigger;
			Input = input;
			Command = command;
			_errorMessage = errorMessage;
		}

		/// <summary>
		///   Gets the input that triggers the binding.
		/// </summary>
		public ConfigurableInput Trigger { get; }

		/// <summary>
		///   The original command string.
		/// </summary>
		public string Command { get; }

		/// <summary>
		///   The input that triggers the execution of the instruction.
		/// </summary>
		public LogicalInput Input { get; }

		/// <summary>
		///   Executes the user request if the input has been triggered.
		/// </summary>
		public void ExecuteIfTriggered()
		{
			if (Input.IsTriggered && _errorMessage == null)
				_instruction.Execute(true);
			else if (Input.IsTriggered)
				Log.Error("Error while parsing the command '{0}':\n{1}", Command, _errorMessage);
		}
	}
}