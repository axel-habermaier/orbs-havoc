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
	using System.Numerics;
	using System.Runtime.InteropServices;
	using Assets;
	using Platform.Graphics;
	using Platform.Memory;
	using Utilities;
	using static Platform.Graphics.OpenGL3;
	using Buffer = Platform.Graphics.Buffer;

	/// <summary>
	///   Represents a effect that blooms the input render target.
	/// </summary>
	public class BloomEffect : FullscreenEffect
	{
		private readonly Buffer _bloomSettingsBuffer;
		private readonly Buffer _horizontalBlurBuffer;
		private readonly Buffer _verticalBlurBuffer;

		private BloomSettings _bloomSettings = new BloomSettings
		{
			BloomIntensity = 2.5f,
			BloomSaturation = 1.5f,
			BloomThreshold = 0.5f,
			SourceImageIntensity = 1,
			SourceImageSaturation = 1.1f,
			BlurSampleCount = 15
		};

		private bool _dirty = true;
		private BlurSettings _horizontalBlurSettings;
		private RenderTarget _temporaryTarget1;
		private RenderTarget _temporaryTarget2;
		private BlurSettings _verticalBlurSettings;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="inputRenderTarget">The input render target the effect should be applied to.</param>
		/// <param name="outputRenderTarget">The output render target that stores the rendered result.</param>
		public unsafe BloomEffect(RenderTarget inputRenderTarget, RenderTarget outputRenderTarget)
			: base(inputRenderTarget, outputRenderTarget)
		{
			_bloomSettingsBuffer = new Buffer(GL_UNIFORM_BUFFER, GL_STATIC_DRAW, sizeof(BloomSettings), null);
			_horizontalBlurBuffer = new Buffer(GL_UNIFORM_BUFFER, GL_STATIC_DRAW, sizeof(BlurSettings), null);
			_verticalBlurBuffer = new Buffer(GL_UNIFORM_BUFFER, GL_STATIC_DRAW, sizeof(BlurSettings), null);
		}

		/// <summary>
		///   Controls the amount of the bloom image that will be mixed into the final scene.
		/// </summary>
		public float BloomIntensity
		{
			get { return _bloomSettings.BloomIntensity; }
			set
			{
				_bloomSettings.BloomIntensity = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Independently controls the color saturation of the bloom image. Zero is totally desaturated, 1.0 leaves saturation
		///   unchanged, while higher values increase the saturation level.
		/// </summary>
		public float BloomSaturation
		{
			get { return _bloomSettings.BloomSaturation; }
			set
			{
				_bloomSettings.BloomSaturation = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Controls how bright a pixel needs to be before it will bloom. Zero makes everything bloom equally, while higher values
		///   select only brighter colors. Somewhere between 0.25 and 0.5 is good.
		/// </summary>
		public float BloomThreshold
		{
			get { return _bloomSettings.BloomThreshold; }
			set
			{
				_bloomSettings.BloomThreshold = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Controls how much blurring is applied to the bloom image. Range is from 1 up to 15.
		/// </summary>
		public int BlurSampleCount
		{
			get { return _bloomSettings.BlurSampleCount; }
			set
			{
				_bloomSettings.BlurSampleCount = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Controls the amount of the source image that will be mixed into the final scene. Range 0 to 1.
		/// </summary>
		public float SourceImageIntensity
		{
			get { return _bloomSettings.SourceImageIntensity; }
			set
			{
				_bloomSettings.SourceImageIntensity = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Independently controls the color saturation of the source image. Zero is totally desaturated, 1.0 leaves saturation
		///   unchanged, while higher values increase the saturation level.
		/// </summary>
		public float SourceImageSaturation
		{
			get { return _bloomSettings.SourceImageSaturation; }
			set
			{
				_bloomSettings.SourceImageSaturation = value;
				_dirty = true;
			}
		}

		/// <summary>
		///   Prepares the effect for rendering and returns the number of required rendering passes..
		/// </summary>
		/// <param name="size">The size of the effect's render area.</param>
		public override unsafe int PrepareForRendering(Size size)
		{
			if (_temporaryTarget1 == null || _temporaryTarget1.Size != size)
			{
				_temporaryTarget1.SafeDispose();
				_temporaryTarget2.SafeDispose();

				_temporaryTarget1 = new RenderTarget(size);
				_temporaryTarget2 = new RenderTarget(size);
			}

			if (_dirty)
			{
				UpdateBlurSettings(ref _horizontalBlurSettings, 1.0f / _temporaryTarget1.Size.Width, 0);
				UpdateBlurSettings(ref _verticalBlurSettings, 0, 1.0f / _temporaryTarget1.Size.Height);

				fixed (BloomSettings* settings = &_bloomSettings)
					_bloomSettingsBuffer.Copy(settings);

				fixed (BlurSettings* settings = &_horizontalBlurSettings)
					_horizontalBlurBuffer.Copy(settings);

				fixed (BlurSettings* settings = &_verticalBlurSettings)
					_verticalBlurBuffer.Copy(settings);

				_dirty = false;
			}

			return 4;
		}

		/// <summary>
		///   Prepares the effect for rendering the given pass.
		/// </summary>
		/// <param name="pass">The zero-based index of the pass that should be prepared.</param>
		public override RenderTarget PreparePass(int pass)
		{
			Assert.ArgumentInRange(pass, 0, 3, nameof(pass));

			switch (pass)
			{
				// Extract the brightness from the scene render target to temporary render target 1
				case 0:
					_bloomSettingsBuffer.Bind(2);
					InputRenderTarget.Texture.Bind(0);
					SamplerState.Bilinear.Bind(0);
					SamplerState.Bilinear.Bind(1);
					BlendState.Opaque.Bind();
					AssetBundle.ExtractBloomShader.Bind();

					return _temporaryTarget1;

				// Blur temporary render target 1 horizontally into temporary render target 2
				case 1:
					_temporaryTarget1.Texture.Bind(0);
					_horizontalBlurBuffer.Bind(3);
					AssetBundle.BlurShader.Bind();

					return _temporaryTarget2;

				// Blur temporary render target 2 vertically into temporary render target 1
				case 2:
					_temporaryTarget2.Texture.Bind(0);
					_verticalBlurBuffer.Bind(3);

					return _temporaryTarget1;

				// Combine scene render target and blurred image into the final render output
				case 3:
					InputRenderTarget.Texture.Bind(0);
					_temporaryTarget1.Texture.Bind(1);
					AssetBundle.CombineBloomShader.Bind();

					return OutputRenderTarget;

				default:
					return null;
			}
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_bloomSettingsBuffer.SafeDispose();
			_verticalBlurBuffer.SafeDispose();
			_horizontalBlurBuffer.SafeDispose();
			_temporaryTarget1.SafeDispose();
			_temporaryTarget2.SafeDispose();
		}

		/// <summary>
		///   Computes sample weightings and texture coordinate offsets for one pass of a separable Gaussian blur filter.
		/// </summary>
		/// <remarks>Adopted from the XNA bloom example.</remarks>
		private unsafe void UpdateBlurSettings(ref BlurSettings settings, float dx, float dy)
		{
			Assert.InRange(BlurSampleCount, 1, BlurSettings.Count + 1);

			fixed (float* sampleWeights = settings.SampleWeights)
			fixed (float* sampleOffsets = settings.SampleOffsets)
			{
				// The first sample always has a zero offset.
				sampleWeights[0] = ComputeGaussian(0);
				sampleOffsets[0] = 0;
				sampleOffsets[1] = 0;

				// Maintain a sum of all the weighting values.
				var totalWeights = sampleWeights[0];

				// Add pairs of additional sample taps, positioned
				// along a line in both directions from the center.
				for (var i = 0; i < BlurSampleCount / 2; i++)
				{
					// Store weights for the positive and negative taps.
					var weight = ComputeGaussian(i + 1);

					sampleWeights[i * 8 + 4] = weight;
					sampleWeights[i * 8 + 8] = weight;

					totalWeights += weight * 2;

					// To get the maximum amount of blurring from a limited number of
					// pixel shader samples, we take advantage of the bilinear filtering
					// hardware inside the texture fetch unit. If we position our texture
					// coordinates exactly halfway between two texels, the filtering unit
					// will average them for us, giving two samples for the price of one.
					// This allows us to step in units of two texels per sample, rather
					// than just one at a time. The 1.5 offset kicks things off by
					// positioning us nicely in between two texels.
					var sampleOffset = i * 2 + 1.5f;

					var delta = new Vector2(dx, dy) * sampleOffset;

					// Store texture coordinate offsets for the positive and negative taps.
					sampleOffsets[i * 8 + 4] = delta.X;
					sampleOffsets[i * 8 + 5] = delta.Y;
					sampleOffsets[i * 8 + 8] = -delta.X;
					sampleOffsets[i * 8 + 9] = -delta.Y;
				}

				// Normalize the list of sample weightings, so they will always sum to one.
				for (var i = 0; i < BlurSettings.Count; i++)
					sampleWeights[i * 4] /= totalWeights;
			}
		}

		/// <summary>
		///   Evaluates a single point on the Gaussian falloff curve.
		/// </summary>
		/// <remarks>Adopted from the XNA bloom example.</remarks>
		private static float ComputeGaussian(float n)
		{
			const float theta = 4;
			return (float)(1.0 / Math.Sqrt(2 * Math.PI * theta) * Math.Exp(-(n * n) / (2 * theta * theta)));
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct BloomSettings
		{
			public float SourceImageIntensity;
			public float SourceImageSaturation;
			public float BloomIntensity;
			public float BloomSaturation;
			public float BloomThreshold;
			public int BlurSampleCount;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private unsafe struct BlurSettings
		{
			public const int Count = 15;

			// Each array element is padded to a 16 byte boundary
			public fixed float SampleOffsets [Count * 4];
			public fixed float SampleWeights [Count * 4];
		}
	}
}