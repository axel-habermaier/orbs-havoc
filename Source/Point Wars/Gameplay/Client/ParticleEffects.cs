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
	using System;
	using Assets;
	using Platform.Memory;
	using Rendering;
	using Rendering.Particles;
	using Utilities;

	/// <summary>
	///   Provides access to particle effect templates.
	/// </summary>
	internal sealed class ParticleEffects : DisposableObject
	{
		public readonly ParticleEffectTemplate AvatarCore;
		public readonly ParticleEffectTemplate AvatarExplosion;
		public readonly ParticleEffectTemplate Bullet;
		public readonly ParticleEffectTemplate BulletExplosion;
		public readonly ParticleEffectTemplate Collectible;
		public readonly ParticleEffectTemplate Regeneration;

		/// <summary>
		///   Initializes a new instance.
		/// </summary>
		/// <param name="gameSession">The game session the particle effects are provided for.</param>
		public ParticleEffects(GameSession gameSession)
		{
			Assert.ArgumentNotNull(gameSession, nameof(gameSession));

			var fadeOutModifier = new FadeOutModifier();
			var velocityOrientationModifier = new VelocityOrientationModifier();
			var speedModifier = new SpeedModifier();

			AvatarCore = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 100,
						Duration = Single.PositiveInfinity,
						EmissionRate = 100,
						ColorRange = new Range<Color>(new Color(0xFFFFF202), new Color(0xFFFF9900)),
						LiftetimeRange = 0.04f,
						ScaleRange = new Range<float>(0.6f, 1.1f),
						SpeedRange = 0,
						Texture = AssetBundle.RoundParticle,
						Modifiers =
						{
							new FadeOutModifier()
						}
					})
				);

			AvatarExplosion = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 400,
						Duration = 1,
						EmissionRate = Int32.MaxValue,
						LiftetimeRange = new Range<float>(1.8f, 2.1f),
						ScaleRange = 1,
						SpeedRange = new Range<float>(600, 1000),
						Texture = AssetBundle.LineParticle,
						Modifiers =
						{
							fadeOutModifier,
							velocityOrientationModifier,
							speedModifier,
							new VelocityScaleModifier(0.4f, 1, 150, -1f, 0),
							new ParticleReflectionModifier(gameSession.Level)
						}
					})
				);

			Bullet = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 100,
						Duration = Single.PositiveInfinity,
						EmissionRate = 200,
						LiftetimeRange = new Range<float>(0.2f, 0.4f),
						ScaleRange = new Range<float>(1.3f, 1.7f),
						SpeedRange = Game.MiniGunTemplate.Speed,
						Texture = AssetBundle.LineParticle,
						Modifiers =
						{
							fadeOutModifier,
							speedModifier,
							new ScaleModifier(-2)
						}
					})
				);

			BulletExplosion = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 25,
						Duration = 0.1f,
						EmissionRate = 200,
						LiftetimeRange = new Range<float>(0.2f, 0.5f),
						ScaleRange = 1,
						SpeedRange = new Range<float>(500, 700),
						Texture = AssetBundle.LineParticle,
						Modifiers =
						{
							fadeOutModifier,
							velocityOrientationModifier,
							speedModifier,
							new VelocityScaleModifier(0.4f, 1, 150, -1f, 0),
							new ParticleReflectionModifier(gameSession.Level)
						}
					})
				);

			Collectible = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 200,
						Duration = 0.1f,
						EmissionRate = Int32.MaxValue,
						LiftetimeRange = new Range<float>(0.2f, 0.5f),
						ScaleRange = 1,
						SpeedRange = new Range<float>(600, 800),
						Texture = AssetBundle.LineParticle,
						Modifiers =
						{
							fadeOutModifier,
							velocityOrientationModifier,
							speedModifier,
							new VelocityScaleModifier(0.4f, 1, 150, -1f, 0),
							new ParticleReflectionModifier(gameSession.Level)
						}
					})
				);

			Regeneration = new ParticleEffectTemplate(effect =>
				effect.Emitters.Add(
					new Emitter
					{
						Capacity = 100,
						Duration = Single.PositiveInfinity,
						EmissionRate = 20,
						LiftetimeRange = new Range<float>(1, 1.3f),
						ScaleRange = 0.7f,
						SpeedRange = new Range<float>(100, 200),
						Direction = new Range<float>(MathUtils.PiOver2 - 0.45f, MathUtils.PiOver2 + 0.45f),
						ColorRange = Colors.Green,
						Texture = AssetBundle.HealthParticle,
						Modifiers =
						{
							fadeOutModifier
						}
					})
				);
		}

		/// <summary>
		///   Disposes the object, releasing all managed and unmanaged resources.
		/// </summary>
		protected override void OnDisposing()
		{
			AvatarExplosion.SafeDispose();
			AvatarCore.SafeDispose();
			BulletExplosion.SafeDispose();
			Bullet.SafeDispose();
			Collectible.SafeDispose();
			Regeneration.SafeDispose();
		}
	}
}