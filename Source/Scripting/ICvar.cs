// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
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
	using Validators;

	/// <summary>
	///   Represents a configurable value.
	/// </summary>
	public interface ICvar
	{
		/// <summary>
		///   Gets the external name of the cvar that is used to refer to the cvar in the console, for instance.
		/// </summary>
		string Name { get; }

		/// <summary>
		///   Gets the type of the cvar's value.
		/// </summary>
		Type ValueType { get; }

		/// <summary>
		///   Gets the validators that are used to validate the values of the cvar.
		/// </summary>
		IEnumerable<ValidatorAttribute> Validators { get; }

		/// <summary>
		///   Gets a string describing the usage and the purpose of the cvar.
		/// </summary>
		string Description { get; }

		/// <summary>
		///   Gets the cvar's value.
		/// </summary>
		object Value { get; }

		/// <summary>
		///   Gets the cvar's default value.
		/// </summary>
		object DefaultValue { get; }

		/// <summary>
		///   Gets a value indicating whether the cvar's value has been set explicitly. If false, the cvar has its default value. This
		///   property is also true if the cvar's default value has been set explicitly.
		/// </summary>
		bool HasExplicitValue { get; }

		/// <summary>
		///   Indicates whether the cvar's value is persisted across sessions.
		/// </summary>
		bool Persistent { get; }

		/// <summary>
		///   Gets a value indicating whether the cvar can only be set by the system and not via the console.
		/// </summary>
		bool SystemOnly { get; }

		/// <summary>
		///   Sets the cvar's value.
		/// </summary>
		/// <param name="value">The value that should be set.</param>
		/// <param name="setByUser">If true, indicates that the cvar was set by the user (e.g., via the console).</param>
		void SetValue(object value, bool setByUser);

		/// <summary>
		///   Parses the next value in the input stream that corresponds to the cvar's type.
		/// </summary>
		/// <param name="inputStream">The input stream the value should be parsed from.</param>
		object Parse(InputStream inputStream);
	}
}