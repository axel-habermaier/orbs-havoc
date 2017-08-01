// The MIT License (MIT)
// 
// Copyright (c) 2012-2017, Axel Habermaier
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

namespace OrbsHavoc.Assets
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using System.Reflection;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using Utilities;

	/// <summary>
	///   Provides access to assets used throughout the application.
	/// </summary>
	public sealed partial class AssetBundle : DisposableObject
	{
		/// <summary>
		///   Initializes the assets.
		/// </summary>
		public AssetBundle()
		{
			InitializeAssets();
			Load();

			Commands.OnReloadAssets += Load;
		}

		/// <summary>
		///   Loads the assets from disk.
		/// </summary>
		private static void Load()
		{
			var start = Clock.GetTime();
			var assets = Decompress(Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName));

			LoadAssets(new BufferReader(assets, Endianess.Little));
			Log.Info($"Asset bundle loaded ({(Clock.GetTime() - start) * 1000:F2}ms).");
		}

		/// <summary>
		///   Decompresses the compressed content.
		/// </summary>
		private static byte[] Decompress(Stream compressedContent)
		{
			// Validate the header
			if (compressedContent.Length < 16)
				Log.Die("Asset bundle is corrupted: Header information missing.");

			var guid = new byte[16];
			compressedContent.Read(guid, 0, guid.Length);

			if (Guid != new Guid(guid))
				Log.Die("Asset bundle is corrupted: The header contains an invalid hash.");

			// Decompress the bundle's content
			var contentLength = new byte[4];
			compressedContent.Read(contentLength, 0, contentLength.Length);
			
			var content = new byte[BitConverter.ToInt32(contentLength, 0)];
			using (var stream = new GZipStream(compressedContent, CompressionMode.Decompress))
				stream.Read(content, 0, content.Length);

			return content;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			DisposeAssets();
			Commands.OnReloadAssets -= Load;
		}
	}
}