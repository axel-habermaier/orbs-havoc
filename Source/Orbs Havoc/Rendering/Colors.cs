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
	/// <summary>
	///   Provides access to several predefined colors.
	/// </summary>
	public static class Colors
	{
		public static readonly Color Black = new Color(0, 0, 0, 255);
		public static readonly Color White = new Color(255, 255, 255, 255);
		public static readonly Color AliceBlue = new Color(0.941176534f, 0.972549081f, 1.000000000f, 1.000000000f);
		public static readonly Color AntiqueWhite = new Color(0.980392218f, 0.921568692f, 0.843137324f, 1.000000000f);
		public static readonly Color Aqua = new Color(0.000000000f, 1.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color Aquamarine = new Color(0.498039246f, 1.000000000f, 0.831372619f, 1.000000000f);
		public static readonly Color Azure = new Color(0.941176534f, 1.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color Beige = new Color(0.960784376f, 0.960784376f, 0.862745166f, 1.000000000f);
		public static readonly Color Bisque = new Color(1.000000000f, 0.894117713f, 0.768627524f, 1.000000000f);
		public static readonly Color BlanchedAlmond = new Color(1.000000000f, 0.921568692f, 0.803921640f, 1.000000000f);
		public static readonly Color Blue = new Color(0.000000000f, 0.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color BlueViolet = new Color(0.541176498f, 0.168627456f, 0.886274576f, 1.000000000f);
		public static readonly Color Brown = new Color(0.647058845f, 0.164705887f, 0.164705887f, 1.000000000f);
		public static readonly Color BurlyWood = new Color(0.870588303f, 0.721568644f, 0.529411793f, 1.000000000f);
		public static readonly Color CadetBlue = new Color(0.372549027f, 0.619607866f, 0.627451003f, 1.000000000f);
		public static readonly Color Chartreuse = new Color(0.498039246f, 1.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color Chocolate = new Color(0.823529482f, 0.411764741f, 0.117647067f, 1.000000000f);
		public static readonly Color Coral = new Color(1.000000000f, 0.498039246f, 0.313725501f, 1.000000000f);
		public static readonly Color CornflowerBlue = new Color(0.392156899f, 0.584313750f, 0.929411829f, 1.000000000f);
		public static readonly Color Cornsilk = new Color(1.000000000f, 0.972549081f, 0.862745166f, 1.000000000f);
		public static readonly Color Crimson = new Color(0.862745166f, 0.078431375f, 0.235294133f, 1.000000000f);
		public static readonly Color Cyan = new Color(0.000000000f, 1.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color DarkBlue = new Color(0.000000000f, 0.000000000f, 0.545098066f, 1.000000000f);
		public static readonly Color DarkCyan = new Color(0.000000000f, 0.545098066f, 0.545098066f, 1.000000000f);
		public static readonly Color DarkGoldenrod = new Color(0.721568644f, 0.525490224f, 0.043137256f, 1.000000000f);
		public static readonly Color DarkGray = new Color(0.662745118f, 0.662745118f, 0.662745118f, 1.000000000f);
		public static readonly Color DarkGreen = new Color(0.000000000f, 0.392156899f, 0.000000000f, 1.000000000f);
		public static readonly Color DarkKhaki = new Color(0.741176486f, 0.717647076f, 0.419607878f, 1.000000000f);
		public static readonly Color DarkMagenta = new Color(0.545098066f, 0.000000000f, 0.545098066f, 1.000000000f);
		public static readonly Color DarkOliveGreen = new Color(0.333333343f, 0.419607878f, 0.184313729f, 1.000000000f);
		public static readonly Color DarkOrange = new Color(1.000000000f, 0.549019635f, 0.000000000f, 1.000000000f);
		public static readonly Color DarkOrchid = new Color(0.600000024f, 0.196078449f, 0.800000072f, 1.000000000f);
		public static readonly Color DarkRed = new Color(0.545098066f, 0.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color DarkSalmon = new Color(0.913725555f, 0.588235319f, 0.478431404f, 1.000000000f);
		public static readonly Color DarkSeaGreen = new Color(0.560784340f, 0.737254918f, 0.545098066f, 1.000000000f);
		public static readonly Color DarkSlateBlue = new Color(0.282352954f, 0.239215702f, 0.545098066f, 1.000000000f);
		public static readonly Color DarkSlateGray = new Color(0.184313729f, 0.309803933f, 0.309803933f, 1.000000000f);
		public static readonly Color DarkTurquoise = new Color(0.000000000f, 0.807843208f, 0.819607913f, 1.000000000f);
		public static readonly Color DarkViolet = new Color(0.580392182f, 0.000000000f, 0.827451050f, 1.000000000f);
		public static readonly Color DeepPink = new Color(1.000000000f, 0.078431375f, 0.576470613f, 1.000000000f);
		public static readonly Color DeepSkyBlue = new Color(0.000000000f, 0.749019623f, 1.000000000f, 1.000000000f);
		public static readonly Color DimGray = new Color(0.411764741f, 0.411764741f, 0.411764741f, 1.000000000f);
		public static readonly Color DodgerBlue = new Color(0.117647067f, 0.564705908f, 1.000000000f, 1.000000000f);
		public static readonly Color Firebrick = new Color(0.698039234f, 0.133333340f, 0.133333340f, 1.000000000f);
		public static readonly Color FloralWhite = new Color(1.000000000f, 0.980392218f, 0.941176534f, 1.000000000f);
		public static readonly Color ForestGreen = new Color(0.133333340f, 0.545098066f, 0.133333340f, 1.000000000f);
		public static readonly Color Fuchsia = new Color(1.000000000f, 0.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color Gainsboro = new Color(0.862745166f, 0.862745166f, 0.862745166f, 1.000000000f);
		public static readonly Color GhostWhite = new Color(0.972549081f, 0.972549081f, 1.000000000f, 1.000000000f);
		public static readonly Color Gold = new Color(1.000000000f, 0.843137324f, 0.000000000f, 1.000000000f);
		public static readonly Color Goldenrod = new Color(0.854902029f, 0.647058845f, 0.125490203f, 1.000000000f);
		public static readonly Color Gray = new Color(0.501960814f, 0.501960814f, 0.501960814f, 1.000000000f);
		public static readonly Color Green = new Color(0.000000000f, 0.501960814f, 0.000000000f, 1.000000000f);
		public static readonly Color GreenYellow = new Color(0.678431392f, 1.000000000f, 0.184313729f, 1.000000000f);
		public static readonly Color Honeydew = new Color(0.941176534f, 1.000000000f, 0.941176534f, 1.000000000f);
		public static readonly Color HotPink = new Color(1.000000000f, 0.411764741f, 0.705882370f, 1.000000000f);
		public static readonly Color IndianRed = new Color(0.803921640f, 0.360784322f, 0.360784322f, 1.000000000f);
		public static readonly Color Indigo = new Color(0.294117659f, 0.000000000f, 0.509803951f, 1.000000000f);
		public static readonly Color Ivory = new Color(1.000000000f, 1.000000000f, 0.941176534f, 1.000000000f);
		public static readonly Color Khaki = new Color(0.941176534f, 0.901960850f, 0.549019635f, 1.000000000f);
		public static readonly Color Lavender = new Color(0.901960850f, 0.901960850f, 0.980392218f, 1.000000000f);
		public static readonly Color LavenderBlush = new Color(1.000000000f, 0.941176534f, 0.960784376f, 1.000000000f);
		public static readonly Color LawnGreen = new Color(0.486274540f, 0.988235354f, 0.000000000f, 1.000000000f);
		public static readonly Color LemonChiffon = new Color(1.000000000f, 0.980392218f, 0.803921640f, 1.000000000f);
		public static readonly Color LightBlue = new Color(0.678431392f, 0.847058892f, 0.901960850f, 1.000000000f);
		public static readonly Color LightCoral = new Color(0.941176534f, 0.501960814f, 0.501960814f, 1.000000000f);
		public static readonly Color LightCyan = new Color(0.878431439f, 1.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color LightGoldenrodYellow = new Color(0.980392218f, 0.980392218f, 0.823529482f, 1.000000000f);
		public static readonly Color LightGreen = new Color(0.564705908f, 0.933333397f, 0.564705908f, 1.000000000f);
		public static readonly Color LightGray = new Color(0.827451050f, 0.827451050f, 0.827451050f, 1.000000000f);
		public static readonly Color LightPink = new Color(1.000000000f, 0.713725507f, 0.756862819f, 1.000000000f);
		public static readonly Color LightSalmon = new Color(1.000000000f, 0.627451003f, 0.478431404f, 1.000000000f);
		public static readonly Color LightSeaGreen = new Color(0.125490203f, 0.698039234f, 0.666666687f, 1.000000000f);
		public static readonly Color LightSkyBlue = new Color(0.529411793f, 0.807843208f, 0.980392218f, 1.000000000f);
		public static readonly Color LightSlateGray = new Color(0.466666698f, 0.533333361f, 0.600000024f, 1.000000000f);
		public static readonly Color LightSteelBlue = new Color(0.690196097f, 0.768627524f, 0.870588303f, 1.000000000f);
		public static readonly Color LightYellow = new Color(1.000000000f, 1.000000000f, 0.878431439f, 1.000000000f);
		public static readonly Color Lime = new Color(0.000000000f, 1.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color LimeGreen = new Color(0.196078449f, 0.803921640f, 0.196078449f, 1.000000000f);
		public static readonly Color Linen = new Color(0.980392218f, 0.941176534f, 0.901960850f, 1.000000000f);
		public static readonly Color Magenta = new Color(1.000000000f, 0.000000000f, 1.000000000f, 1.000000000f);
		public static readonly Color Maroon = new Color(0.501960814f, 0.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color MediumAquamarine = new Color(0.400000036f, 0.803921640f, 0.666666687f, 1.000000000f);
		public static readonly Color MediumBlue = new Color(0.000000000f, 0.000000000f, 0.803921640f, 1.000000000f);
		public static readonly Color MediumOrchid = new Color(0.729411781f, 0.333333343f, 0.827451050f, 1.000000000f);
		public static readonly Color MediumPurple = new Color(0.576470613f, 0.439215720f, 0.858823597f, 1.000000000f);
		public static readonly Color MediumSeaGreen = new Color(0.235294133f, 0.701960802f, 0.443137288f, 1.000000000f);
		public static readonly Color MediumSlateBlue = new Color(0.482352972f, 0.407843173f, 0.933333397f, 1.000000000f);
		public static readonly Color MediumSpringGreen = new Color(0.000000000f, 0.980392218f, 0.603921592f, 1.000000000f);
		public static readonly Color MediumTurquoise = new Color(0.282352954f, 0.819607913f, 0.800000072f, 1.000000000f);
		public static readonly Color MediumVioletRed = new Color(0.780392230f, 0.082352944f, 0.521568656f, 1.000000000f);
		public static readonly Color MidnightBlue = new Color(0.098039225f, 0.098039225f, 0.439215720f, 1.000000000f);
		public static readonly Color MintCream = new Color(0.960784376f, 1.000000000f, 0.980392218f, 1.000000000f);
		public static readonly Color MistyRose = new Color(1.000000000f, 0.894117713f, 0.882353008f, 1.000000000f);
		public static readonly Color Moccasin = new Color(1.000000000f, 0.894117713f, 0.709803939f, 1.000000000f);
		public static readonly Color NavajoWhite = new Color(1.000000000f, 0.870588303f, 0.678431392f, 1.000000000f);
		public static readonly Color Navy = new Color(0.000000000f, 0.000000000f, 0.501960814f, 1.000000000f);
		public static readonly Color OldLace = new Color(0.992156923f, 0.960784376f, 0.901960850f, 1.000000000f);
		public static readonly Color Olive = new Color(0.501960814f, 0.501960814f, 0.000000000f, 1.000000000f);
		public static readonly Color OliveDrab = new Color(0.419607878f, 0.556862772f, 0.137254909f, 1.000000000f);
		public static readonly Color Orange = new Color(1.000000000f, 0.647058845f, 0.000000000f, 1.000000000f);
		public static readonly Color OrangeRed = new Color(1.000000000f, 0.270588249f, 0.000000000f, 1.000000000f);
		public static readonly Color Orchid = new Color(0.854902029f, 0.439215720f, 0.839215755f, 1.000000000f);
		public static readonly Color PaleGoldenrod = new Color(0.933333397f, 0.909803987f, 0.666666687f, 1.000000000f);
		public static readonly Color PaleGreen = new Color(0.596078455f, 0.984313786f, 0.596078455f, 1.000000000f);
		public static readonly Color PaleTurquoise = new Color(0.686274529f, 0.933333397f, 0.933333397f, 1.000000000f);
		public static readonly Color PaleVioletRed = new Color(0.858823597f, 0.439215720f, 0.576470613f, 1.000000000f);
		public static readonly Color PapayaWhip = new Color(1.000000000f, 0.937254965f, 0.835294187f, 1.000000000f);
		public static readonly Color PeachPuff = new Color(1.000000000f, 0.854902029f, 0.725490212f, 1.000000000f);
		public static readonly Color Peru = new Color(0.803921640f, 0.521568656f, 0.247058839f, 1.000000000f);
		public static readonly Color Pink = new Color(1.000000000f, 0.752941251f, 0.796078503f, 1.000000000f);
		public static readonly Color Plum = new Color(0.866666734f, 0.627451003f, 0.866666734f, 1.000000000f);
		public static readonly Color PowderBlue = new Color(0.690196097f, 0.878431439f, 0.901960850f, 1.000000000f);
		public static readonly Color Purple = new Color(0.501960814f, 0.000000000f, 0.501960814f, 1.000000000f);
		public static readonly Color Red = new Color(1.000000000f, 0.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color RosyBrown = new Color(0.737254918f, 0.560784340f, 0.560784340f, 1.000000000f);
		public static readonly Color RoyalBlue = new Color(0.254901975f, 0.411764741f, 0.882353008f, 1.000000000f);
		public static readonly Color SaddleBrown = new Color(0.545098066f, 0.270588249f, 0.074509807f, 1.000000000f);
		public static readonly Color Salmon = new Color(0.980392218f, 0.501960814f, 0.447058856f, 1.000000000f);
		public static readonly Color SandyBrown = new Color(0.956862807f, 0.643137276f, 0.376470625f, 1.000000000f);
		public static readonly Color SeaGreen = new Color(0.180392161f, 0.545098066f, 0.341176480f, 1.000000000f);
		public static readonly Color SeaShell = new Color(1.000000000f, 0.960784376f, 0.933333397f, 1.000000000f);
		public static readonly Color Sienna = new Color(0.627451003f, 0.321568638f, 0.176470593f, 1.000000000f);
		public static readonly Color Silver = new Color(0.752941251f, 0.752941251f, 0.752941251f, 1.000000000f);
		public static readonly Color SkyBlue = new Color(0.529411793f, 0.807843208f, 0.921568692f, 1.000000000f);
		public static readonly Color SlateBlue = new Color(0.415686309f, 0.352941185f, 0.803921640f, 1.000000000f);
		public static readonly Color SlateGray = new Color(0.439215720f, 0.501960814f, 0.564705908f, 1.000000000f);
		public static readonly Color Snow = new Color(1.000000000f, 0.980392218f, 0.980392218f, 1.000000000f);
		public static readonly Color SpringGreen = new Color(0.000000000f, 1.000000000f, 0.498039246f, 1.000000000f);
		public static readonly Color SteelBlue = new Color(0.274509817f, 0.509803951f, 0.705882370f, 1.000000000f);
		public static readonly Color Tan = new Color(0.823529482f, 0.705882370f, 0.549019635f, 1.000000000f);
		public static readonly Color Teal = new Color(0.000000000f, 0.501960814f, 0.501960814f, 1.000000000f);
		public static readonly Color Thistle = new Color(0.847058892f, 0.749019623f, 0.847058892f, 1.000000000f);
		public static readonly Color Tomato = new Color(1.000000000f, 0.388235331f, 0.278431386f, 1.000000000f);
		public static readonly Color Transparent = new Color(0.000000000f, 0.000000000f, 0.000000000f, 0.000000000f);
		public static readonly Color Turquoise = new Color(0.250980407f, 0.878431439f, 0.815686345f, 1.000000000f);
		public static readonly Color Violet = new Color(0.933333397f, 0.509803951f, 0.933333397f, 1.000000000f);
		public static readonly Color Wheat = new Color(0.960784376f, 0.870588303f, 0.701960802f, 1.000000000f);
		public static readonly Color WhiteSmoke = new Color(0.960784376f, 0.960784376f, 0.960784376f, 1.000000000f);
		public static readonly Color Yellow = new Color(1.000000000f, 1.000000000f, 0.000000000f, 1.000000000f);
		public static readonly Color YellowGreen = new Color(0.603921592f, 0.803921640f, 0.196078449f, 1.000000000f);
	}
}