﻿namespace OrbsHavoc.Platform
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using JetBrains.Annotations;
	using Utilities;

	internal static class UserFile
	{
		private const int SpacesPerTab = 4;

		static UserFile()
		{
			UserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.Name).Replace("\\", "/");
			Directory.CreateDirectory(UserDirectory);
		}

		public static string UserDirectory { get; }

		public static bool IsValidFileName(string fileName)
		{
			Assert.ArgumentNotNull(fileName, nameof(fileName));

			if (String.IsNullOrWhiteSpace(fileName))
				return false;

			return fileName.ToCharArray().Any(c => Char.IsLetterOrDigit(c) || c == '_' || c == '.');
		}

		public static string ReadAllText(string fileName)
		{
			return Normalize(File.ReadAllText(GetPath(fileName)));
		}

		public static void WriteAllText(string fileName, string content)
		{
			Assert.ArgumentNotNull(content, nameof(content));
			File.WriteAllText(GetPath(fileName), content, Encoding.UTF8);
		}

		public static void AppendText(string fileName, string content)
		{
			Assert.ArgumentNotNull(content, nameof(content));
			File.AppendAllText(GetPath(fileName), content, Encoding.UTF8);
		}

		public static void Delete(string fileName)
		{
			File.Delete(GetPath(fileName));
		}

		private static string Normalize(string input)
		{
			return input.Replace("\r\n", "\n")
						.Replace("\r", "\n")
						.Replace("\t", new String(' ', SpacesPerTab));
		}

		private static string GetPath([NotNull] string fileName)
		{
			Assert.ArgumentNotNull(fileName, nameof(fileName));
			Assert.That(IsValidFileName(fileName), "Invalid file name.");
			Assert.That(Path.GetFileName(fileName) == fileName, "Expected a file name without a path.");

			return Path.Combine(UserDirectory, fileName);
		}
	}
}