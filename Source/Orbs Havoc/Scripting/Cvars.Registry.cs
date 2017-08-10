namespace OrbsHavoc.Scripting
{
	using System.Collections.Generic;
	using System.Linq;
	using Utilities;

	/// <summary>
	///   Provides access to all cvars.
	/// </summary>
	internal static partial class Cvars
	{
		/// <summary>
		///   The registered cvars.
		/// </summary>
		private static readonly Dictionary<string, ICvar> _registeredCvars = new Dictionary<string, ICvar>();

		/// <summary>
		///   Gets all registered cvars.
		/// </summary>
		internal static IEnumerable<ICvar> All => _registeredCvars.Values;

		/// <summary>
		///   Registers the given cvar.
		/// </summary>
		/// <param name="cvar">The cvar that should be registered.</param>
		private static void Register(ICvar cvar)
		{
			Assert.ArgumentNotNull(cvar, nameof(cvar));
			Assert.NotNullOrWhitespace(cvar.Name, "The cvar cannot have an empty name.");
			Assert.That(!_registeredCvars.ContainsKey(cvar.Name), $"A cvar with the name '{cvar.Name}' has already been registered.");
			Assert.That(Commands.All.All(command => command.Name != cvar.Name),
				$"A command with the name '{cvar.Name}' has already been registered.");

			_registeredCvars.Add(cvar.Name, cvar);
		}

		/// <summary>
		///   Finds the cvar with the given name. Returns false if no such cvar is found.
		/// </summary>
		/// <param name="name">The name of the cvar that should be returned.</param>
		/// <param name="cvar">The cvar with the given name, if it is found.</param>
		internal static bool TryFind(string name, out ICvar cvar)
		{
			Assert.ArgumentNotNullOrWhitespace(name, nameof(name));
			return _registeredCvars.TryGetValue(name, out cvar);
		}
	}
}