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

namespace OrbsHavoc.Rendering.Particles
{
	using System;
	using Platform.Memory;

	/// <summary>
	///   Represents a template from which a certain particle effect can be created.
	/// </summary>
	public sealed class ParticleEffectTemplate : DisposableObject
	{
		private readonly ObjectPool<ParticleEffect> _pooledEffects;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="initializationCallback">
		///   The function that should be used by the template to initialize a newly allocated particle effect.
		/// </param>
		public ParticleEffectTemplate(Action<ParticleEffect> initializationCallback)
		{
			_pooledEffects = new ObjectPool<ParticleEffect>(initializationCallback);
		}

		/// <summary>
		///   Gets a pooled effect or allocates a new instance if none are currently pooled.
		/// </summary>
		public ParticleEffect Allocate()
		{
			var effect = _pooledEffects.Allocate();
			effect.Reset();

			return effect;
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			_pooledEffects.SafeDispose();
		}
	}
}