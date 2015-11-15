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

namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using JetBrains.Annotations;
	using Platform.Logging;
	using Utilities;
	using Validators;

	/// <summary>
	///   Represents a parameter of a command.
	/// </summary>
	public struct CommandParameter
	{
		/// <summary>
		///   The validators that are used to validate a parameter value.
		/// </summary>
		private readonly ValidatorAttribute[] _validators;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="type">The type of the parameter.</param>
		/// <param name="hasDefaultValue">Indicates whether the parameter has a default value.</param>
		/// <param name="defaultValue">The default value that should be used if no value has been specified.</param>
		/// <param name="description">The description of the parameter.</param>
		/// <param name="validators">The validators that should be used to validate a parameter value.</param>
		public CommandParameter(string name, Type type, bool hasDefaultValue, object defaultValue, string description,
								params ValidatorAttribute[] validators)
			: this()
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			Assert.ArgumentNotNull(type, nameof(type));
			Assert.ArgumentNotNullOrWhitespace(description, nameof(description));

			Name = name;
			Type = type;
			HasDefaultValue = hasDefaultValue;
			DefaultValue = defaultValue;
			Description = description;
			_validators = validators;
		}

		/// <summary>
		///   Gets the validators that are used to validate the values of the parameter.
		/// </summary>
		public IEnumerable<ValidatorAttribute> Validators => _validators;

		/// <summary>
		///   Gets the name of the parameter.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///   Gets the type of the parameter.
		/// </summary>
		public Type Type { get; }

		/// <summary>
		///   Gets the default value that is used if no value has been specified.
		/// </summary>
		public object DefaultValue { get; }

		/// <summary>
		///   Gets a value indicating whether the parameter has a default value.
		/// </summary>
		public bool HasDefaultValue { get; }

		/// <summary>
		///   Gets the description of the parameter.
		/// </summary>
		public string Description { get; }

		/// <summary>
		///   Validates the given value.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		[Pure]
		internal bool Validate(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));

			foreach (var validator in _validators)
			{
				if (validator.Validate(value))
					continue;

				Log.Error("'{0}' is not a valid value for parameter '{1}': {2}", value, Name, validator.ErrorMessage);
				return false;
			}

			return true;
		}
	}
}