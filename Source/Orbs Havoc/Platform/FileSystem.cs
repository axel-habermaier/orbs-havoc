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

namespace OrbsHavoc.Platform
{
	using System;
	using System.IO;
	using System.Linq;
	using System.Text;
	using JetBrains.Annotations;
	using Utilities;

	/// <summary>
	///   Provides access to the operating system's file system.
	/// </summary>
	internal static class FileSystem
	{
		/// <summary>
		///   The number of spaces per tab.
		/// </summary>
		private const int SpacesPerTab = 4;

		/// <summary>
		///   Initializes the type.
		/// </summary>
		static FileSystem()
		{
			UserDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Application.Name).Replace("\\", "/");
			Directory.CreateDirectory(UserDirectory);
		}

		/// <summary>
		///   Gets the path to the user directory.
		/// </summary>
		public static string UserDirectory { get; }

		/// <summary>
		///   Checks whether the given file name is valid.
		/// </summary>
		/// <param name="fileName">The file name that should be checked.</param>
		public static bool IsValidFileName(string fileName)
		{
			Assert.ArgumentNotNull(fileName, nameof(fileName));

			if (String.IsNullOrWhiteSpace(fileName))
				return false;

			return fileName.ToCharArray().Any(c => Char.IsLetterOrDigit(c) || c == '_' || c == '.');
		}

		/// <summary>
		///   Reads all bytes of the file at the given path. This method can only read files that were shipped with the application.
		/// </summary>
		/// <param name="path">The path of the file that should be read.</param>
		public static byte[] ReadAllBytes(string path)
		{
			Assert.ArgumentNotNullOrWhitespace(path, nameof(path));
			Assert.That(IsValidFileName(path), "Invalid file name.");

			try
			{
				return File.ReadAllBytes(path);
			}
			catch (Exception e)
			{
				throw new FileSystemException(e.Message);
			}
		}

		/// <summary>
		///   Reads the entire UTF8-encoded text content of the file and returns it as a string. This method can only read files in
		///   the application's user directory.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be read.</param>
		public static string ReadAllText(string fileName)
		{
			try
			{
				return Normalize(File.ReadAllText(GetUserFileName(fileName)));
			}
			catch (Exception e)
			{
				throw new FileSystemException(e.Message);
			}
		}

		/// <summary>
		///   Normalizes the line endings of the given input string to '\n', and replaces all tabs with spaces.
		/// </summary>
		/// <param name="input">The input whose line endings should be normalized.</param>
		private static string Normalize(string input)
		{
			return input.Replace("\r\n", "\n")
						.Replace("\r", "\n")
						.Replace("\t", new string(' ', SpacesPerTab));
		}

		/// <summary>
		///   Writes the UTF8-encoded content to the file. If the file does not yet exist, it is created. If it does exist, its
		///   contents are overwritten. This method can only write files in the application's user directory.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be written.</param>
		/// <param name="content">The content that should be written to the file.</param>
		public static void WriteAllText(string fileName, string content)
		{
			Assert.ArgumentNotNull(content, nameof(content));

			try
			{
				File.WriteAllText(GetUserFileName(fileName), content, Encoding.UTF8);
			}
			catch (Exception e)
			{
				throw new FileSystemException(e.Message);
			}
		}

		/// <summary>
		///   Appends the UTF8-encoded content to the file. If the file does not yet exist, it is created. This method can only write
		///   files in the application's user directory.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be written.</param>
		/// <param name="content">The content that should be written to the file.</param>
		public static void AppendText(string fileName, string content)
		{
			Assert.ArgumentNotNull(content, nameof(content));

			try
			{
				File.AppendAllText(GetUserFileName(fileName), content, Encoding.UTF8);
			}
			catch (Exception e)
			{
				throw new FileSystemException(e.Message);
			}
		}

		/// <summary>
		///   Deletes the file with the given name. This method can only read files in
		///   the application's user directory.
		/// </summary>
		/// <param name="fileName">The name of the file in the application's user directory that should be deleted.</param>
		public static void Delete(string fileName)
		{
			File.Delete(GetUserFileName(fileName));
		}

		/// <summary>
		///   Gets the path to the file with the given name in the application's user directory.
		/// </summary>
		/// <param name="fileName">The name of the file.</param>
		private static string GetUserFileName([NotNull] string fileName)
		{
			Assert.ArgumentNotNull(fileName, nameof(fileName));
			Assert.That(IsValidFileName(fileName), "Invalid file name.");
			Assert.That(Path.GetFileName(fileName) == fileName, "Expected a file name without a path.");

			return Path.Combine(UserDirectory, fileName);
		}
	}
}