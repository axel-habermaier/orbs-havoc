namespace OrbsHavoc.Platform.Graphics
{
	using Utilities;
	using static OpenGL3;

	/// <summary>
	///   Describes a blend operation of the output merger pipeline stage.
	/// </summary>
	internal enum BlendOperation
	{
		/// <summary>
		///   Indicates that no blending should be used and all drawn objects are opaque.
		/// </summary>
		Opaque = 1,

		/// <summary>
		///   Indicates that pre-multiplied alpha blending should be used.
		/// </summary>
		Premultiplied,

		/// <summary>
		///   Indicates that additive blending should be used.
		/// </summary>
		Additive,

		/// <summary>
		///   Indicates that alpha blending should be used.
		/// </summary>
		Alpha
	}

	/// <summary>
	///   Provides extension methods for blend operations.
	/// </summary>
	internal static class BlendOperationExtensions
	{
		/// <summary>
		///   Binds the blend state for rendering.
		/// </summary>
		public static void Bind(this BlendOperation blendOperation)
		{
			if (!Change(ref State.BlendOperation, blendOperation))
				return;

			switch (blendOperation)
			{
				case BlendOperation.Opaque:
					glDisable(GL_BLEND);
					break;
				case BlendOperation.Premultiplied:
					glEnable(GL_BLEND);
					glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
					break;
				case BlendOperation.Additive:
					glEnable(GL_BLEND);
					glBlendFunc(GL_ONE, GL_ONE);
					break;
				case BlendOperation.Alpha:
					glEnable(GL_BLEND);
					glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
					break;
				default:
					Assert.NotReached("Unknown blend state.");
					break;
			}
		}
	}
}