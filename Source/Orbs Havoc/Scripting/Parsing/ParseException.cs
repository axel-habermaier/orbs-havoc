namespace OrbsHavoc.Scripting.Parsing
{
	using System;
	using Utilities;

	/// <summary>
	///   Provides information about parsing errors.
	/// </summary>
	internal class ParseException : Exception
	{
		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="inputStream">The input stream the exception is raised for.</param>
		/// <param name="message">A message explaining the exception.</param>
		public ParseException(InputStream inputStream, string message)
			: base(message)
		{
			Assert.ArgumentNotNull(inputStream, nameof(inputStream));
			Assert.ArgumentNotNullOrWhitespace(message, nameof(message));

			InputStream = inputStream;
		}

		/// <summary>
		///   Gets the input stream the exception was raised for.
		/// </summary>
		public InputStream InputStream { get; }

		/// <summary>
		///   Gets zero-based position within the input where the parse error occurred.
		/// </summary>
		public int Position => InputStream.Position;

		/// <summary>
		///   Gets the input that was parsed.
		/// </summary>
		public string Input => InputStream.Input;
	}
}