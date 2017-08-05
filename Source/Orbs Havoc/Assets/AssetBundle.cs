namespace OrbsHavoc.Assets
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.IO.Compression;
	using Platform.Logging;
	using Platform.Memory;
	using Scripting;
	using Utilities;

	public sealed partial class AssetBundle : DisposableObject
	{
		public AssetBundle()
		{
			Load();
			Commands.OnReloadAssets += Load;
		}

		private static void Load()
		{
			try
			{
				var stopwatch = Stopwatch.StartNew();
				var data = LoadAssetsData(File.ReadAllBytes("Assets.pak"));

				LoadAssets(new BufferReader(data, Endianess.Little));
				Log.Info($"Asset bundle loaded ({stopwatch.Elapsed.TotalMilliseconds:F2}ms).");
			}
			catch (Exception e)
			{
				throw new FatalErrorException($"Failed to load assets bundle: {e.Message.EnsureEndsWithDot()}");
			}
		}

		private static byte[] LoadAssetsData(byte[] compressedContent)
		{
			var buffer = new BufferReader(compressedContent, Endianess.Little);

			ValidateHeader(ref buffer);
			return DecompressContent(ref buffer);
		}

		private static void ValidateHeader(ref BufferReader buffer)
		{
			if (!buffer.CanRead(16))
				Log.Die("Asset bundle is corrupted: Header information missing.");

			if (Guid != new Guid(buffer.ReadByteArray(16)))
				Log.Die("Asset bundle is corrupted: The header contains an invalid hash.");
		}

		private static byte[] DecompressContent(ref BufferReader buffer)
		{
			var content = new byte[buffer.ReadInt32()];
			using (var stream = new GZipStream(new MemoryStream(buffer.ReadByteArray()), CompressionMode.Decompress))
				stream.Read(content, 0, content.Length);

			return content;
		}

		protected override void OnDisposing()
		{
			DisposeAssets();
			Commands.OnReloadAssets -= Load;
		}
	}
}