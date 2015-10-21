﻿// The MIT License (MIT)
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
	using System;
	using System.Collections.Generic;
	using Parsing;
	using Platform.Logging;
	using Utilities;

	/// <summary>
	///   Represents a command with one parameter.
	/// </summary>
	public sealed class Command<T> : ICommand
	{
		/// <summary>
		///   The representation the command's parameter.
		/// </summary>
		private readonly CommandParameter _parameter;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="name">The external name of the command that is used to refer to the command in the console, for instance.</param>
		/// <param name="description">A string describing the usage and the purpose of the command.</param>
		/// <param name="systemOnly">Indicates whether the command can only be invoked by the system and not via the console.</param>
		/// <param name="parameter">The representation the command's parameter.</param>
		public Command(string name, string description, bool systemOnly, CommandParameter parameter)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			Assert.ArgumentNotNullOrWhitespace(description, nameof(description));

			Name = name;
			Description = description;
			SystemOnly = systemOnly;

			_parameter = parameter;
		}

		/// <summary>
		///   Gets a value indicating whether the command can only be invoked by the system and not via the console.
		/// </summary>
		public bool SystemOnly { get; }

		/// <summary>
		///   Gets the command's parameters.
		/// </summary>
		public IEnumerable<CommandParameter> Parameters
		{
			get { yield return _parameter; }
		}

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
			Assert.ArgumentSatisfies(parameters.Length == 1, nameof(parameters), "Argument count mismatch.");

			if (userInvoked && SystemOnly)
				Log.Warn("'{0}' can only be invoked by the application.", Name);
			else
				Invoke((T)parameters[0]);
		}

		/// <summary>
		///   Parses the next value in the input stream that corresponds to the type of the command's parameter with the given index.
		/// </summary>
		/// <param name="inputStream">The input stream the value should be parsed from.</param>
		/// <param name="parameterIndex">The zero-based command parameter index.</param>
		public object Parse(InputStream inputStream, int parameterIndex)
		{
			switch (parameterIndex)
			{
				case 0:
					return TypeRegistry.GetParser<T>()(inputStream);
				default:
					Assert.NotReached("Invalid parameter index.");
					return null;
			}
		}

		/// <summary>
		///   Invokes the command.
		/// </summary>
		/// <param name="value">The value of the parameter of the command.</param>
		public void Invoke(T value)
		{
			if (!_parameter.Validate(value))
			{
				Log.Info("{0}", Help.GetHint(Name));
				return;
			}

			Invoked?.Invoke(value);
		}

		/// <summary>
		///   Raised when the command has been invoked.
		/// </summary>
		public event Action<T> Invoked;
	}
}