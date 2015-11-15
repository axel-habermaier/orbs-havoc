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

namespace OrbsHavoc.Platform
{
	using System;
	using Memory;

	/// <summary>
	///   Provides further information about the platform the application is running on.
	/// </summary>
	public static class PlatformInfo
	{
		/// <summary>
		///   Indicates whether the platform is a big or little endian architecture.
		/// </summary>
		public const Endianess Endianess = 
#if BigEndian
			Memory.Endianess.Big;
#else
			Memory.Endianess.Little;
#endif

		/// <summary>
		///   Indicates whether the application was built in debug mode.
		/// </summary>
		public const bool IsDebug =
#if DEBUG
			true;
#else
			false;
#endif

		/// <summary>
		///   The type of the platform the application is running on.
		/// </summary>
		public static readonly PlatformType Platform =
			Environment.OSVersion.Platform == PlatformID.Win32NT ? PlatformType.Windows : PlatformType.Linux;
	}
}