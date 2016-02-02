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

namespace OrbsHavoc.Scripting.Parsing
{
	using System;
	using System.Linq;
	using System.Net;
	using System.Numerics;
	using Platform.Input;
	using Utilities;

	/// <summary>
	///   Provides access to parsing functions.
	/// </summary>
	public static class Parser
	{
		/// <summary>
		///   Parses a cvar or command name.
		/// </summary>
		public static string ParseIdentifier(InputStream inputStream)
		{
			var position = inputStream.State.Position;
			var c = inputStream.Peek();

			if (!Char.IsLetter(c) && c != '_')
				throw new ParseException(inputStream, "Expected a valid identifier.");

			while (!inputStream.EndOfInput)
			{
				c = inputStream.Peek();
				if (!Char.IsLetterOrDigit(c) && c != '_')
					break;

				inputStream.Skip(1);
			}
			return inputStream.Substring(position, inputStream.State.Position - position);
		}

		/// <summary>
		///   Parses a byte value.
		/// </summary>
		public static byte ParseUInt8(InputStream inputStream)
		{
			return ParseNumber(inputStream, Byte.Parse, allowNegative: false, allowDecimal: false);
		}

		/// <summary>
		///   Parses a sbyte value.
		/// </summary>
		public static sbyte ParseInt8(InputStream inputStream)
		{
			return ParseNumber(inputStream, SByte.Parse, allowNegative: true, allowDecimal: false);
		}

		/// <summary>
		///   Parses a ushort value.
		/// </summary>
		public static ushort ParseUInt16(InputStream inputStream)
		{
			return ParseNumber(inputStream, UInt16.Parse, allowNegative: false, allowDecimal: false);
		}

		/// <summary>
		///   Parses a short value.
		/// </summary>
		public static short ParseInt16(InputStream inputStream)
		{
			return ParseNumber(inputStream, Int16.Parse, allowNegative: true, allowDecimal: false);
		}

		/// <summary>
		///   Parses a uint value.
		/// </summary>
		public static uint ParseUInt32(InputStream inputStream)
		{
			return ParseNumber(inputStream, UInt32.Parse, allowNegative: false, allowDecimal: false);
		}

		/// <summary>
		///   Parses a int value.
		/// </summary>
		public static int ParseInt32(InputStream inputStream)
		{
			return ParseNumber(inputStream, Int32.Parse, allowNegative: true, allowDecimal: false);
		}

		/// <summary>
		///   Parses a ulong value.
		/// </summary>
		public static ulong ParseUInt64(InputStream inputStream)
		{
			return ParseNumber(inputStream, UInt64.Parse, allowNegative: false, allowDecimal: false);
		}

		/// <summary>
		///   Parses a long value.
		/// </summary>
		public static long ParseInt64(InputStream inputStream)
		{
			return ParseNumber(inputStream, Int64.Parse, allowNegative: true, allowDecimal: false);
		}

		/// <summary>
		///   Parses a float value.
		/// </summary>
		public static float ParseFloat32(InputStream inputStream)
		{
			return ParseNumber(inputStream, Single.Parse, allowNegative: true, allowDecimal: true);
		}

		/// <summary>
		///   Parses a double value.
		/// </summary>
		public static double ParseFloat64(InputStream inputStream)
		{
			return ParseNumber(inputStream, Double.Parse, allowNegative: true, allowDecimal: true);
		}

		/// <summary>
		///   Checks whether the next token in the stream is the given keyword; the stream position remains unmodified if false is
		///   returned.
		/// </summary>
		public static bool IsKeyword(InputStream inputStream, string keyword)
		{
			var state = inputStream.State;
			foreach (var character in keyword.ToCharArray())
			{
				if (inputStream.EndOfInput || Char.ToLower(inputStream.Peek()) != character)
				{
					inputStream.State = state;
					return false;
				}

				inputStream.Skip(1);
			}

			return true;
		}

		/// <summary>
		///   Parses a bool value.
		/// </summary>
		public static bool ParseBoolean(InputStream inputStream)
		{
			if (IsKeyword(inputStream, "true") || IsKeyword(inputStream, "1") || IsKeyword(inputStream, "on"))
				return true;

			if (IsKeyword(inputStream, "false") || IsKeyword(inputStream, "0") || IsKeyword(inputStream, "off"))
				return false;

			throw new ParseException(inputStream, "Expected: {0}", TypeRegistry.GetDescription<bool>());
		}

		/// <summary>
		///   Parses a possibly quoted string.
		/// </summary>
		public static string ParseQuotedString(InputStream inputStream)
		{
			if (inputStream.Peek() != '"')
			{
				var position = inputStream.State.Position;
				while (!inputStream.EndOfInput && !Char.IsWhiteSpace(inputStream.Peek()))
					inputStream.Skip(1);

				return inputStream.Substring(position, inputStream.State.Position - position);
			}
			else
			{
				var state = inputStream.State;
				inputStream.Skip(1);

				var wasBackslash = false;
				while (!inputStream.EndOfInput)
				{
					var character = inputStream.Peek();

					if (character == '"' && !wasBackslash)
						break;

					wasBackslash = character == '\\';
					inputStream.Skip(1);
				}

				// Parse the closing quote
				if (inputStream.EndOfInput)
				{
					inputStream.State = state;
					throw new ParseException(inputStream, "Missing closing quote '\"'.");
				}

				// Extract the string literal and unescape all escaped quotes
				var result = inputStream.Substring(state.Position + 1, inputStream.State.Position - state.Position - 1);
				result = result.Replace("\\\"", "\"");

				// Skip the closing quote and return the result.
				inputStream.Skip(1);
				return result;
			}
		}

		/// <summary>
		///   Parses an enumeration literal of the given type.
		/// </summary>
		public static T ParseEnumerationLiteral<T>(InputStream inputStream)
		{
			try
			{
				var identifier = ParseIdentifier(inputStream);
				var literal = Enum.Parse(typeof(T), identifier, ignoreCase: true);
				return (T)literal;
			}
			catch (Exception)
			{
				throw new ParseException(inputStream, "Expected a valid '{0}' literal.", TypeRegistry.GetDescription<T>());
			}
		}

		/// <summary>
		///   Parses an enumeration literal of the given type.
		/// </summary>
		public static Func<InputStream, object> ParseEnumerationLiteral(Type type)
		{
			return inputStream =>
			{
				try
				{
					var identifier = ParseIdentifier(inputStream);
					return Enum.Parse(type, identifier, ignoreCase: true);
				}
				catch (Exception)
				{
					throw new ParseException(inputStream, "Expected a valid '{0}' literal.", TypeRegistry.GetDescription(type));
				}
			};
		}

		/// <summary>
		///   Parses an IPv4 or IPv6 address.
		/// </summary>
		public static IPAddress ParseIPAddress(InputStream inputStream)
		{
			var state = inputStream.State;

			// Check for the 'localhost' keyword first
			if (IsKeyword(inputStream, "localhost"))
				return IPAddress.IPv6Loopback;

			// Try to find out whether it is an IPv4 or IPv6 address
			var ipv4 = inputStream.Skip(c => c != '.');
			inputStream.State = state;
			var ipv6 = inputStream.Skip(c => c != ':');
			inputStream.State = state;

			var isIPv6 = ipv6 < ipv4;
			int length;

			if (isIPv6)
				length = inputStream.Skip(c => Char.IsDigit(c) || c == ':' || c == '.' || c == '%' ||
											   (Char.ToLower(c) >= 'a' && Char.ToLower(c) <= 'f'));
			else
				length = inputStream.Skip(c => Char.IsDigit(c) || c == '.');

			IPAddress address;
			if (IPAddress.TryParse(inputStream.Substring(state.Position, length), out address))
				return address;

			throw new ParseException(inputStream, "Expected an IP address (either IPv4 or IPv6).");
		}

		/// <summary>
		///   Parses a size value of the format 100x100.
		/// </summary>
		public static Size ParseSize(InputStream inputStream)
		{
			var width = ParseFloat32(inputStream);

			if (inputStream.Peek() != 'x')
				throw new ParseException(inputStream, "Expected two values of type '{0}' separated by 'x'.", TypeRegistry.GetDescription<float>());

			inputStream.Skip(1);
			var height = ParseFloat32(inputStream);
			return new Size(width, height);
		}

		/// <summary>
		///   Parses a Vector2 value of the format 100;100.
		/// </summary>
		public static Vector2 ParseVector2(InputStream inputStream)
		{
			var width = ParseFloat32(inputStream);

			if (inputStream.Peek() != ';')
				throw new ParseException(inputStream, "Expected two values of type '{0}' separated by ';'.", TypeRegistry.GetDescription<float>());

			inputStream.Skip(1);
			var height = ParseFloat32(inputStream);
			return new Size(width, height);
		}

		/// <summary>
		///   Parses a configurable input of the form 'Key.A', 'Key.A+Control', 'Mouse.Left+Alt+Shift', etc.
		/// </summary>
		public static ConfigurableInput ParseConfigurableInput(InputStream inputStream)
		{
			if (inputStream.Peek() != '[')
				throw new ParseException(inputStream, "Expected a '['.");

			var state = inputStream.State;
			inputStream.Skip(c => c != ']');

			if (inputStream.EndOfInput)
			{
				inputStream.State = state;
				throw new ParseException(inputStream, "'[' is never closed.");
			}

			inputStream.State = state;
			inputStream.Skip(1);

			var modifiers = KeyModifiers.None;
			Key? key = null;
			MouseButton? button = null;
			MouseWheelDirection? direction = null;

			while (true)
			{
				inputStream.SkipWhitespaces();
				if (inputStream.Peek() == ']')
					break;

				if (inputStream.Peek() == '+')
				{
					inputStream.Skip(1);
					inputStream.SkipWhitespaces();
				}

				var begin = inputStream.State;
				inputStream.Skip(c => c != '+' && c != ']');

				var value = inputStream.Substring(begin.Position, inputStream.State.Position - begin.Position);
				var normalizedValue = value.ToLower().Trim();

				if (normalizedValue == String.Empty)
					throw new ParseException(inputStream, "Unexpected token '{0}'.", inputStream.Peek());

				switch (normalizedValue)
				{
					case "control":
						modifiers |= KeyModifiers.Control;
						break;
					case "alt":
						modifiers |= KeyModifiers.Alt;
						break;
					case "shift":
						modifiers |= KeyModifiers.Shift;
						break;
					default:
						try
						{
							if (normalizedValue.StartsWith("key."))
							{
								var literal = normalizedValue.Substring("key.".Length);
								key = (Key)Enum.Parse(typeof(Key), literal, ignoreCase: true);
							}
							else if (normalizedValue.StartsWith("mouse."))
							{
								var literal = normalizedValue.Substring("mouse.".Length);
								button = (MouseButton)Enum.Parse(typeof(MouseButton), literal, ignoreCase: true);
							}
							else if (normalizedValue.StartsWith("mousewheel."))
							{
								var literal = normalizedValue.Substring("mousewheel.".Length);
								direction = (MouseWheelDirection)Enum.Parse(typeof(MouseWheelDirection), literal, ignoreCase: true);
							}
							else
							{
								inputStream.State = begin;
								throw new ParseException(inputStream, "Input contains unrecognizable value '{0}'.", value.Trim());
							}
						}
						catch (ArgumentException)
						{
							inputStream.State = begin;
							throw new ParseException(inputStream, "Input contains unrecognizable value '{0}'.", value.Trim());
						}
						break;
				}

				if (key != null && button != null)
				{
					inputStream.State = begin;
					throw new ParseException(inputStream, "Input cannot use both key and mouse button at the same time.");
				}
			}

			inputStream.Skip(1);

			if (key != null)
				return new ConfigurableInput(key.Value, modifiers);

			if (button != null)
				return new ConfigurableInput(button.Value, modifiers);

			if (direction != null)
				return new ConfigurableInput(direction.Value, modifiers);

			inputStream.State = state;
			throw new ParseException(inputStream, "Input must use a key or a mouse button.");
		}

		/// <summary>
		///   Parses the cvar or command instruction.
		/// </summary>
		internal static Instruction ParseInstruction(InputStream inputStream)
		{
			try
			{
				// Skip all leading white space
				inputStream.SkipWhitespaces();

				if (inputStream.EndOfInput)
					throw new ParseException(inputStream, "Expected a valid cvar or command name.");

				// Parse the cvar or command name
				var state = inputStream.State;
				var name = ParseIdentifier(inputStream);

				// Check if a cvar has been referenced and if so, return the appropriate instruction
				ICvar cvar;
				if (Cvars.TryFind(name, out cvar))
					return ParseCvar(inputStream, cvar);

				// Check if a command has been referenced and if so, return the appropriate instruction
				ICommand command;
				if (Commands.TryFind(name, out command))
					return ParseCommand(inputStream, command);

				// If the name refers to neither a cvar nor a command, give up
				inputStream.State = state;
				throw new ParseException(inputStream, $"Unknown cvar or command '{name}'.");
			}
			catch (ParseException e)
			{
				throw new ParseException(e.InputStream, "{0}\n{1}^\n{2}", e.Input, new string(' ', e.Position), e.Message);
			}
		}

		/// <summary>
		///   Parses a cvar instruction.
		/// </summary>
		private static Instruction ParseCvar(InputStream inputStream, ICvar cvar)
		{
			if (inputStream.WhiteSpaceUntilEndOfInput())
				return new Instruction(cvar, null);

			// The argument must be separated from the previous one by at least one white space character
			if (!Char.IsWhiteSpace(inputStream.Peek()))
				throw new ParseException(inputStream, "Expected a whitespace character.");

			inputStream.SkipWhitespaces();

			Instruction instruction;
			try
			{
				instruction = new Instruction(cvar, cvar.Parse(inputStream));
			}
			catch (ParseException e)
			{
				throw new ParseException(inputStream, "{0}\n{1}\n{2}",
					e.Message, TypeRegistry.GetExampleString(cvar.ValueType), Help.GetHint(cvar.Name));
			}

			if (!inputStream.WhiteSpaceUntilEndOfInput())
				throw new ParseException(inputStream, "Expected end of input.");

			return instruction;
		}

		/// <summary>
		///   Parses a command invocation.
		/// </summary>
		private static Instruction ParseCommand(InputStream inputStream, ICommand command)
		{
			var parameters = command.Parameters.ToArray();
			var values = new object[parameters.Length];

			for (var i = 0; i < parameters.Length; ++i)
			{
				// We've reached the end of the input, and all subsequent parameters should use their default value, so we're done
				if (inputStream.WhiteSpaceUntilEndOfInput() && parameters[i].HasDefaultValue)
				{
					for (var j = i; j < parameters.Length; ++j)
						values[j] = parameters[j].DefaultValue;

					break;
				}

				// We've reached the end of the input, but we're missing at least one parameter
				if (inputStream.WhiteSpaceUntilEndOfInput() && !parameters[i].HasDefaultValue)
				{
					inputStream.SkipWhitespaces(); // To get the correct column in the error message

					throw new ParseException(inputStream, "Expected a value of type '{0}'.\n{1}\n{2}",
						TypeRegistry.GetDescription(parameters[i].Type), TypeRegistry.GetExampleString(parameters[i].Type), Help.GetHint(command.Name));
				}

				// The argument must be separated from the previous one by at least one white space character
				if (!Char.IsWhiteSpace(inputStream.Peek()))
					throw new ParseException(inputStream, "Expected a whitespace character.");

				inputStream.SkipWhitespaces();

				try
				{
					values[i] = command.Parse(inputStream, parameterIndex: i);
				}
				catch (ParseException e)
				{
					throw new ParseException(inputStream,
						"Invalid value for parameter '{0}': {1}\nParameter type: {2}\n{3}\n{4}",
						parameters[i].Name, e.Message, TypeRegistry.GetDescription(parameters[i].Type),
						TypeRegistry.GetExampleString(parameters[i].Type), Help.GetHint(command.Name));
				}
			}

			if (!inputStream.WhiteSpaceUntilEndOfInput())
				throw new ParseException(inputStream, "Expected end of input.");

			return new Instruction(command, values);
		}

		/// <summary>
		///   Parses a number value.
		/// </summary>
		private static T ParseNumber<T>(InputStream inputStream, Func<string, T> convert, bool allowNegative, bool allowDecimal)
		{
			var state = inputStream.State;

			// Parse the optional sign
			var sign = inputStream.Peek();
			var negative = sign == '-';
			var positive = sign == '+';

			if (negative && !allowNegative)
			{
				inputStream.State = state;
				throw new ParseException(inputStream, "Expected a valid value of type '{0}'.", TypeRegistry.GetDescription<T>());
			}

			if (negative || positive)
				inputStream.Skip(1);

			// Parse the integer part, if any -- it may be missing if a fractional part follows
			var count = inputStream.Skip(Char.IsDigit);
			if (count == 0 && (!allowDecimal || inputStream.Peek() != '.'))
			{
				inputStream.State = state;
				throw new ParseException(inputStream, "Expected a valid value of type '{0}'.", TypeRegistry.GetDescription<T>());
			}

			// If a fractional part is allowed, parse it, but there must be at least one digit following
			if (allowDecimal && inputStream.Peek() == '.')
			{
				inputStream.Skip(1);
				count = inputStream.Skip(Char.IsDigit);

				if (count == 0)
				{
					inputStream.State = state;
					throw new ParseException(inputStream, "Expected a valid value of type '{0}'.", TypeRegistry.GetDescription<T>());
				}
			}

			// Convert everything from the first digit (or sign or decimal point) to the last digit followed by a non-digit character
			try
			{
				return convert(inputStream.Substring(state.Position, inputStream.State.Position - state.Position));
			}
			catch (FormatException)
			{
				inputStream.State = state;
				throw new ParseException(inputStream, "Expected a valid value of type '{0}'.", TypeRegistry.GetDescription<T>());
			}
			catch (OverflowException)
			{
				inputStream.State = state;
				throw new ParseException(inputStream, "The value lies outside the range of type '{0}'.", TypeRegistry.GetDescription<T>());
			}
		}
	}
}