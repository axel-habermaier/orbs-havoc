namespace OrbsHavoc.Assets
{
	using System;
	using System.IO;
	using System.IO.Compression;
	using Platform;
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
			var assets = Decompress(FileSystem.ReadAllBytes("Assets.pak"));

			LoadAssets(new BufferReader(assets, Endianess.Little));
			Log.Info($"Asset bundle loaded ({(Clock.GetTime() - start) * 1000:F2}ms).");
		}

		/// <summary>
		///   Decompresses the compressed content.
		/// </summary>
		private static byte[] Decompress(byte[] compressedContent)
		{
			var buffer = new BufferReader(compressedContent, Endianess.Little);

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