﻿namespace OrbsHavoc.Scripting
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Parsing;
	using Platform;
	using UserInterface.Input;
	using Platform.Logging;
	using Rendering;
	using Utilities;
	using static Parsing.Parser;

	/// <summary>
	///   Manages the types that can be used for command parameters and cvar values. All enumerations types as well as most C#
	///   built-in types and common .NET types are supported automatically. Enumeration types can also be registered, in which case
	///   they overwrite the defaults.
	/// </summary>
	internal static class TypeRegistry
	{
		/// <summary>
		///   Stores the registered type information.
		/// </summary>
		private static readonly Dictionary<Type, TypeInfo> _registeredTypes = new Dictionary<Type, TypeInfo>();

		/// <summary>
		///   Initializes the type and registers all built-in types.
		/// </summary>
		static TypeRegistry()
		{
			// Register the C# built-in types (except for char, for which there is no obvious use-case)
			Register(ParseBoolean, "Boolean", b => b ? "true" : "false", "true", "false", "0", "1", "on", "off");
			Register(ParseUInt8, "8-bit unsigned integer", null, "0", "17");
			Register(ParseInt8, "8-bit signed integer", null, "-17", "0", "17");
			Register(ParseUInt16, "16-bit unsigned integer", null, "0", "17");
			Register(ParseInt16, "16-bit signed integer", null, "-17", "0", "17");
			Register(ParseUInt32, "32-bit unsigned integer", null, "0", "17");
			Register(ParseInt32, "32-bit signed integer", null, "-17", "0", "17");
			Register(ParseUInt64, "64-bit unsigned integer", null, "0", "17");
			Register(ParseInt64, "64-bit signed integer", null, "-17", "0", "17");
			Register(ParseFloat32, "32-bit floating point number", f => f.ToString("F"), "-17.1", "0.0", "17");
			Register(ParseFloat64, "64-bit floating point number", d => d.ToString("F"), "-17.1", "0.0", "17");
			Register(ParseQuotedString, "string", null, "\"\"", "word", "\"multiple words\"", "\"escaped quote: \\\"\"");

			// Register some additional common types
			Register(ParseVector2, null, s => $"{s.X};{s.Y}", "0;0", "-10;10.5");
			Register(ParseSize, null, s => $"{s.Width}x{s.Height}", "0x0", "-10x10.5", "1920x1200");
			Register(ParseEnumerationLiteral<WindowMode>, null, null);
			Register(ParseEnumerationLiteral<QualityLevel>, null, null);
			Register(ParseEnumerationLiteral<Key>, "Key", null, "A", "B", "LeftControl", "Return", "F1");
			Register(ParseEnumerationLiteral<MouseButton>, "Mouse Button", null, "Left", "Right", "Middle", "XButton1", "XButton2");
			Register(ParseInputTrigger, null, null, "[Key.A+Control]", "[Mouse.Left+Alt]", "[Mouse.XButton1+Shift+Alt]");
		}

		/// <summary>
		///   Registers the given type.
		/// </summary>
		/// <typeparam name="T">The type that should be registered.</typeparam>
		/// <param name="parser">A parser that parses an input string into the given type.</param>
		/// <param name="description">A more user-friendly description of the type. If null, the C# type name is used.</param>
		/// <param name="toString">Converts a value of the type to a string. If null, object.ToString() is used.</param>
		/// <param name="examples">
		///   A list of example values for the type that can be parsed by the parser. Can only contain zero elements for
		///   enumeration types, where a list of all enumeration literals can be generated automatically.
		/// </param>
		public static void Register<T>(Func<InputStream, T> parser, string description, Func<T, string> toString, params string[] examples)
		{
			Assert.ArgumentNotNull(parser, nameof(parser));
			Assert.ArgumentNotNull(examples, nameof(examples));
			Assert.ArgumentSatisfies(description == null || !String.IsNullOrWhiteSpace(description), nameof(description),
				"The description cannot be empty.");
			Assert.ArgumentSatisfies(examples.Length > 0 || typeof(T).GetTypeInfo().IsEnum, nameof(examples),
				"The examples can be empty for enumeration types only.");
			Assert.That(!_registeredTypes.ContainsKey(typeof(T)), "The type has already been registered.");

			description = description ?? typeof(T).Name;

			if (examples.Length == 0)
				examples = Enum.GetNames(typeof(T));

			if (!typeof(T).GetTypeInfo().IsEnum || examples.Length != Enum.GetNames(typeof(T)).Length)
				examples = examples.Concat(new[] { "..." }).ToArray();

			Func<object, string> objToString;
			if (toString == null)
				objToString = o => o.ToString();
			else
				objToString = o => toString((T)o);

			_registeredTypes.Add(typeof(T), new TypeInfo(parser, description, examples, objToString));
		}

		/// <summary>
		///   Gets a parser for the given type.
		/// </summary>
		/// <typeparam name="T">The type for which the parser should be returned.</typeparam>
		internal static Func<InputStream, T> GetParser<T>()
		{
			return (Func<InputStream, T>)GetParser(typeof(T));
		}

		/// <summary>
		///   Gets a parser for the given type.
		/// </summary>
		/// <param name="type">The type for which the parser should be returned.</param>
		internal static object GetParser(Type type)
		{
			Assert.ArgumentNotNull(type, nameof(type));

			if (_registeredTypes.TryGetValue(type, out var info))
				return info.Parser;

			if (type.GetTypeInfo().IsEnum)
				return ParseEnumerationLiteral(type);

			Log.Die("An attempt was made to get a parser for an unregistered type.");
			return null;
		}

		/// <summary>
		///   Gets the user-friendly description of the given type.
		/// </summary>
		/// <typeparam name="T">The type for which the user-friendly description should be returned.</typeparam>
		internal static string GetDescription<T>()
		{
			return GetDescription(typeof(T));
		}

		/// <summary>
		///   Gets the user-friendly description of the given type.
		/// </summary>
		/// <param name="type">The type for which the user-friendly description should be returned.</param>
		internal static string GetDescription(Type type)
		{
			Assert.ArgumentNotNull(type, nameof(type));

			if (_registeredTypes.TryGetValue(type, out var info))
				return info.Description;

			if (type.GetTypeInfo().IsEnum)
				return type.Name;

			Log.Die("An attempt was made to get the description of an unregistered type.");
			return null;
		}

		/// <summary>
		///   Gets the examples for the given type.
		/// </summary>
		/// <typeparam name="T">The type for which the examples should be returned.</typeparam>
		internal static IEnumerable<string> GetExamples<T>()
		{
			return GetExamples(typeof(T));
		}

		/// <summary>
		///   Gets the examples for the given type.
		/// </summary>
		/// <param name="type">The type for which the examples should be returned.</param>
		internal static IEnumerable<string> GetExamples(Type type)
		{
			Assert.ArgumentNotNull(type, nameof(type));

			if (_registeredTypes.TryGetValue(type, out var info))
				return info.Examples;

			if (type.GetTypeInfo().IsEnum)
				return Enum.GetNames(type);

			Log.Die("An attempt was made to get examples for an unregistered type.");
			return null;
		}

		/// <summary>
		///   Gets the example string for the given type.
		/// </summary>
		/// <param name="type">The type the examples should be returned for.</param>
		internal static string GetExampleString(Type type)
		{
			return $"Examples of valid inputs: {string.Join(", ", GetExamples(type).Take(10))}";
		}

		/// <summary>
		///   Gets the string representation of the given value.
		/// </summary>
		/// <param name="value">The value for which the string representation should be returned.</param>
		internal static string ToString(object value)
		{
			Assert.ArgumentNotNull(value, nameof(value));

			if (_registeredTypes.TryGetValue(value.GetType(), out var info))
				return info.ToDisplayString(value);

			if (value.GetType().GetTypeInfo().IsEnum)
				return value.ToString();

			// Try all base types of the value, as typically only the base type of a hierarchy is registered
			var baseType = value.GetType().GetTypeInfo().BaseType;
			while (baseType != null && baseType != typeof(object))
			{
				if (_registeredTypes.TryGetValue(baseType, out info))
					return info.ToDisplayString(value);

				baseType = baseType.GetTypeInfo().BaseType;
			}

			Log.Die("An attempt was made to get the string representation of a value of an unregistered type.");
			return null;
		}

		/// <summary>
		///   Stores the information about a registered type.
		/// </summary>
		private struct TypeInfo
		{
			/// <summary>
			///   Initializes a new instance.
			/// </summary>
			/// <param name="parser">The parser for the type.</param>
			/// <param name="description">A user-friendly description of the type.</param>
			/// <param name="examples">A list of example values of the type.</param>
			/// <param name="toDisplayString">Converts a value of the type to a string.</param>
			public TypeInfo(object parser, string description, string[] examples, Func<object, string> toDisplayString)
				: this()
			{
				Assert.ArgumentNotNullOrWhitespace(description, nameof(description));
				Assert.ArgumentNotNull(examples, nameof(examples));
				Assert.ArgumentSatisfies(examples.Length > 0, nameof(examples), "There must be at least one example.");
				Assert.ArgumentNotNull(toDisplayString, nameof(toDisplayString));

				Parser = parser;
				Description = description;
				Examples = examples;
				ToDisplayString = toDisplayString;
			}

			/// <summary>
			///   Gets the parser for the type.
			/// </summary>
			public object Parser { get; }

			/// <summary>
			///   Gets the user-friendly description of the type.
			/// </summary>
			public string Description { get; }

			/// <summary>
			///   Gets the examples of the type.
			/// </summary>
			public string[] Examples { get; }

			/// <summary>
			///   Gets a function that converts an instance of the type into a string.
			/// </summary>
			public Func<object, string> ToDisplayString { get; }
		}
	}
}