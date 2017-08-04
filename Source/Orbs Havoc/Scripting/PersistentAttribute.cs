﻿namespace OrbsHavoc.Scripting
{
	using System;
	using JetBrains.Annotations;

	/// <summary>
	///   When applied to a cvar property in a registry specification interface, indicates that the value of the cvar is
	///   persisted across app sessions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	[MeansImplicitUse]
	public class PersistentAttribute : Attribute
	{
	}
}