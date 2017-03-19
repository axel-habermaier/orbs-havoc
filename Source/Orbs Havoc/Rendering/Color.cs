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

namespace OrbsHavoc.Rendering
{
	using System;
	using System.Numerics;
	using System.Runtime.InteropServices;
	using Utilities;

	/// <summary>
	///   Represents a 32-bit color (4 bytes) in the form of RGBA.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct Color : IEquatable<Color>
	{
		/// <summary>
		///   The red component of the color.
		/// </summary>
		public byte Red;

		/// <summary>
		///   The green component of the color.
		/// </summary>
		public byte Green;

		/// <summary>
		///   The blue component of the color.
		/// </summary>
		public byte Blue;

		/// <summary>
		///   The alpha component of the color.
		/// </summary>
		public byte Alpha;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="red">The red component of the color.</param>
		/// <param name="green">The green component of the color.</param>
		/// <param name="blue">The blue component of the color.</param>
		/// <param name="alpha">The alpha component of the color.</param>
		public Color(byte red, byte green, byte blue, byte alpha)
			: this(ToFloat(red), ToFloat(green), ToFloat(blue), ToFloat(alpha))
		{
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="red">The red component of the color.</param>
		/// <param name="green">The green component of the color.</param>
		/// <param name="blue">The blue component of the color.</param>
		/// <param name="alpha">The alpha component of the color.</param>
		public Color(float red, float green, float blue, float alpha)
		{
			Red = ToByte(red * alpha);
			Green = ToByte(green * alpha);
			Blue = ToByte(blue * alpha);
			Alpha = ToByte(alpha);
		}

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="argb">A packed unsigned integer containing all four color components.</param>
		public Color(uint argb)
			: this(ToFloat((byte)((argb >> 16) & 255)),
				ToFloat((byte)((argb >> 8) & 255)),
				ToFloat((byte)(argb & 255)),
				ToFloat((byte)((argb >> 24) & 255)))
		{
		}

		/// <summary>
		///   Converts the color into a packed integer.
		/// </summary>
		/// <returns>A packed integer containing all four color components.</returns>
		public uint ToArgb()
		{
			uint value = Blue;
			value |= (uint)(Green << 8);
			value |= (uint)(Red << 16);
			value |= (uint)(Alpha << 24);

			return value;
		}

		/// <summary>
		///   Tests for equality between two colors.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		public static bool operator ==(Color left, Color right)
		{
			return left.Equals(right);
		}

		/// <summary>
		///   Tests for inequality between two colors.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		public static bool operator !=(Color left, Color right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		///   Returns a string representation of this color.
		/// </summary>
		public override string ToString()
		{
			return $"Red: {Red}, Green: {Green}, Blue: {Blue}, Alpha: {Alpha}";
		}

		/// <summary>
		///   Returns a hash code for this color.
		/// </summary>
		public override int GetHashCode()
		{
			return Alpha.GetHashCode() + Red.GetHashCode() + Green.GetHashCode() + Blue.GetHashCode();
		}

		/// <summary>
		///   Determines whether the given color is equal to this color.
		/// </summary>
		/// <param name="other">The other color to compare with this color.</param>
		public bool Equals(Color other)
		{
			return Red == other.Red && Green == other.Green && Blue == other.Blue && Alpha == other.Alpha;
		}

		/// <summary>
		///   Determines whether the specified object is equal to this color.
		/// </summary>
		/// <param name="value">The object to compare with this color.</param>
		public override bool Equals(object value)
		{
			if (value == null)
				return false;

			if (!ReferenceEquals(value.GetType(), typeof(Color)))
				return false;

			return Equals((Color)value);
		}

		/// <summary>
		///   Stores the color information in the given array in RGBA format.
		/// </summary>
		/// <param name="color">The array to which the color information should be copied.</param>
		public void ToFloatArray(float[] color)
		{
			Assert.ArgumentNotNull(color, nameof(color));
			Assert.ArgumentSatisfies(color.Length == 4, nameof(color), "Array has wrong size.");

			color[0] = ToFloat(Red);
			color[1] = ToFloat(Green);
			color[2] = ToFloat(Blue);
			color[3] = ToFloat(Alpha);
		}

		/// <summary>
		///   Stores the color information in the given array in RGBA format.
		/// </summary>
		/// <param name="color">The array to which the color information should be copied.</param>
		public unsafe void ToFloatArray(float* color)
		{
			Assert.ArgumentNotNull(new IntPtr(color), nameof(color));

			color[0] = ToFloat(Red);
			color[1] = ToFloat(Green);
			color[2] = ToFloat(Blue);
			color[3] = ToFloat(Alpha);
		}

		/// <summary>
		///   Converts a byte value to a floating point value.
		/// </summary>
		/// <param name="component">The value that should be converted.</param>
		private static float ToFloat(byte component)
		{
			return component / 255.0f;
		}

		/// <summary>
		///   Converts a floating point value to a byte value.
		/// </summary>
		/// <param name="component">The value that should be converted.</param>
		private static byte ToByte(float component)
		{
			var value = (int)(component * 255.0f);
			return (byte)(value < 0 ? 0 : value > 255 ? 255 : value);
		}

		/// <summary>
		///   Creates a color from hue, saturation, and value.
		/// </summary>
		/// <param name="color">The HSV color.</param>
		public static Color FromHsv(Vector4 color)
		{
			var hue = color.X * 360.0f;
			var saturation = color.Y;
			var value = color.Z;

			var c = value * saturation;
			var h = hue / 60.0f;
			var x = c * (1.0f - Math.Abs(h % 2.0f - 1.0f));

			float r, g, b;
			if (0.0f <= h && h < 1.0f)
			{
				r = c;
				g = x;
				b = 0.0f;
			}
			else if (1.0f <= h && h < 2.0f)
			{
				r = x;
				g = c;
				b = 0.0f;
			}
			else if (2.0f <= h && h < 3.0f)
			{
				r = 0.0f;
				g = c;
				b = x;
			}
			else if (3.0f <= h && h < 4.0f)
			{
				r = 0.0f;
				g = x;
				b = c;
			}
			else if (4.0f <= h && h < 5.0f)
			{
				r = x;
				g = 0.0f;
				b = c;
			}
			else if (5.0f <= h && h < 6.0f)
			{
				r = c;
				g = 0.0f;
				b = x;
			}
			else
			{
				r = 0.0f;
				g = 0.0f;
				b = 0.0f;
			}

			var m = value - c;

			return new Color(r + m, g + m, b + m, color.W);
		}

		/// <summary>
		///   Converts RGB color values to HSV color values.
		/// </summary>
		public Vector4 ToHsv()
		{
			var red = ToFloat(Red);
			var green = ToFloat(Green);
			var blue = ToFloat(Blue);
			var alpha = ToFloat(Alpha);

			var max = Math.Max(red, Math.Max(green, blue));
			var min = Math.Min(red, Math.Min(green, blue));
			var c = max - min;

			var h = 0.0f;
			// ReSharper disable CompareOfFloatsByEqualityOperator
			if (max == red)
				h = (green - blue) / c % 6.0f;
			else if (max == green)
				h = (blue - red) / c + 2.0f;
			else if (max == blue)
				h = (red - green) / c + 4.0f;

			var hue = h * 60.0f;
			if (hue < 0.0f)
				hue += 360.0f;

			var saturation = 0.0f;
			if (max != 0)
				saturation = c / max;
			// ReSharper restore CompareOfFloatsByEqualityOperator

			return new Vector4(hue / 360.0f, saturation, max, alpha);
		}
	}
}