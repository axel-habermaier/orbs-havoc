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

namespace PointWars.Gameplay.Client
{
	using Assets;
	using Platform.Memory;
	using Rendering;
	using Rendering.Particles;
	using Utilities;

	public sealed class ParticleEffects : DisposableObject
	{
		public readonly ParticleEffectTemplate AvatarExplosion = new ParticleEffectTemplate(() =>
			new ParticleEffect
			{
				Emitters =
				{
					new Emitter
					{
						Capacity = 200,
						Duration = 0.5f,
						EmissionRate = 500,
						InitialColor = new Range<Color>(new Color(0xFFFFF202), new Color(0xFFFF9900)),
						InitialLifetime = new Range<float>(0.1f, 0.5f),
						InitialScale = new Range<float>(1),
						InitialSpeed = new Range<float>(200, 400),
						Texture = AssetBundle.Bullet,
						Modifiers =
						{
							new FadeOutModifier()
						}
					}
				}
			});

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			AvatarExplosion.SafeDispose();
		}
	}
}