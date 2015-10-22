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

namespace PointWars.Rendering
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using Platform;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;

	/// <summary>
	///   Provides access to assets used throughout the application.
	/// </summary>
	public sealed partial class Assets : DisposableObject
	{
		/// <summary>
		///   Initializes the assets.
		/// </summary>
		public Assets()
		{
			Load(LoadAssets);
			Commands.OnReloadAssets += Reload;
		}

		/// <summary>
		///   Reloads the assets.
		/// </summary>
		private static void Reload()
		{
			Load(ReloadAssets);
		}

		/// <summary>
		///   Loads the assets from disk.
		/// </summary>
		private static void Load(Loader loader)
		{
			var assets = Decompress(FileSystem.ReadAllBytes("Assets.pak"));
			var reader = new BufferReader(assets, Endianess.Little);

			loader(ref reader);
			reader.Dispose();
		}

		/// <summary>
		///   Decompresses the compressed content.
		/// </summary>
		private static byte[] Decompress(byte[] compressedContent)
		{
			using (var buffer = new BufferReader(compressedContent, Endianess.Little))
			{
				// Validate the header
				if (!buffer.CanRead(16))
					Log.Die("Asset bundle is corrupted: Header information missing.");

				if (Guid != new Guid(buffer.ReadByteArray(16)))
					Log.Die("Asset bundle is corrupted: The header contains an invalid hash.");

				// Decompress the bundle's content
				var content = new byte[buffer.ReadInt32()];
				using (var stream = new GZipStream(new MemoryStream(buffer.ReadByteArray()), CompressionMode.Decompress))
					stream.Read(content, 0, content.Length);

				return content;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			DisposeAssets();
			Commands.OnReloadAssets -= Reload;
		}

		private delegate void Loader(ref BufferReader reader);
	}
}