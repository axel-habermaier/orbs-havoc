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
	using Utilities;

	/// <summary>
	///   Describes the state of an input stream.
	/// </summary>
	public struct InputStreamState : IEquatable<InputStreamState>
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="position">The zero-based position of the next character that will be read by the input stream.</param>
		public InputStreamState(int position)
			: this()
		{
			Assert.ArgumentInRange(position, 0, Int32.MaxValue, nameof(position));
			Position = position;
		}

		/// <summary>
		///   Gets the zero-based position of the next character that will be read by the input stream.
		/// </summary>
		public int Position { get; }

		/// <summary>
		///   Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(InputStreamState other)
		{
			return Position == other.Position;
		}

		/// <summary>
		///   Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">Another object to compare to.</param>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
				return false;
			return obj is InputStreamState && Equals((InputStreamState)obj);
		}

		/// <summary>
		///   Returns the hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}

		/// <summary>
		///   Checks the two input streams for equality.
		/// </summary>
		/// <param name="left">The left input stream.</param>
		/// <param name="right">The right input stream.</param>
		public static bool operator ==(InputStreamState left, InputStreamState right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Checks the two input streams for inequality.
		/// </summary>
		/// <param name="left">The left input stream.</param>
		/// <param name="right">The right input stream.</param>
		public static bool operator !=(InputStreamState left, InputStreamState right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///   Advances the position.
		/// </summary>
		internal InputStreamState Advance()
		{
			return new InputStreamState(Position + 1);
		}
	}
}