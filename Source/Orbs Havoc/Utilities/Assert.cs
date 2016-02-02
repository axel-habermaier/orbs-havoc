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

namespace OrbsHavoc.Utilities
{
	using System;
	using System.Collections;
	using System.Diagnostics;
	using JetBrains.Annotations;
	using Platform.Memory;

	// ReSharper disable UnusedParameter.Global

	/// <summary>
	///   Defines assertion helpers that can be used to check for errors. The checks are only performed in debug builds.
	/// </summary>
	public static class Assert
	{
		/// <summary>
		///   Throws an ArgumentNullException if the argument is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="argument">The argument to check for null.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("argument: null, argumentName: notnull => halt")]
		public static void ArgumentNotNull<T>([NoEnumeration] T argument, string argumentName)
			where T : class
		{
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (argument == null)
				throw new ArgumentNullException(argumentName);
		}

		/// <summary>
		///   Throws an ArgumentNullException if the pointer is null.
		/// </summary>
		/// <param name="pointer">The pointer to check for null.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("pointer: null,  argumentName: notnull => halt")]
		public static void ArgumentNotNull(IntPtr pointer, string argumentName)
		{
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (pointer == IntPtr.Zero)
				throw new ArgumentNullException(argumentName);
		}

		/// <summary>
		///   Throws an ArgumentException if the object is not the same as or subtype of the given type.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void ArgumentOfType<T>([NoEnumeration] object obj, string argumentName)
		{
			ArgumentNotNull(obj, nameof(obj));
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (!(obj is T))
				throw new ArgumentException("The given object is not of the requested type.", argumentName);
		}

		/// <summary>
		///   Throws an ArgumentException if the string argument is null or an ArgumentException if the string is empty (or
		///   only whitespace).
		/// </summary>
		/// <param name="argument">The argument to check.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("argument: null,  argumentName: notnull => halt")]
		public static void ArgumentNotNullOrWhitespace(string argument, string argumentName)
		{
			if (String.IsNullOrWhiteSpace(argumentName))
				throw new ArgumentException("String parameter cannot be empty or consist of whitespace only.", nameof(argumentName));

			if (String.IsNullOrWhiteSpace(argument))
				throw new ArgumentException("String parameter cannot be empty or consist of whitespace only.", argumentName);
		}

		/// <summary>
		///   Throws an ArgumentOutOfRangeException if the enum argument is outside the range of the enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enumeration.</typeparam>
		/// <param name="argument">The enumeration value to check.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void ArgumentInRange<TEnum>(TEnum argument, string argumentName)
			where TEnum : struct
		{
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (!Enum.IsDefined(typeof(TEnum), argument))
				throw new ArgumentOutOfRangeException(argumentName);
		}

		/// <summary>
		///   Throws an ArgumentOutOfRangeException if the argument is outside the range.
		/// </summary>
		/// <typeparam name="T">The type of the value to check.</typeparam>
		/// <param name="argument">The value to check.</param>
		/// <param name="min">The inclusive lower bound of the range.</param>
		/// <param name="max">The inclusive upper bound of the range.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void ArgumentInRange<T>(T argument, T min, T max, string argumentName)
			where T : IComparable<T>
		{
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (argument.CompareTo(min) < 0)
				throw new ArgumentOutOfRangeException(argumentName);

			if (argument.CompareTo(max) > 0)
				throw new ArgumentOutOfRangeException(argumentName);
		}

		/// <summary>
		///   Throws an ArgumentOutOfRangeException if the argument is outside the range.
		/// </summary>
		/// <param name="argument">The value of the index argument to check.</param>
		/// <param name="collection">The collection that defines the valid range of the given index argument.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void ArgumentInRange(int argument, ICollection collection, string argumentName)
		{
			ArgumentNotNull(collection, nameof(collection));
			ArgumentInRange(argument, 0, collection.Count - 1, argumentName);
		}

		/// <summary>
		///   Throws an ArgumentException if the condition does not hold.
		/// </summary>
		/// <param name="condition">The condition that, if false, causes the exception to be raised.</param>
		/// <param name="argumentName">The name of the argument that is checked.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("condition: false => halt")]
		public static void ArgumentSatisfies(bool condition, string argumentName, string formatMessage, params object[] parameters)
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));
			ArgumentNotNullOrWhitespace(argumentName, nameof(argumentName));

			if (!condition)
				throw new ArgumentException(String.Format(formatMessage, parameters), argumentName);
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is not null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="obj">The object to check for null.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("obj: notnull => halt")]
		public static void IsNull<T>([NoEnumeration] T obj, string formatMessage, params object[] parameters)
			where T : class
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (obj != null)
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="obj">The object to check for null.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("obj: null => halt")]
		public static void NotNull<T>([NoEnumeration] T obj, string formatMessage, params object[] parameters)
			where T : class
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (obj == null)
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="obj">The object to check for null.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("obj: null => halt")]
		public static void NotNull<T>(T? obj, string formatMessage, params object[] parameters)
			where T : struct
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (obj == null)
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the pointer is null.
		/// </summary>
		/// <param name="ptr">The pointer to check for null.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("ptr: null => halt")]
		public static void NotNull(IntPtr ptr, string formatMessage, params object[] parameters)
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (ptr == IntPtr.Zero)
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="obj">The object to check for null.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("null => halt")]
		public static void NotNull<T>([NoEnumeration] T obj)
			where T : class
		{
			if (obj == null)
				throw new FatalErrorException("Expected a valid reference.");
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is null.
		/// </summary>
		/// <typeparam name="T">The type of the argument to check for null.</typeparam>
		/// <param name="obj">The object to check for null.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("null => halt")]
		public static void NotNull<T>(T? obj)
			where T : struct
		{
			if (obj == null)
				throw new FatalErrorException("Expected a valid reference.");
		}

		/// <summary>
		///   Throws a FatalErrorException if the pointer is null.
		/// </summary>
		/// <param name="ptr">The pointer to check for null.</param>
		[Conditional("DEBUG"), DebuggerHidden, ContractAnnotation("null => halt")]
		public static void NotNull(IntPtr ptr)
		{
			if (ptr == IntPtr.Zero)
				throw new FatalErrorException("Expected a valid pointer.");
		}

		/// <summary>
		///   Throws a FatalErrorException if the string is null or empty (or only whitespace).
		/// </summary>
		/// <param name="s">The string to check.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("s: null => halt")]
		public static void NotNullOrWhitespace(string s, string formatMessage, params object[] parameters)
		{
			ArgumentNotNull(s, nameof(s));

			if (String.IsNullOrWhiteSpace(s))
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the condition does not hold.
		/// </summary>
		/// <param name="condition">The condition that, if false, causes the exception to be raised.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("condition: false => halt")]
		public static void That(bool condition, string formatMessage, params object[] parameters)
		{
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (!condition)
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the method is invoked.
		/// </summary>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage"), ContractAnnotation("=> halt")]
		public static void NotReached(string formatMessage, params object[] parameters)
		{
			That(false, formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the enum argument is outside the range of the enumeration.
		/// </summary>
		/// <typeparam name="TEnum">The type of the enumeration.</typeparam>
		/// <param name="argument">The enumeration value to check.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void InRange<TEnum>(TEnum argument)
			where TEnum : struct
		{
			if (!Enum.IsDefined(typeof(TEnum), argument))
				throw new FatalErrorException("The enumeration value lies outside the allowable range.");
		}

		/// <summary>
		///   Throws a FatalErrorException if the argument is outside the range.
		/// </summary>
		/// <typeparam name="T">The type of the value to check.</typeparam>
		/// <param name="index">The value to check.</param>
		/// <param name="min">The inclusive lower bound of the range.</param>
		/// <param name="max">The inclusive upper bound of the range.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void InRange<T>(T index, T min, T max)
			where T : IComparable<T>
		{
			if (index.CompareTo(min) < 0)
				throw new FatalErrorException("Lower bound violation. Expected argument to lie between {0} and {1}.", min, max);

			if (index.CompareTo(max) > 0)
				throw new FatalErrorException("Upper bound violation. Expected argument to lie between {0} and {1}.", min, max);
		}

		/// <summary>
		///   Throws a FatalErrorException if the argument is outside the range.
		/// </summary>
		/// <param name="index">The value of the index to check.</param>
		/// <param name="collection">The collection that defines the valid range of the given index argument.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void InRange(int index, ICollection collection)
		{
			ArgumentNotNull(collection, nameof(collection));
			InRange(index, 0, collection.Count - 1);
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is not the same as or subtype of the given type.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void OfType<T>([NoEnumeration] object obj)
		{
			ArgumentNotNull(obj, nameof(obj));

			if (!(obj is T))
				throw new FatalErrorException("The given object is not of the requested type.");
		}

		/// <summary>
		///   Throws a FatalErrorException if the object is not the same as or subtype of the given type.
		/// </summary>
		/// <param name="obj">The object to check.</param>
		/// <param name="formatMessage">An error message explaining the exception to the user.</param>
		/// <param name="parameters">The parameters for the error message.</param>
		[Conditional("DEBUG"), DebuggerHidden, StringFormatMethod("formatMessage")]
		public static void OfType<T>([NoEnumeration] object obj, string formatMessage, params object[] parameters)
		{
			ArgumentNotNull(obj, nameof(obj));
			ArgumentNotNull(formatMessage, nameof(formatMessage));

			if (!(obj is T))
				throw new FatalErrorException(formatMessage, parameters);
		}

		/// <summary>
		///   Throws a FatalErrorException if the given object is not null or has not been disposed.
		/// </summary>
		/// <param name="obj">The object that should be checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void NullOrDisposed<T>([NoEnumeration] T obj)
			where T : DisposableObject
		{
			if (obj != null && !obj.IsDisposed)
				throw new FatalErrorException("The '{0}' instance has not been disposed.", typeof(T).FullName);
		}

		/// <summary>
		///   Throws a FatalErrorException if the given object has already been disposed.
		/// </summary>
		/// <param name="obj">The object that should be checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void NotDisposed([NoEnumeration] DisposableObject obj)
		{
			ArgumentNotNull(obj, nameof(obj));

			if (obj.IsDisposed)
				throw new FatalErrorException("The object of type '{0}' has already been disposed.", obj.GetType().FullName);
		}

		/// <summary>
		///   Throws a FatalErrorException if the given object has currently pooled and not in use.
		/// </summary>
		/// <param name="obj">The object that should be checked.</param>
		[Conditional("DEBUG"), DebuggerHidden]
		public static void NotPooled([NoEnumeration] IPooledObject obj)
		{
			ArgumentNotNull(obj, nameof(obj));

			if (!obj.InUse)
				throw new FatalErrorException("The object of type '{0}' is currently pooled.", obj.GetType().FullName);
		}
	}
}