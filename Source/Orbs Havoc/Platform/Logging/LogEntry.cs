﻿// The MIT License (MIT)
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

namespace OrbsHavoc.Platform.Logging
{
	using System;
	using System.Diagnostics;
	using Utilities;

	/// <summary>
	///   Represents a log entry with a specific type and message.
	/// </summary>
	public struct LogEntry : IEquatable<LogEntry>
	{
		/// <summary>
		///   Used to measure the time since the start of the application.
		/// </summary>
		private static readonly Stopwatch Stopwatch = new Stopwatch();

		/// <summary>
		///   Initializes the type.
		/// </summary>
		static LogEntry()
		{
			Stopwatch.Start();
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="logType">The type of the log entry.</param>
		/// <param name="message">The message of the log entry.</param>
		public LogEntry(LogType logType, string message)
			: this()
		{
			Assert.ArgumentInRange(logType, nameof(logType));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			LogType = logType;
			Message = message;
			Time = Stopwatch.Elapsed.TotalSeconds;
		}

		/// <summary>
		///   Gets the date and time of the creation of the log entry.
		/// </summary>
		public double Time { get; }

		/// <summary>
		///   Gets the message of the log entry.
		/// </summary>
		public string Message { get; }

		/// <summary>
		///   Gets the type of the log entry.
		/// </summary>
		public LogType LogType { get; }

		/// <summary>
		///   Gets a string representation of the type of the log entry. Each string is guaranteed to have the same length.
		/// </summary>
		public string LogTypeString
		{
			get
			{
				switch (LogType)
				{
					case LogType.Fatal:
						return "Fatal  ";
					case LogType.Error:
						return "Error  ";
					case LogType.Warning:
						return "Warning";
					case LogType.Info:
						return "Info   ";
					case LogType.Debug:
						return "Debug  ";
					default:
						throw new InvalidOperationException("Unknown log type.");
				}
			}
		}

		/// <summary>
		///   Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(LogEntry other)
		{
			return LogType == other.LogType && string.Equals(Message, other.Message) && Time.Equals(other.Time);
		}

		/// <summary>
		///   Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to. </param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;

			return obj is LogEntry && Equals((LogEntry)obj);
		}

		/// <summary>
		///   Returns the hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)LogType;
				hashCode = (hashCode * 397) ^ (Message?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ Time.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		///   Checks whether the given log entries are equal.
		/// </summary>
		public static bool operator ==(LogEntry left, LogEntry right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Checks whether the given log entries are not equal.
		/// </summary>
		public static bool operator !=(LogEntry left, LogEntry right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///   Raises the appropriate log event for the log entries.
		/// </summary>
		[DebuggerHidden]
		public void RaiseLogEvent()
		{
			switch (LogType)
			{
				case LogType.Fatal:
					Log.Die("{0}", Message);
					break;
				case LogType.Error:
					Log.Error("{0}", Message);
					break;
				case LogType.Warning:
					Log.Warn("{0}", Message);
					break;
				case LogType.Info:
					Log.Info("{0}", Message);
					break;
				case LogType.Debug:
					Log.Debug("{0}", Message);
					break;
				default:
					throw new InvalidOperationException("Unknown log entry type.");
			}
		}
	}
}