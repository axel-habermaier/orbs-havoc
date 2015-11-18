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

namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using Parsing;
	using Platform.Logging;
	using Utilities;
	using Validators;

	/// <summary>
	///   Represents a configurable value.
	/// </summary>
	/// <typeparam name="T">The type of the value.</typeparam>
	public sealed class Cvar<T> : ICvar
	{
		/// <summary>
		///   The default value of the cvar.
		/// </summary>
		private readonly T _defaultValue;

		/// <summary>
		///   The validators that are used to validate the values of the cvar.
		/// </summary>
		private readonly ValidatorAttribute[] _validators;

		/// <summary>
		///   The current value of the cvar.
		/// </summary>
		private T _value;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="name">The external name of the cvar.</param>
		/// <param name="defaultValue">The default value of the cvar.</param>
		/// <param name="description">A description of the cvar's purpose.</param>
		/// <param name="persistent">Indicates whether the cvar's value should be persisted across sessions.</param>
		/// <param name="systemOnly">Indicates whether the cvar can only be set by the system and not via the console.</param>
		/// <param name="validators">The validators that should be used to validate a new cvar value before it is set.</param>
		public Cvar(string name, T defaultValue, string description, bool persistent, bool systemOnly,
					params ValidatorAttribute[] validators)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			Assert.ArgumentNotNullOrWhitespace(description, nameof(description));

			Name = name;
			Description = description;
			Persistent = persistent;
			SystemOnly = systemOnly;

			_defaultValue = defaultValue;
			_value = defaultValue;
			_validators = validators;
		}

		/// <summary>
		///   Gets or sets the value of the cvar.
		/// </summary>
		public T Value
		{
			get { return _value; }
			set
			{
				if (!ValidateValue(value))
					return;

				UpdateValue(value);
			}
		}

		/// <summary>
		///   Gets a value indicating whether the cvar is readonly and cannot be set from the console.
		/// </summary>
		public bool SystemOnly { get; }

		/// <summary>
		///   Gets the validators that are used to validate the values of the cvar.
		/// </summary>
		public IEnumerable<ValidatorAttribute> Validators => _validators;

		/// <summary>
		///   Gets or sets the value of the cvar.
		/// </summary>
		object ICvar.Value => _value;

		/// <summary>
		///   Gets the cvar's default value.
		/// </summary>
		object ICvar.DefaultValue => _defaultValue;

		/// <summary>
		///   Indicates whether the cvar's value is persisted across sessions.
		/// </summary>
		public bool Persistent { get; }

		/// <summary>
		///   Gets the external name of the cvar that is used to refer to the cvar in the console, for instance.
		/// </summary>
		public string Name { get; }

		/// <summary>
		///   Gets a string describing the usage and the purpose of the cvar.
		/// </summary>
		public string Description { get; }

		/// <summary>
		///   Gets the type of the cvar's value.
		/// </summary>
		Type ICvar.ValueType => typeof(T);

		/// <summary>
		///   Sets the cvar's value.
		/// </summary>
		/// <param name="value">The value that should be set.</param>
		/// <param name="setByUser">If true, indicates that the cvar was set by the user (e.g., via the console).</param>
		void ICvar.SetValue(object value, bool setByUser)
		{
			Assert.ArgumentNotNull(value, nameof(value));

			if (setByUser && SystemOnly)
				Log.Warn("'{0}' can only be set by the application.", Name);
			else
				Value = (T)value;
		}

		/// <summary>
		///   Parses the next value in the input stream that corresponds to the cvar's type.
		/// </summary>
		/// <param name="inputStream">The input stream the value should be parsed from.</param>
		public object Parse(InputStream inputStream)
		{
			return TypeRegistry.GetParser<T>()(inputStream);
		}

		/// <summary>
		///   Validates the given value, returning true if the value is valid.
		/// </summary>
		/// <param name="value">The value that should be validated.</param>
		private bool ValidateValue(T value)
		{
			foreach (var validator in _validators)
			{
				if (validator.Validate(value))
					continue;

				Log.Error("'{0}' could not be set to '{1}\\default': {2}", Name, TypeRegistry.ToString(value), validator.ErrorMessage);
				Log.Info("{0}", Help.GetHint(Name));
				return false;
			}

			return true;
		}

		/// <summary>
		///   Sets the cvar's current value to the new one.
		/// </summary>
		/// <param name="value">The value the cvar should be set to.</param>
		private void UpdateValue(T value)
		{
			if (_value.Equals(value))
			{
				if (Program.Initialized)
					Log.Warn("'{0}' has not been changed, because the new and the old value are the same.", Name);
			}
			else
			{
				_value = value;
				Log.Info("'{0}' is now '{1}\\default'.", Name, TypeRegistry.ToString(value));

				Changed?.Invoke();
			}
		}

		/// <summary>
		///   Raised when the value of the cvar has changed.
		/// </summary>
		public event Action Changed;
	}
}