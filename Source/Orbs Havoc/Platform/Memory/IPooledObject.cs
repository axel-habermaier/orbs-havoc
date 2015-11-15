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

namespace OrbsHavoc.Platform.Memory
{
	using System;

	/// <summary>
	///   Represents an object that is allocated from an object pool.
	/// </summary>
	public interface IPooledObject : IDisposable
	{
		/// <summary>
		///   Gets a value indicating whether the instance is currently available, that is, waiting in the pool to be reused.
		/// </summary>
		bool InUse { get; }

		/// <summary>
		///   Allows the caller to acquire shared ownership of the object. The object will not be returned to the pool before the
		///   caller called its Dispose method.
		/// </summary>
		/// <remarks>Unless, of course, some malicious caller invokes Dispose multiple times...</remarks>
		IDisposable AcquireOwnership();
	}
}