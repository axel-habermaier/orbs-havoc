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
	internal sealed class Cvar<T> : ICvar
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
		///   Gets a value indicating whether the cvar's value has been set explicitly. If false, the cvar has its default value. This
		///   property is also true if the cvar's default value has been set explicitly.
		/// </summary>
		public bool HasExplicitValue { get; private set; }

		/// <summary>
		///   Gets or sets the value of the cvar.
		/// </summary>
		public T Value
		{
			get => _value;
			set => SetValue(value, setByUser: false);
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
				Log.Warn($"'{Name}' can only be set by the application.");
			else
				SetValue((T)value, setByUser);
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
		///   Sets the cvar's value.
		/// </summary>
		/// <param name="value">The value that should be set.</param>
		/// <param name="setByUser">If true, indicates that the cvar was set by the user (e.g., via the console).</param>
		private void SetValue(T value, bool setByUser)
		{
			if (!ValidateValue(value))
				return;

			UpdateValue(value, setByUser);
			HasExplicitValue = true;
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

				Log.Error($"'{Name}' could not be set to '{TypeRegistry.ToString(value)}\\default': {validator.ErrorMessage}");
				Log.Info(Help.GetHint(Name));
				return false;
			}

			return true;
		}

		/// <summary>
		///   Sets the cvar's current value to the new one.
		/// </summary>
		/// <param name="value">The value the cvar should be set to.</param>
		/// <param name="setByUser">If true, indicates that the cvar was set by the user (e.g., via the console).</param>
		private void UpdateValue(T value, bool setByUser)
		{
			if (_value.Equals(value))
			{
				if (setByUser)
					Log.Warn($"'{Name}' has not been changed, because the new and the old value are the same.");
			}
			else
			{
				_value = value;
				Log.Info($"'{Name}' is now '{TypeRegistry.ToString(value)}\\default'.");

				Changed?.Invoke();
			}
		}

		/// <summary>
		///   Raised when the value of the cvar has changed.
		/// </summary>
		public event Action Changed;
	}
}