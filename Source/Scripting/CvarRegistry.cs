﻿// The MIT License (MIT)
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
	using System.Collections.Generic;
	using System.Linq;
	using Utilities;

	/// <summary>
	///   Provides access to all cvars.
	/// </summary>
	public static class CvarRegistry
	{
		/// <summary>
		///   The registered cvars.
		/// </summary>
		private static readonly Dictionary<string, ICvar> Cvars = new Dictionary<string, ICvar>();

		/// <summary>
		///   Gets all registered cvars.
		/// </summary>
		internal static IEnumerable<ICvar> All => Cvars.Values;

		/// <summary>
		///   Registers the given cvar.
		/// </summary>
		/// <param name="cvar">The cvar that should be registered.</param>
		public static void Register(ICvar cvar)
		{
			Assert.ArgumentNotNull(cvar, nameof(cvar));
			Assert.NotNullOrWhitespace(cvar.Name, "The cvar cannot have an empty name.");
			Assert.That(!Cvars.ContainsKey(cvar.Name), "A cvar with the name '{0}' has already been registered.", cvar.Name);
			Assert.That(CommandRegistry.All.All(command => command.Name != cvar.Name),
				"A command with the name '{0}' has already been registered.", cvar.Name);

			Cvars.Add(cvar.Name, cvar);
		}

		/// <summary>
		///   Finds the cvar with the given name. Returns false if no such cvar is found.
		/// </summary>
		/// <param name="name">The name of the cvar that should be returned.</param>
		/// <param name="cvar">The cvar with the given name, if it is found.</param>
		internal static bool TryFind(string name, out ICvar cvar)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			return Cvars.TryGetValue(name, out cvar);
		}
	}
}