// The MIT License (MIT)
// 
// Copyright (c) 2015, Axel Habermaier
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

namespace PointWars.Platform.Input
{
	using System;
	using Utilities;

	/// <summary>
	///   Represents an input activation state with deferred updates. The state is considered active for as long as Count != 0.
	/// </summary>
	internal struct ActivationState
	{
		/// <summary>
		///   The current activation count.
		/// </summary>
		public ushort Count;

		/// <summary>
		///   The pending activation and deactivation requests.
		/// </summary>
		public short Pending;

		/// <summary>
		///   Executes all deferred updates to the state.
		/// </summary>
		public void Update()
		{
			Count = (ushort)(Count + Pending);
			Pending = 0;
		}

		/// <summary>
		///   Handles a deferred activation request.
		/// </summary>
		public void Activate()
		{
			Assert.InRange(Pending, Int16.MinValue, Int16.MaxValue);
			Assert.That(Count + Pending + 1 < UInt16.MaxValue, "Too many activations.");

			++Pending;
		}

		/// <summary>
		///   Handles a deferred deactivation request.
		/// </summary>
		public void Deactivate()
		{
			Assert.InRange(Pending, Int16.MinValue, Int16.MaxValue);
			Assert.That(Count + Pending > 0, "Imbalanced call to deactivate.");

			--Pending;
		}
	}
}