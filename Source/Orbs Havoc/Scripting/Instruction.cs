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